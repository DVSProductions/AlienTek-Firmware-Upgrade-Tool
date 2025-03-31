using AlienTek_Firmware_Upgrade_Tool;

namespace T12Test.Model;

internal sealed class DevModel {
	public required string Name { get; set; }

	internal DevType DevType { get; set; }

	public ushort PID { get; set; }

	public ushort Vid { get; set; }

	public required string DevicePath { get; set; }

	public DevProtocolType ProtocolType { get; set; }
	internal DevStatus Status { get; set; }

	public byte DevModeVal { get; set; }
	public string GetStatusInfo() => $"{UI.ConntectedAs} {DevType} {UI.via} {ProtocolType}";
	public string StatusInfo => GetStatusInfo();
}
