namespace SystrayShortcuts
{
    
    internal class TrayMain
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Add folder...", Properties.Resources.AddFolderImage, (obj,e) => AddFolder());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (obj, e) => Application.Exit());

            trayIcon = new NotifyIcon()
            {
                Text = "Systray shortcuts",
                Icon = Properties.Resources.ApplicationIcon,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
        }

        internal void AddFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            TrayFolder tf = new TrayFolder(fbd.SelectedPath);
            // TODO: Add and sort list instead of inserting
            trayMenu.Items.Insert(1, tf.GetFolderItem());
        }
    }
}
