using System.Runtime.InteropServices;

namespace T12Test.NewUpdateFw;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DevInfo {
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] dev_type;

	public ushort hdw_ver;

	public ushort app_ver;

	public ushort boot_ver;

	public ushort run_area;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
	public byte[] dev_sn;

	public ushort year;

	public byte month;

	public byte day;
}
