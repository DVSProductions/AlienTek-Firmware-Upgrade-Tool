using ATK_DAPWinUSB;

namespace T12Test.NewUpdateFw;

internal sealed class HIDManager {
	public List<byte> recBufferList;

	internal ManageProtocal ManageProtocal { get; set; }

	public FrameEntity SendEntity { get; set; }

	public FrameEntity RecEntity { get; set; }

	public HIDeviceManager Sp { get; set; }

	public bool IsOpen => Sp.IsConnected();

	public HIDManager(HIDeviceManager sp) {
		Sp = sp;
		recBufferList = [];
		SendEntity = new FrameEntity();
		RecEntity = new FrameEntity();
		ManageProtocal = new ManageProtocal {
			SendEntity = SendEntity,
			RecEntity = RecEntity
		};
		sp.DataReceived += Sp_DataReceived;
	}

	private void Sp_DataReceived(byte[] buff) {
		var array = new byte[64];
		Buffer.BlockCopy(buff, 1, array, 0, 64);
		recBufferList.Clear();
		recBufferList.AddRange(array);
	}

	private bool SendCmd() {
		try {
			var buff = ManageProtocal.ConvertFrameEntityToBytes();
			Sp.SendData(buff);
			return true;
		}
		catch {
			return false;
		}
	}

	public async Task<ApplicationStatus> SendCMDWaitResult(byte cmd, byte seq, byte[] data) {
		recBufferList.Clear();
		SendEntity.DeviceAddr = 251;
		SendEntity.FunctionType = cmd;
		SendEntity.Sequence = seq;
		SendEntity.DataLenght = Convert.ToByte(data.Length);
		SendEntity.Data = data;
		SendEntity.SetCheck();
		SendCmd();
		var num = 0;
		do {
			if(!ManageProtocal.ConvertByteToFrameEntity([.. recBufferList])) {
				await Task.Delay(10);
				num++;
				continue;
			}
			return ApplicationStatus.STATUS_OK;
		}
		while(num <= 100);
		return ApplicationStatus.STATUS_TIMEOUT;
	}

	public void ScanningHIDDev() {
		try {
			if(!Sp.IsConnected()) {
				foreach(var dev in Sp.ScanDev()) {
					Sp.ConnDev(dev);
					return;
				}
			}
		}
		catch(Exception ex) {
			Console.WriteLine(ex.ToString());
		}
	}
}