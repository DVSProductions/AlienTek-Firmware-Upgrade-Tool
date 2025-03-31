using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AlienTek_Firmware_Upgrade_Tool;
/// <summary>
/// Interaction logic for SelectUpgradeFile.xaml
/// </summary>
internal partial class SelectUpgradeFile : Page {
	private readonly State State = ((MainWindow)Application.Current.MainWindow).State;
	public SelectUpgradeFile() => InitializeComponent();

	private void BtRemote_Click(object sender, RoutedEventArgs e) {
		State.FirmwareSource = FirmwareSource.Remote;
		State.FilePath = string.Empty;
		NavigationService.Navigate(new FirmwareUpgradePage());
	}

	private void BtFilePicker_Click(object sender, RoutedEventArgs e) {
		//open file dialog for .atk file
		var openFileDialog = new OpenFileDialog {
			Filter = UI.FileDialogFilename
		};
		if(openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName)) {
			State.FirmwareSource = FirmwareSource.Local;
			State.FilePath = openFileDialog.FileName;
			NavigationService.Navigate(new FirmwareUpgradePage());
		}
	}
}
