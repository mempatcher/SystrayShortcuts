using System.Diagnostics;

namespace SystrayShortcuts
{
    internal class TrayFolder : IDisposable
    {
        public ToolStripMenuItem FolderItem { get; private set; }

        public NotifyIcon? TrayIcon { get; private set; }
        public string FolderPath { get; }

        public string Name => Path.GetFileName(FolderPath) ?? "";

        public TrayFolder(string path)
        {
            FolderPath = path;
            FolderItem = CreateFolderStructure(FolderPath);
            CreateNotifyIcon();
        }

        private ToolStripMenuItem CreateFolderStructure(string path)
        {
            ToolStripMenuItem parent = new ToolStripMenuItem()
            {
                Text = Path.GetFileName(path),
                Image = Properties.Resources.FolderClosedImage,
            };
            parent.DropDownItems.Add(CreateFolderHeaderRow(path));
            parent.DropDownItems.Add(new ToolStripSeparator());
            ScanFolder(path, parent);
            return parent;
        }
        private void CreateNotifyIcon()
        {
            var menuItem = new ContextMenuStrip();
            // Need to copy over the items. ToolStripItems can only have one parent
            menuItem.Items.AddRange(CreateFolderStructure(FolderPath).DropDownItems);

            TrayIcon = new NotifyIcon()
            {
                Text = Name,
                Icon = Properties.Resources.ApplicationIcon,
                ContextMenuStrip = menuItem,
                Visible = true
            };
        }

        private void ScanFolder(string path, ToolStripMenuItem parent)
        {
            // Get folders, loop though and add them as menu items
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                // Skip symlinks
                if (new DirectoryInfo(dir).LinkTarget != null) continue;
                parent.DropDownItems.Add(CreateFolderStructure(dir));
            }

            var files = Directory.GetFiles(path);
            // Get files, loop though and add them as menu items
            foreach (var file in files)
            {
                parent.DropDownItems.Add(Path.GetFileName(file),
                    Icon.ExtractAssociatedIcon(file)?.ToBitmap(),
                    (sender, args) => LaunchApp(file));
            }

            // If no directories or files exist in folder, add an informative row.
            if (dirs.Length == 0 && files.Length == 0)
            {
                parent.DropDownItems.Add(
                    new ToolStripLabel()
                    {
                        Text = "Empty",
                        Enabled = false,
                        Font = new Font(SystemFonts.DefaultFont, FontStyle.Italic)
                    });
            }
        }

        private static ToolStripMenuItem CreateFolderHeaderRow(string path)
        {
            ToolStripMenuItem headerItem = new ToolStripMenuItem()
            {
                Text = Path.GetFileName(path),
                ToolTipText = path,
                Tag = path,
                Image = Properties.Resources.FolderOpenedImage,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            headerItem.Click += (sender, args) => { LaunchApp(path); };
            return headerItem;
        }

        private static void LaunchApp(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to launch \"{Path.GetFileName(path)}\"\n: {e.Message}");
            }
        }

        public void SetIcon(Icon icon)
        {
            if (TrayIcon != null) TrayIcon.Icon = icon;
        }

        public void Dispose()
        {
            if (TrayIcon == null) return;
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }
    }
}
