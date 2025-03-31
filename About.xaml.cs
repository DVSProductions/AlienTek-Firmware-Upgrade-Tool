﻿using System.Diagnostics;
using System.Windows;

namespace AlienTek_Firmware_Upgrade_Tool;
/// <summary>
/// Interaction logic for About.xaml
/// </summary>
public partial class About : Window {
	public About() {
		InitializeComponent();
	}

	private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		e.Handled = true;

	}
}
