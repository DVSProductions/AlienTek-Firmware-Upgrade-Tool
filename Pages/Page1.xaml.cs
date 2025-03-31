using System.Windows.Controls;

namespace AlienTek_Firmware_Upgrade_Tool.Pages;
/// <summary>
/// Interaction logic for Page1.xaml
/// </summary>
internal partial class Page1 : Page {
	public Page1() {
		InitializeComponent();
	}

	private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
		NavigationService.Navigate(new Page2());
	}
}
