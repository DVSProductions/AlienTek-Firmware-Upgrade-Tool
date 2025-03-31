using ATK_DAPWinUSB;
using System.Windows;
using System.Windows.Controls;
using T12Test.Model;
using T12Test.NewUpdateFw;

namespace AlienTek_Firmware_Upgrade_Tool;
/// <summary>
/// Interaction logic for Device_Selector.xaml
/// </summary>
internal partial class DeviceSelector : Page {
	private readonly State State = ((MainWindow)Application.Current.MainWindow).State;
	public DeviceSelector() {
		State.HidManager = new HIDManager(DeviceManager);
		State.Cmd = new GetOrSetParameter(State.HidManager);
		InitializeComponent();
		BtRefresh_Click(null!, null!);
		lvDevices.SelectionChanged += LvDevices_SelectionChanged;
	}
	internal HIDeviceManager DeviceManager { get; private set; } = new HIDeviceManager();
	public bool IsSerailPortClose { get; private set; }

	private DevModel? selectedDevice;
	private DevModel[] FoundDevices = [];
	private async void BtRefresh_Click(object sender, RoutedEventArgs e) {
		btRefresh.IsEnabled = false;
		if(selectedDevice != null && !DeviceManager.IsConnected()) {
			DeviceManager.ConnDev(selectedDevice);
			lvDevices.SelectedIndex = -1;
			btRefresh.IsEnabled = true;
			return;
		}
		FoundDevices = [.. DeviceManager.ScanDev()];
		if(FoundDevices.Length > 0) {
			foreach(var dev in FoundDevices) {
				DeviceManager.ConnDev(dev);
				await RefreshDevModel(dev);
				lvDevices.ItemsSource = null;
				lvDevices.ItemsSource = FoundDevices;
			}
		}
		lvDevices.ItemsSource = FoundDevices;
		btRefresh.IsEnabled = true;
	}
	private async void LvDevices_SelectionChanged(object sender, SelectionChangedEventArgs e) {
		var delta = e.AddedItems.Count - e.RemovedItems.Count;
		if(delta <= 0) {
			selectedDevice = null;
			btNext.IsEnabled = false;
			return;
		}
		if(lvDevices.SelectedItems.Count == 1) {
			selectedDevice = FoundDevices[lvDevices.SelectedIndex];
			DeviceManager.ConnDev(selectedDevice);
			await RefreshDevModel(selectedDevice);
			btNext.IsEnabled = true;
		}
	}
	private async Task<bool> RefreshDevModel(DevModel dev) {
		if(DeviceManager.IsConnected() && State.Cmd != null) {
			var (status, devInfo) = await State.Cmd.GetDevInfo();
			if(status == ApplicationStatus.STATUS_OK) {
				dev.Name = Utils.GetString(devInfo!.Value.dev_type);
				return true;
			}
			MessageBox.Show(UI.GetDeviceInfoError);
			return false;
		}
		return false;
	}
	private void BtNext_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SelectUpgradeFile());
}
