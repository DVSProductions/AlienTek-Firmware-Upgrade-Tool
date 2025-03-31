using System.Runtime.InteropServices;

namespace T12Test.NewUpdateFw;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct FirmInfo {
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	public byte[] dev_type;

	public byte dataLen;

	public byte encrypt_pos;

	public ushort app_addr;

	public ushort app_ver;

	public ushort bin_crc16;

	public uint bin_size;

	public ushort year;

	public byte month;

	public byte day;
}
