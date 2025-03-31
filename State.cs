using T12Test.NewUpdateFw;

namespace AlienTek_Firmware_Upgrade_Tool;
internal enum FirmwareSource {
	Local,
	Remote
}
internal class State {
	public FirmwareSource FirmwareSource { get; set; } = FirmwareSource.Remote;
	public string FilePath { get; set; } = string.Empty;
	public HIDManager? HidManager { get; set; }
	public GetOrSetParameter? Cmd { get; set; }
}
