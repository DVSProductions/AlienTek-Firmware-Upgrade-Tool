namespace USB_HID;

internal class HidDeviceInfo(ushort vID, ushort pID, ushort ver, string serialString, string path) {
	public ushort venderID = vID;

	public ushort productID = pID;

	public string serialNumber = serialString;

	public ushort protocolVersionNumber = ver;

	public string devicePath = path;
}
