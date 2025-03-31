using AlienTek_Firmware_Upgrade_Tool;
using System.Windows;
using T12Test.Model;
using USB_HID;

namespace ATK_DAPWinUSB;

internal class HIDeviceManager {
	private readonly Hid HidDriver;

	private readonly HidReportData hd;

	public event Action<byte[]>? DataReceived;

	public HIDeviceManager() {
		HidDriver = new Hid();
		hd = new HidReportData();
		hd.SetReportID(0);
		HidDriver.DataRecieveHandle += ReceiveData;
	}

	public void CloseDev() {
		if(HidDriver.isOpened) {
			HidDriver.CloseDevice();
		}
	}

	public void ConnDev(DevModel model) {
		CloseDev();
		HidDriver.OpenDevice(model.DevicePath);
	}

	public bool IsConnected() => HidDriver.isOpened;

	public IEnumerable<DevModel> ScanDev() {
		CloseDev();
		return ConvertHid(Hid.GetDevices(16701, 8455), DevType.dev1);
	}

	public void SendData(byte[] buff) {
		if(buff.Length <= 64) {
			var array = new byte[65];
			Buffer.BlockCopy(buff, 0, array, 1, buff.Length);
			hd.reportBuffer = array;
			HidDriver.Write(hd);
		}
		else {
			MessageBox.Show(UI.DataAnomalies);
		}
	}

	private void ReceiveData(object? sender, ReportEventArgs e) => DataReceived?.Invoke(e.reportEventBuf);

	private static IEnumerable<DevModel> ConvertHid(IEnumerable<HidDeviceInfo> hidDevices, DevType devType = DevType.err) {
		return hidDevices.Select(device => new DevModel {
			Name = "ATK_DT65",
			DevType = devType,
			PID = device.productID,
			Vid = device.venderID,
			DevicePath = device.devicePath,
			ProtocolType = DevProtocolType.WinHid
		});
	}
}
