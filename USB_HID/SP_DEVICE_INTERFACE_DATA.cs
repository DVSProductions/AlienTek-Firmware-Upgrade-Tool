namespace USB_HID;

internal struct SP_DEVICE_INTERFACE_DATA {
	public int cbSize;

	public Guid interfaceClassGuid;

	public int flags;

	public IntPtr reserved;
}
