using System.Windows.Controls;

namespace AlienTek_Firmware_Upgrade_Tool.Pages;
/// <summary>
/// Interaction logic for Page2.xaml
/// </summary>
internal partial class Page2 : Page {
	public Page2() {
		InitializeComponent();
	}

	private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
		NavigationService.Navigate(new DeviceSelector());
	}
}
