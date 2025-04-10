using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace NetworkPriorityManager
{
    public partial class Form1 : Form
    {
        private List<NetworkInterface> _adapters = new List<NetworkInterface>();

        public Form1()
        {
            InitializeComponent();
            LoadAdapters();
        }

        private void LoadAdapters()
        {
            // 获取所有处于活动状态的网络适配器
            _adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             !ni.Name.ToLower().Contains("loopback"))
                .ToList();

            // 填充下拉框
            comboBoxAdapter.Items.Clear();
            foreach (var adapter in _adapters)
            {
                comboBoxAdapter.Items.Add(adapter.Name);
            }

            if (comboBoxAdapter.Items.Count > 0)
                comboBoxAdapter.SelectedIndex = 0;
        }

        private void buttonSetPriority_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxAdapter.SelectedIndex == -1)
                {
                    statusLabel.Text = "⚠️ 请选择一个网络适配器";
                    statusLabel.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (!int.TryParse(textBoxPriority.Text, out int metric) || metric < 0)
                {
                    statusLabel.Text = "⚠️ 优先级必须是非负整数";
                    statusLabel.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string adapterName = _adapters[comboBoxAdapter.SelectedIndex].Name;
                SetAdapterPriority(adapterName, metric);
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"⚠️ 错误: {ex.Message}";
                statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void buttonRestoreDefault_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxAdapter.SelectedIndex == -1)
                {
                    statusLabel.Text = "⚠️ 请选择一个网络适配器";
                    statusLabel.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                string adapterName = _adapters[comboBoxAdapter.SelectedIndex].Name;
                SetAdapterPriority(adapterName, 0); // 0表示系统自动分配
                textBoxPriority.Text = "10"; // 恢复默认值
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"⚠️ 错误: {ex.Message}";
                statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void SetAdapterPriority(string adapterName, int metric)
        {
            try
            {
                // 执行netsh命令
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
                        statusLabel.Text = $"✅ {adapterName} 的优先级已设置为 {metric}";
                        statusLabel.ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        statusLabel.Text = $"❌ 错误: {error.Trim()}";
                        statusLabel.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"❌ 错误: {ex.Message}";
                statusLabel.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}