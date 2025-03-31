using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace AlienTek_Firmware_Upgrade_Tool;
internal sealed class FieldSet : ContentControl {
	/// <summary>
	/// Registers a dependency property as backing store for the Content property
	/// </summary>
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register("Header", typeof(string), typeof(FieldSet),
		new FrameworkPropertyMetadata(null));
	/// <summary>
	/// Gets or sets the Content.
	/// </summary>
	/// <value>The Content.</value>
	public string Header {
		get => (string)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}
	private Binding CreateBindingTo(string target) {
		var ret = new Binding(target) {
			Source = this,
			Mode = BindingMode.OneWay
		};
		return ret;
	}
	protected override void OnInitialized(EventArgs e) {
		base.OnInitialized(e);
		var grid = new Grid();
		grid.SetBinding(Panel.BackgroundProperty, CreateBindingTo("Background"));
		var b = new Border() {
			Padding = new Thickness(0, 6.5, 0, 0),
			Margin = new Thickness(0, 10, 0, 0),
			CornerRadius = new CornerRadius(6),
			BorderThickness = new Thickness(2)
		};
		b.SetBinding(Border.BorderBrushProperty, CreateBindingTo("BorderBrush"));
		var cp = new ContentPresenter {
			Content = Content,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
		b.Child = cp;
		var lb = new Label() {
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Left,
			Padding = new Thickness(5, 1, 5, 1),
			Margin = new Thickness(10, 0, 20, 0)
		};
		lb.SetBinding(ContentProperty, CreateBindingTo("Header"));
		lb.SetBinding(ForegroundProperty, CreateBindingTo("Foreground"));
		lb.SetBinding(BackgroundProperty, CreateBindingTo("Background"));
		grid.Children.Add(b);
		grid.Children.Add(lb);
		Content = grid;
	}
}
