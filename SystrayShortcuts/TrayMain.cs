using System.Runtime.CompilerServices;

namespace SystrayShortcuts
{

    internal class TrayMain
    {
        private NotifyIcon mainIcon;
        private List<TrayFolder> trayFolders = [];
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            CreateNotifyIcon("Systray Shortcuts", Properties.Resources.ApplicationIcon, CreateTrayMenu());
        }

        private void CreateNotifyIcon(string name, Icon? icon, ContextMenuStrip contextMenuStrip)
        {
            mainIcon = new NotifyIcon()
            {
                Text = name,
                Icon = icon,
                ContextMenuStrip = contextMenuStrip,
                Visible = true
            };
        }

        private ContextMenuStrip CreateTrayMenu()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Add folder...", Properties.Resources.AddFolderImage, (sender, args) => AddFolder());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (sender, args) => Application.Exit());
            return trayMenu;
        }

        internal void AddFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            TrayFolder tf = new TrayFolder(fbd.SelectedPath);
            trayFolders.Add(tf);

            ToolStripMenuItem folderItem = tf.FolderItem;
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
                    foreach (var folder in trayFolders.Where(f => f.FolderItem == menuItem).ToList())
                    {
                        folder.Dispose();
                        trayFolders.Remove(folder);
                    }
                });

        }
    }
}
