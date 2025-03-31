using System.IO;
using System.Windows;
using System.Windows.Controls;
using T12Test.NewUpdateFw;

namespace AlienTek_Firmware_Upgrade_Tool;
/// <summary>
/// Interaction logic for FirmwareUpgradePage.xaml
/// </summary>
internal partial class FirmwareUpgradePage : Page {
	private readonly State State = ((MainWindow)Application.Current.MainWindow).State;
	private ATKFile? atkFile;

	public FirmwareUpgradePage() => InitializeComponent();
	private void EnsureExit() {
		btFlash.IsEnabled = true;
		((MainWindow)Application.Current.MainWindow).PreventNavigation = false;
	}
	private async void BtFlash_Click(object sender, RoutedEventArgs e) {
		if(State.FirmwareSource == FirmwareSource.Local && (string.IsNullOrWhiteSpace(State.FilePath) || !File.Exists(State.FilePath))) {
			AddLogLine(UI.FirmwareFileNotFound);
			return;
		}
		if(State.HidManager == null || State.Cmd == null || !State.HidManager.IsOpen) {
			AddLogLine(UI.ConnectionNotOpen);
			return;
		}
		btFlash.IsEnabled = false;
		((MainWindow)Application.Current.MainWindow).PreventNavigation = true;
		var (status, devInfoNullable) = await State.Cmd.GetDevInfo();
		if(devInfoNullable is not DevInfo devInfo || status != ApplicationStatus.STATUS_OK) {
			AddLogLine(UI.FailureReceivingDeviceInfo);
			EnsureExit();
			return;
		}
		AddLogLine(UI.LoadingFirmware);
		var connectedDeviceType = Utils.GetString(devInfo.dev_type);
		if(State.FirmwareSource == FirmwareSource.Local) {
			atkFile = new ATKFile(File.ReadAllBytes(State.FilePath));
		}
		else {
			try {
				atkFile = new ATKFile(await Utils.GetUrlFileToBytes(connectedDeviceType == "ATK-PTT80P" ? "http://www.openedv.com/ATK-Prod/ATK-T100/ATK_PTT80P.atk" : "http://www.openedv.com/ATK-Prod/ATK-T100/ATK_PTT80.atk"));
			}
			catch(NullReferenceException) {
				AddLogLine(UI.FirmwareDownloadFailed);
				EnsureExit();
				return;
			}
		}
		if(Utils.GetString(atkFile.FirmInfo.dev_type) != connectedDeviceType) {
			AddLogLine(UI.FirmwareModelMismatch);
			EnsureExit();
			return;
		}
		if(await UpdateFwAsync()) {
			EnsureExit();
			((MainWindow)Application.Current.MainWindow).OnFirmwareUpgradeComplete(true);
		}
		else {
			((MainWindow)Application.Current.MainWindow).OnFirmwareUpgradeComplete(false);
			EnsureExit();
			AddLogLine(UI.FlashingFailed);
		}
	}
	private void AddLogLine(string Message) => Application.Current.Dispatcher.Invoke(()=> {
		tbOutput.Text += Environment.NewLine + Message;
		tbOutput.ScrollToEnd();
	});
	private void UpdatePB(double value) => Application.Current.Dispatcher.Invoke(() => pb.Value = value);
	public async Task<bool> UpdateFwAsync() {
		if(State.Cmd == null || State.HidManager == null || atkFile == null) {
			return false;
		}
		if(!await IntoBootloaderAsync()) {
			AddLogLine(UI.StartAbnormal);
			return false;
		}
		AddLogLine(UI.StartOK);
		if(await State.Cmd.SetFwInfo(atkFile.FirmInfo) != ApplicationStatus.STATUS_OK) {
			AddLogLine(UI.FirmwareInformationAbnormality);
			return false;
		}
		AddLogLine(UI.FirmwareInformationOK);
		if(await State.Cmd.SetStartSendFW() != ApplicationStatus.STATUS_OK) {
			AddLogLine(UI.StartAbnormal);
			return false;
		}
		AddLogLine(UI.StartOK);
		byte frame = 0;
		var resend = 0;
		var dataCount = 0;
		var DataLength = atkFile.FwData.Length;
		var now = DateTime.Now;
		while(true) {
			var dataOfSeq = atkFile.GetDataOfSeq(dataCount);
			if(dataOfSeq.Length == 0) {
				break;
			}
			var status = await State.Cmd.SetSendData(frame, dataOfSeq);
			UpdatePB(dataCount / (double)DataLength);
			if(status != ApplicationStatus.STATUS_OK) {
				resend++;
				if(resend > 3) {
					return false;
				}
#if DEBUG
				AddLogLine("frame:" + frame + " resend" + resend);
#endif
			}
			else {
				frame++;
				resend = 0;
				dataCount += dataOfSeq.Length;
			}
		}
		UpdatePB(100);
#if DEBUG
		AddLogLine("sent: " + dataCount);
		AddLogLine("took: " + (DateTime.Now - now).TotalSeconds + "s");
#endif
		if(await State.Cmd.SetSendDataEnd() != ApplicationStatus.STATUS_OK) {
			AddLogLine(UI.EndInstructionException);
			return false;
		}
		else {
			AddLogLine(UI.EndOK);
		}
		return true;
		async Task<bool> IntoBootloaderAsync() {
			var result = await State.Cmd.GetDevInfo();
			if(result.status != ApplicationStatus.STATUS_OK) {
				AddLogLine(UI.ExceptionGettingDeviceInformation);
				return false;
			}
			AddLogLine(UI.GetDeviceInformationOK);
			if(result.devInfo!.Value.run_area == 187) {
				return true;
			}

			await Task.Delay(500);
			for(var retry = 1; retry <= 10; retry++) {
				State.HidManager.ScanningHIDDev();
				await Task.Delay(500);
				if(State.HidManager.IsOpen) {
					result = await State.Cmd.GetDevInfo();
					if(result.status == ApplicationStatus.STATUS_OK) {
						if(result.devInfo!.Value.run_area == 187) {
							return true;
						}
						await State.Cmd.SetBoot();
					}
					else {
						AddLogLine(UI.GetDeviceStatus + retry);
					}
				}
				else {
					AddLogLine(UI.BootloaderReconnecting + retry);
				}
			}
			return false;
		}
	}
}
