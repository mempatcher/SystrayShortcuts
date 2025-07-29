using System.Runtime.InteropServices;
using System.Text;

namespace SystrayShortcuts
{

    internal class TrayMain
    {
        private NotifyIcon mainIcon;
        private List<TrayFolder> trayFolders = [];
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            trayMenu = new ContextMenuStrip();
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
            trayMenu.Items.Clear();
            trayMenu.Items.Add("Add folder...", Properties.Resources.AddFolderImage, (sender, args) => AddFolder());
            if (trayFolders.Count > 0)
            {
                trayMenu.Items.Add(new ToolStripSeparator());
                foreach (TrayFolder folder in trayFolders.OrderBy(f => f.Name))
                {
                    trayMenu.Items.Add(folder.FolderItem);
                }
            }

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

            tf.FolderItem.DropDownItems.Add(new ToolStripSeparator());
            AddChangeIconRow(tf);
            AddRemovalRow(tf);

            // Redraw the tray menu
            CreateTrayMenu();

        }

        public bool ShowPickIconDialog(ref string iconPath, ref int iconIndex)
        {
            if (string.IsNullOrEmpty(iconPath))
            { 
                iconPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\shell32.dll"); ;
            }
            StringBuilder path = new StringBuilder(260);
            path.Append(iconPath); // Default to this one
            bool result = PickIconDlg(IntPtr.Zero, path, (uint)path.Capacity, ref iconIndex);

            if (result)
            {
                iconPath = path.ToString();
            }

            return result;
        }

        private void AddChangeIconRow(TrayFolder trayItem)
        {
            trayItem.FolderItem.DropDownItems.Add(
                "Change icon...",
                null,
                (sender, args) =>
                {
                    string iconPath = trayItem.IconPath;
                    int iconIndex = trayItem.IconIndex;
                    if (ShowPickIconDialog(ref iconPath, ref iconIndex))
                    {
                        trayItem.SetIcon(iconPath, iconIndex);
                    }
                });
        }


        private void AddRemovalRow(TrayFolder trayItem)
        {
            trayItem.FolderItem.DropDownItems.Add(
                "Remove",
                Properties.Resources.RemoveImage,
                (sender, args) =>
                {
                    trayMenu.Items.Remove(trayItem.FolderItem);
                    foreach (var folder in trayFolders.Where(f => f.FolderItem == trayItem.FolderItem).ToList())
                    {
                        folder.Dispose();
                        trayFolders.Remove(folder);
                    }
                });
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool PickIconDlg(IntPtr hwndOwner, StringBuilder pszIconPath, uint cchIconPath, ref int piIconIndex);
        }
}
