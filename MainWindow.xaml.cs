using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Windows.Graphics;

namespace NetworkPriorityManager
{
    public sealed partial class MainWindow : Window
    {
        private List<NetworkInterface> _adapters = new List<NetworkInterface>();
        private AppWindow m_AppWindow = null!;

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            this.SystemBackdrop = new Microsoft.UI.Composition.SystemBackdrops.MicaBackdrop();

            m_AppWindow = this.AppWindow;
            m_AppWindow.SetIcon("favicon.ico");

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = m_AppWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }

            SetFixedWindowSize(540, 380);
            Activated += MainWindow_Activated;
            LoadAdapters();
        }

        private void SetFixedWindowSize(int width, int height)
        {
            m_AppWindow.Resize(new SizeInt32(width, height));
            if (m_AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }
        }

        private void LoadAdapters()
        {
            _adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             !ni.Name.ToLower().Contains("loopback"))
                .ToList();

            AdapterComboBox.Items.Clear();
            foreach (var adapter in _adapters)
            {
                AdapterComboBox.Items.Add(adapter.Name);
            }

            if (AdapterComboBox.Items.Count > 0)
                AdapterComboBox.SelectedIndex = 0;
        }

        private void SetPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AdapterComboBox.SelectedIndex == -1)
                {
                    StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                    StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                    return;
                }

                if (!int.TryParse(PriorityTextBox.Text, out int metric) || metric < 0)
                {
                    StatusTextBlock.Text = "⚠️ 优先级必须是非负整数";
                    StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                    return;
                }

                string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                SetAdapterPriority(adapterName, metric);
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
            }
        }

        private void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AdapterComboBox.SelectedIndex == -1)
                {
                    StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                    StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                    return;
                }

                string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                SetAdapterPriority(adapterName, 0);
                PriorityTextBox.Text = "10";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
            }
        }

        private void SetAdapterPriority(string adapterName, int metric)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ipv4 set interface \"{adapterName}\" metric={metric}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        StatusTextBlock.Text = $"✅ {adapterName} 的优先级已设置为 {metric}";
                        StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorSuccessBrush"];
                    }
                    else
                    {
                        StatusTextBlock.Text = $"❌ 错误: {error.Trim()}";
                        StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
                    }
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ 错误: {ex.Message}";
                StatusTextBlock.Foreground = (SolidColorBrush)App.Current.Resources["SystemFillColorCriticalBrush"];
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                TitleBarTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                TitleBarTextBlock.Foreground =
                    (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }
        }
    }
}
