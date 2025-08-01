using System.Runtime.InteropServices;
using System.Text;

namespace SystrayShortcuts
{

    public class TrayMain : ApplicationContext
    {
        private NotifyIcon mainIcon;
        private List<TrayFolder> trayFolders = [];
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            trayMenu = new ContextMenuStrip();
            Settings.Load(ref trayFolders);
            CreateNotifyIcon("Systray Shortcuts", Properties.Resources.ApplicationIcon, CreateTrayMenu());
            foreach (TrayFolder folder in trayFolders)
            {
                AddTrayMenuOptions(folder);
                folder.CreateNotifyIcon();
            }

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
            trayMenu.Items.Add("Exit", null, (sender, args) => Exit());
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
            AddToTrayMenu(tf);
            tf.CreateNotifyIcon();
            Settings.Update(trayFolders);
        }

        private void AddToTrayMenu(TrayFolder folder)
        {
            trayFolders.Add(folder);
            AddTrayMenuOptions(folder);
            // Redraw the tray menu
            CreateTrayMenu();
        }

        private void AddTrayMenuOptions(TrayFolder folder)
        {
            folder.FolderItem.DropDownItems.Add(new ToolStripSeparator());
            AddChangeIconRow(folder);
            AddRemovalRow(folder);
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
                    Settings.Update(trayFolders);
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
                        // Update settings file
                        Settings.Update(trayFolders);
                    }
                });
        }

        private void Exit()
        {
            foreach (TrayFolder folder in trayFolders)
            {
                folder.Dispose();
                mainIcon.Dispose();
                Application.Exit();
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool PickIconDlg(IntPtr hwndOwner, StringBuilder pszIconPath, uint cchIconPath, ref int piIconIndex);
    }
}
