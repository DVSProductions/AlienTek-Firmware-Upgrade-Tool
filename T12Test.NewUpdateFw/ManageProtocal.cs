using AlienTek_Firmware_Upgrade_Tool;

namespace T12Test.NewUpdateFw;

internal class ManageProtocal {
	private readonly int minLen;

	public required FrameEntity SendEntity { get; set; }

	public required FrameEntity RecEntity { get; set; }

	public byte[] ConvertFrameEntityToBytes() {
		var array = new byte[SendEntity.DataLenght + minLen];
		var offset = 0;
		Utils.ValueToArray((object)array, SendEntity.DeviceAddr, ref offset);
		Utils.ValueToArray((object)array, SendEntity.FunctionType, ref offset);
		Utils.ValueToArray((object)array, SendEntity.Sequence, ref offset);
		Utils.ValueToArray((object)array, SendEntity.DataLenght, ref offset);
		Utils.ValueToArray(array, SendEntity.Data, ref offset);
		SendEntity.SetCheck();
		Utils.ValueToArray(array, SendEntity.Check, ref offset);
		return array;
	}

	public bool ConvertByteToFrameEntity(byte[] rData) {
		if(rData.Length < minLen) {
			return false;
		}
		try {
			RecEntity.DeviceAddr = rData[0];
			RecEntity.FunctionType = rData[1];
			RecEntity.Sequence = rData[2];
			RecEntity.DataLenght = rData[3];
			if(rData.Length >= RecEntity.DataLenght + minLen) {
				RecEntity.Data = Utils.GetSubArray(rData, 4, RecEntity.DataLenght);
				RecEntity.Check = Utils.ByteArrayToUInt16(rData, RecEntity.DataLenght + 4);
				return true;
			}
			return false;
		}
		catch(Exception) {
			return false;
		}
	}

	public ManageProtocal() => minLen = 6;
}
