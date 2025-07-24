using System.Runtime.CompilerServices;

namespace SystrayShortcuts
{

    internal class TrayMain
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            
            InitTrayMenu();
            trayIcon = new NotifyIcon()
            {
                Text = "Systray shortcuts",
                Icon = Properties.Resources.ApplicationIcon,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
        }

        private void InitTrayMenu()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Add folder...", Properties.Resources.AddFolderImage, (sender, args) => AddFolder());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (sender, args) => Application.Exit());
        }

        internal void AddFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            TrayFolder tf = new TrayFolder(fbd.SelectedPath);
            ToolStripMenuItem folderItem = tf.GetFolderItem();
            // Add row to remove it
            AddRemovalRow(folderItem);
            // TODO: Stort items instead of just inserting
            trayMenu.Items.Insert(2, folderItem);
        }

        private void AddRemovalRow(ToolStripMenuItem menuItem)
        {
            menuItem.DropDownItems.Add(new ToolStripSeparator());
            menuItem.DropDownItems.Add("Remove",
                Properties.Resources.RemoveImage,
                (sender, args) =>
                {
                    trayMenu.Items.Remove(menuItem);
                });

        }
    }
}
