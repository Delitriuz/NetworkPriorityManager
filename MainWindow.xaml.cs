using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Windows.Graphics;

namespace NetworkPriorityManager
{
    public sealed partial class MainWindow : Window
    {
        private List<NetworkInterface> _adapters = new List<NetworkInterface>();
        private AppWindow _appWindow = null!;

        private static string LogPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NetworkPriorityManager",
            "log.txt");

        private static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogPath, line);
            }
            catch { /* logging must not throw */ }
        }

        private static Brush? GetBrush(string key)
        {
            if (App.Current.Resources.TryGetValue(key, out object? value) && value is Brush brush)
            {
                return brush;
            }
            return null;
        }

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

            Log("=== Application started ===");
            Log($"OS: {Environment.OSVersion}");
            Log($"IsAdmin: {new System.Security.Principal.WindowsPrincipal(System.Security.Principal.WindowsIdentity.GetCurrent()).IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator)}");
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
            try
            {
                _adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 !ni.Name.Contains("loopback", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                AdapterComboBox.Items.Clear();
                foreach (var adapter in _adapters)
                {
                    AdapterComboBox.Items.Add(adapter.Name);
                    Log($"Adapter found: {adapter.Name}");
                }

                if (AdapterComboBox.Items.Count > 0)
                    AdapterComboBox.SelectedIndex = 0;

                Log($"Total adapters loaded: {_adapters.Count}");
            }
            catch (Exception ex)
            {
                Log($"LoadAdapters ERROR: {ex}");
            }
        }

        private void SetPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            Log("SetPriorityButton_Click");
            try
            {
                if (AdapterComboBox.SelectedIndex == -1)
                {
                    StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                    StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
                    Log("No adapter selected");
                    return;
                }

                if (!int.TryParse(PriorityTextBox.Text, out int metric) || metric < 0)
                {
                    StatusTextBlock.Text = "⚠️ 优先级必须是非负整数";
                    StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
                    Log($"Invalid metric input: '{PriorityTextBox.Text}'");
                    return;
                }

                string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                Log($"Setting priority: adapter='{adapterName}', metric={metric}");
                SetAdapterPriority(adapterName, metric);
            }
            catch (Exception ex)
            {
                Log($"SetPriorityButton_Click EXCEPTION: {ex}");
                StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
            }
        }

        private void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Log("RestoreDefaultButton_Click");
            try
            {
                if (AdapterComboBox.SelectedIndex == -1)
                {
                    StatusTextBlock.Text = "⚠️ 请选择一个网络适配器";
                    StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
                    Log("No adapter selected");
                    return;
                }

                string adapterName = _adapters[AdapterComboBox.SelectedIndex].Name;
                Log($"Restoring default: adapter='{adapterName}'");
                SetAdapterPriority(adapterName, 0);
                PriorityTextBox.Text = "10";
            }
            catch (Exception ex)
            {
                Log($"RestoreDefaultButton_Click EXCEPTION: {ex}");
                StatusTextBlock.Text = $"⚠️ 错误: {ex.Message}";
                StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
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

                Log($"ProcessStartInfo: FileName={startInfo.FileName}, Arguments={startInfo.Arguments}");

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    Log($"ExitCode={process.ExitCode}, Output='{output}', Error='{error}'");

                    if (process.ExitCode == 0)
                    {
                        StatusTextBlock.Text = $"✅ {adapterName} 的优先级已设置为 {metric}";
                        StatusTextBlock.Foreground = GetBrush("SystemFillColorSuccessBrush") ?? new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        StatusTextBlock.Text = $"❌ 错误: {error.Trim()}";
                        StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"SetAdapterPriority EXCEPTION: {ex}");
                StatusTextBlock.Text = $"❌ 错误: {ex.Message}";
                StatusTextBlock.Foreground = GetBrush("SystemFillColorCriticalBrush") ?? new SolidColorBrush(Colors.Red);
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                TitleBarTextBlock.Foreground =
                    GetBrush("WindowCaptionForegroundDisabled") ?? new SolidColorBrush(Colors.Gray);
            }
            else
            {
                TitleBarTextBlock.Foreground =
                    GetBrush("WindowCaptionForeground") ?? new SolidColorBrush(Colors.Black);
            }
        }
    }
}
