using System.Diagnostics.CodeAnalysis;

namespace T12Test.NewUpdateFw;

internal class FrameEntity {
	public byte DeviceAddr { get; set; }

	public byte FunctionType { get; set; }

	public byte Sequence { get; set; }

	public byte DataLenght { get; set; }
	[NotNull]
	public byte[]? Data { get; set; }

	public ushort Check { get; set; }
	public ushort CalculateCheck() {
		var list = new List<byte> {
			DeviceAddr,
			FunctionType,
			Sequence,
			DataLenght
		};
		list.AddRange(Data);
		return CRC16([.. list]);
	}

	public void SetCheck() => Check = CalculateCheck();

	public static ushort CRC16(byte[] srcData) {
		var checksum = ushort.MaxValue;
		ushort xor = 40961;
		for(var n = 0; n < srcData.Length; n++) {
			checksum ^= srcData[n];
			for(var i = 0; i < 8; i++) {
				checksum = ((checksum & 1) != 0) ? Convert.ToUInt16((checksum >> 1) ^ xor) : Convert.ToUInt16(checksum >> 1);
			}
		}
		return checksum;
	}
}