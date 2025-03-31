using System.Runtime.InteropServices;

namespace USB_HID;

[StructLayout(LayoutKind.Sequential)]
internal sealed class SP_DEVINFO_DATA {
	public int cbSize;

	public Guid classGuid;

	public int devInst;

	public int reserved;

	public SP_DEVINFO_DATA() {

		cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>();
		classGuid = Guid.Empty;

	}
}
