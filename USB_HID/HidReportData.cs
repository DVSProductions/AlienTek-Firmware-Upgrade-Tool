namespace USB_HID;

internal sealed class HidReportData {
	public byte[] reportBuffer;

	public byte[] SetReportID(byte reportID) {
		reportBuffer[0] = reportID;
		return reportBuffer;
	}

	public HidReportData() => reportBuffer = new byte[65];

}
