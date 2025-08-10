using System.Runtime.InteropServices;
using System.Text;

namespace SystrayShortcuts
{

    public class TrayMain : ApplicationContext
    {
        private NotifyIcon mainIcon;
        private List<TrayFolder> trayFolders = [];

        public TrayMain()
        {
            Settings.Load(ref trayFolders);
            Initialize();
            foreach (TrayFolder folder in trayFolders)
            {
                AddTrayMenuOptions(folder);
                folder.CreateNotifyIcon();
            }

        }

        private void Initialize()
        {
            mainIcon = new NotifyIcon()
            {
                Text = "Systray Shortcuts",
                Icon = Properties.Resources.ApplicationIcon,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true
            };
            DrawContextMenu();
        }

        private void DrawContextMenu()
        {
            var contextMenu = mainIcon.ContextMenuStrip ?? new ContextMenuStrip();

            contextMenu.Items.Clear();
            contextMenu.Items.Add("Add folder...", Properties.Resources.AddFolderImage, (sender, args) => AddFolder());

            // Add the created folders under main icon
            if (trayFolders.Count > 0)
            {
                contextMenu.Items.Add(new ToolStripSeparator());
                foreach (TrayFolder folder in trayFolders.OrderBy(f => f.Name))
                {
                    contextMenu.Items.Add(folder.FolderItem);
                }
            }

            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (sender, args) => Exit());
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
            DrawContextMenu();
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
                    mainIcon.ContextMenuStrip?.Items.Remove(trayItem.FolderItem);
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
