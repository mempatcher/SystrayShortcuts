namespace TaskbarShortcuts
{
    public partial class TrayForm : Form
    {
        private NotifyIcon trayIcon;

        public TrayForm()
        {
            //InitializeComponent();

            trayIcon = new NotifyIcon()
            {
                Text = "Taskbar shortcuts",
                Icon = Properties.Resources.ApplicationIcon,
                Visible = true
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }
    }
}
