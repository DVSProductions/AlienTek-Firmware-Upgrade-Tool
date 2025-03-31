namespace USB_HID;

internal sealed class ReportEventArgs(byte[] reportBuf) : EventArgs {
	public byte[] reportEventBuf = reportBuf;
}
