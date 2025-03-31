using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace AlienTek_Firmware_Upgrade_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
internal partial class MainWindow : Window {
	internal State State { get; set; } = new State();
	public MainWindow() {
		InitializeComponent();
	}

	private Duration AnimationDuration = new(TimeSpan.FromSeconds(1));
	public void OnFirmwareUpgradeComplete(bool success) {
		OnSuccess.Visibility = success ? Visibility.Visible : Visibility.Collapsed;
		OnFail.Visibility = success ? Visibility.Collapsed : Visibility.Visible;
		var blurEffect = new BlurEffect() {
			Radius = 1
		};
		NavigationGrid.Effect = blurEffect;

		var animation = new DoubleAnimation() {
			From = 1,
			To = 20,
			Duration = AnimationDuration
		};
		FinishOverlay.Opacity = 0;
		FinishOverlay.Visibility = Visibility.Visible;
		var fadeAnimation = new DoubleAnimation() {
			From = 0,
			To = 1,
			Duration = AnimationDuration
		};
		blurEffect.BeginAnimation(BlurEffect.RadiusProperty, animation);
		FinishOverlay.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
	}

	private void Button_Click(object sender, RoutedEventArgs e) {
		while(contentFrame.NavigationService.CanGoBack)
			contentFrame.NavigationService.GoBack();
		FinishOverlay.Visibility = Visibility.Collapsed;
		NavigationGrid.Effect = null;
	}
	public bool PreventNavigation { get; set; }
	private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e) {
		if(PreventNavigation) {
			e.Cancel = true;
			return;
		}
		if(e.NavigationMode == NavigationMode.Forward) {
			e.Cancel = true;
		}
	}

	private void Button_Click_1(object sender, RoutedEventArgs e) {
		FinishOverlay.Visibility = Visibility.Collapsed;
		NavigationGrid.Effect = null;
	}

	private void Button_Click_2(object sender, RoutedEventArgs e) => contentFrame.NavigationService.GoBack();

	private void ContentFrame_Navigated(object sender, NavigationEventArgs e) => btBack.IsEnabled = contentFrame.NavigationService.CanGoBack;

	private void Button_Click_3(object sender, RoutedEventArgs e) {
		new About() { Owner = this }.ShowDialog();
	}

	private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
		e.Cancel = PreventNavigation;
	}
}