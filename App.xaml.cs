using Microsoft.UI.Xaml;

namespace NetworkPriorityManager
{
    public partial class App : Application
    {
        private Window? m_window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _ = args;
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
