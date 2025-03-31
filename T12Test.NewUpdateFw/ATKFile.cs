using AlienTek_Firmware_Upgrade_Tool;

namespace T12Test.NewUpdateFw;

internal sealed class ATKFile {
	private FirmInfo firmInfo;

	public byte[] FileBuffer { get; set; }

	public byte[] FwData { get; set; }

	internal FirmInfo FirmInfo {

		get => firmInfo;

		set => firmInfo = value;
	}

	public ATKFile(byte[] buffer) {
		FileBuffer = buffer;
#pragma warning disable CS8605 // Unboxing a possibly null value.
		firmInfo = (FirmInfo)Utils.BytesToStuct(buffer, typeof(FirmInfo), 0);
#pragma warning restore CS8605 // Unboxing a possibly null value.
		var num = buffer.Length - Utils.GetStuctSize(typeof(FirmInfo));
		FwData = new byte[num];
		Buffer.BlockCopy(buffer, Utils.GetStuctSize(typeof(FirmInfo)), FwData, 0, FwData.Length);
	}

	public byte[] GetDataOfSeq(int dataCount) {
		if(dataCount == FwData.Length) {
			return [];
		}
		var array = (dataCount + firmInfo.dataLen > FwData.Length) ? new byte[FwData.Length - dataCount] : new byte[firmInfo.dataLen];
		Buffer.BlockCopy(FwData, dataCount, array, 0, array.Length);
		return array;
	}
}
