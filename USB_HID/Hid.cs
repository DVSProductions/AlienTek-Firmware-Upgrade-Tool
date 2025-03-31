using AlienTek_Firmware_Upgrade_Tool;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace USB_HID;

internal class Hid : IDisposable {
	[StructLayout(LayoutKind.Explicit)]
	private struct DevBroadcastDeviceInterfaceBuffer(int deviceType) {
		[FieldOffset(0)]
		public int dbch_size = Marshal.SizeOf<DevBroadcastDeviceInterfaceBuffer>();

		[FieldOffset(4)]
		public int dbch_devicetype = deviceType;

		[FieldOffset(8)]
		public int dbch_reserved = 0;
	}

	public bool isOpened;

	public HidReportData reportData;

	private IntPtr hidHandle;

	private FileStream? hidDeviceFileStream;

	public int OutputReportLength { get; private set; }

	public int InputReportLength { get; private set; }

	public event EventHandler<ReportEventArgs>? DataRecieveHandle;

	public Hid() {
		reportData = new HidReportData();
		hidHandle = IntPtr.Zero;
	}

	public static List<HidDeviceInfo> GetHidDevices() {
		var list = new List<HidDeviceInfo>();
		var HidGuid = Guid.Empty;
		HidD_GetHidGuid(ref HidGuid);
		var deviceInfoSet = SetupDiGetClassDevs(ref HidGuid, 0u, IntPtr.Zero, (DIGCF)18);
		var deviceInterfaceData = default(SP_DEVICE_INTERFACE_DATA);
		deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
		for(uint num = 0; num < 64; num++) {
			if(!SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref HidGuid, num, ref deviceInterfaceData)) {
				continue;
			}
			var requiredSize = 0;
			SetupDiGetDeviceInterfaceDetailW(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref requiredSize, null!);
			var UnmanagedMemoryPTR = Marshal.AllocHGlobal(requiredSize);
			if(IntPtr.Size != 4) {
				Marshal.WriteInt32(UnmanagedMemoryPTR, 4 + (Marshal.SystemDefaultCharSize * 2));
			}
			else {
				Marshal.WriteInt32(UnmanagedMemoryPTR, 4 + Marshal.SystemDefaultCharSize);
			}
			if(!SetupDiGetDeviceInterfaceDetailW(deviceInfoSet, ref deviceInterfaceData, UnmanagedMemoryPTR, requiredSize, ref requiredSize, null!)) {
				Marshal.FreeHGlobal(UnmanagedMemoryPTR);
				continue;
			}
			var text = Marshal.PtrToStringAuto(UnmanagedMemoryPTR + 4);
			if(text == null) {
				Marshal.FreeHGlobal(UnmanagedMemoryPTR);
				continue;
			}
			var ptrToHIDFile = CreateFile(text, 3221225472u, 0u, 0u, 3u, 1073741824u, 0u);
			if((uint)ptrToHIDFile == 0xffffffff) {
				Marshal.FreeHGlobal(UnmanagedMemoryPTR);
				continue;
			}
			var attributes = default(HIDD_ATTRIBUTES);
			attributes.Size = Marshal.SizeOf(attributes);
			HidD_GetAttributes(ptrToHIDFile, out attributes);
			var empty = string.Empty;
			var item = new HidDeviceInfo(attributes.VendorID, attributes.ProductID, attributes.VersionNumber, empty, text);
			list.Add(item);
			CloseHandle(ptrToHIDFile);
			Marshal.FreeHGlobal(UnmanagedMemoryPTR);
			continue;
		}
		SetupDiDestroyDeviceInfoList(deviceInfoSet);
		return list;
	}

	public static IEnumerable<HidDeviceInfo> GetDevices(ushort vid, ushort pid) => GetHidDevices().Where(hidDeviceInfo => hidDeviceInfo.venderID == vid && hidDeviceInfo.productID == pid);

	public HidDeviceInfo? OpenDevice(string devicePath) {
		var hidDevice = GetHidDevices();
		foreach(var hidDeviceInfo in hidDevice) {
			if(hidDeviceInfo.devicePath == devicePath) {
				hidHandle = CreateHandle(hidDeviceInfo.devicePath);
				try {
					HidD_GetPreparsedData(hidHandle, out var PreparsedData);
					HidP_GetCaps(PreparsedData, out var Capabilities);
					HidD_FreePreparsedData(PreparsedData);
					OutputReportLength = Capabilities.OutputReportByteLength;
					InputReportLength = Capabilities.InputReportByteLength;
					hidDeviceFileStream = new FileStream(new SafeFileHandle(hidHandle, ownsHandle: false), FileAccess.ReadWrite, InputReportLength, isAsync: true);
					isOpened = true;
					BeginAsyncRead();
					return hidDeviceInfo;
				}
				catch(AccessViolationException ex) {
					MessageBox.Show(ex.ToString());
					continue;
				}
			}
		}
		return null;
	}

	private void BeginAsyncRead() {
		try {
			var array = new byte[InputReportLength];
			hidDeviceFileStream?.BeginRead(array, 0, InputReportLength, ReadCompleted, array);
		}
		catch(IOException ex) {
			MessageBox.Show(ex.ToString());
			isOpened = false;
		}
		catch(Exception ex2) {
			MessageBox.Show(ex2.ToString());
			isOpened = false;
		}
	}

	private void ReadCompleted(IAsyncResult iResult) {
		if(hidDeviceFileStream == null || iResult.AsyncState is not byte[] array)
			return;
		try {
			hidDeviceFileStream.EndRead(iResult);
			for(var i = 0; i < array.Length; i++) {
				reportData.reportBuffer[i] = array[i];
			}
			var e = new ReportEventArgs(reportData.reportBuffer);
			DataRecieveHandle?.Invoke(this, e);
			if(isOpened) {
				BeginAsyncRead();
			}
		}
		catch(Exception) {
			isOpened = false;
		}
	}

	public void CloseDevice() {
		if(isOpened) {
			isOpened = false;
			hidDeviceFileStream?.Close();
			hidDeviceFileStream = null;
			CloseHandle(hidHandle);
			hidHandle = IntPtr.Zero;
		}
	}

	private IntPtr CreateHandle(string lpFileName) {
		hidHandle = CreateFile(lpFileName, 3221225472u, 0u, 0u, 3u, 1073741824u, 0u);
		return hidHandle;
	}

	public void Write(HidReportData writeReport) {
		if(isOpened && hidDeviceFileStream != null) {
			try {
				var array = new byte[OutputReportLength];
				for(var i = 0; i < array.Length; i++) {
					array[i] = writeReport.reportBuffer[i];
				}
				hidDeviceFileStream.Write(array, 0, OutputReportLength);
				return;
			}
			catch(IOException) {
				return;
			}
			catch(Exception ex2) {
				MessageBox.Show(ex2.ToString(), UI.MSGTitleError);
				return;
			}
		}
		MessageBox.Show(UI.DeviceNotConnected, UI.MSGTitleError);
	}

	[DllImport("hid.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern void HidD_GetHidGuid(ref Guid HidGuid);

	[DllImport("setupapi.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, SP_DEVINFO_DATA deviceInfoData);

	[DllImport("hid.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool HidD_GetAttributes(IntPtr hidDeviceObject, out HIDD_ATTRIBUTES attributes);

	[DllImport("hid.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr PreparsedData);

	[DllImport("hid.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

	[DllImport("hid.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern uint HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

	[DllImport("kernel32.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, uint securityAttributes, uint creationDisposition, uint flagsAndAttributes, uint templateFile);
	[DllImport("kernel32.dll")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern int CloseHandle(IntPtr hObject);

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing) {
		if(disposing) {
			// Dispose managed resources
			hidDeviceFileStream?.Dispose();
			hidDeviceFileStream = null;
		}
		// Dispose unmanaged resources
		if(hidHandle != IntPtr.Zero) {
			CloseHandle(hidHandle);
			hidHandle = IntPtr.Zero;
		}
	}
}
