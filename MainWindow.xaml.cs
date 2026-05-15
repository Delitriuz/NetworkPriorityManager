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
        private AppWindow _appWindow = null!;

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SystemBackdrop = new MicaBackdrop();

            _appWindow = AppWindow;
            _appWindow.SetIcon("favicon.ico");

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }

            SetFixedWindowSize(540, 380);
            Activated += MainWindow_Activated;
            LoadAdapters();
        }

        private void SetFixedWindowSize(int width, int height)
        {
            _appWindow.Resize(new SizeInt32(width, height));
            if (_appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }
        }

        private void LoadAdapters()
        {
            _adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             !ni.Name.Contains("loopback", StringComparison.OrdinalIgnoreCase))
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
                if (!System.Text.RegularExpressions.Regex.IsMatch(adapterName, @"^[\w\s\-\.]+$"))
                {
                    throw new ArgumentException("适配器名称包含非法字符", nameof(adapterName));
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ipv4 set interface \"{adapterName}\" metric={metric}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
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
