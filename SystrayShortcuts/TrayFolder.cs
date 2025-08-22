using System.Diagnostics;
using System.Reflection;

namespace SystrayShortcuts
{
    internal class TrayFolder : IDisposable
    {
        public delegate void FileStructureChangedEventHandler(object sender, EventArgs e);

        public event FileStructureChangedEventHandler FileStructureChanged;

        private FileSystemWatcher watcher;
        public string IconPath { get; private set; } = "";
        public int IconIndex { get; private set; }
        public NotifyIcon? TrayIcon { get; private set; }
        public string FolderPath { get; private set; }
        public string Name { get; private set; }



        public TrayFolder(string path)
        {
            Initialize(path);
        }

        public TrayFolder(string path, string iconPath, int iconIndex)
        {
            IconPath = iconPath;
            IconIndex = iconIndex;
            Initialize(path);
        }

        private void Initialize(string path)
        {
            FolderPath = path;
            Name = GetFolderOrDriveName(FolderPath);
            InitWatcher();
        }

        private void InitWatcher()
        {
            watcher = new FileSystemWatcher(FolderPath);
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        public Icon? GetCustomTrayIcon()
        {
            return !string.IsNullOrEmpty(IconPath) ? TrayIcon?.Icon : null;
        }

        protected virtual void OnFileStructureChanged()
        {
            FileStructureChanged?.Invoke(this, EventArgs.Empty);
        }

        public ToolStripMenuItem CreateFolderStructure()
        {
            return CreateFolderStructure(FolderPath);
        }

        private ToolStripMenuItem CreateFolderStructure(string path)
        {
            ToolStripMenuItem parent = new ToolStripMenuItem()
            {
                Text = GetFolderOrDriveName(path),
                Image = Properties.Resources.FolderClosedImage,
            };
            parent.DropDownItems.Add(CreateFolderHeaderRow(path));
            parent.DropDownItems.Add(new ToolStripSeparator());
            ScanFolder(path, parent);
            return parent;
        }

        private ContextMenuStrip CreateContextMenu()
        {
            // Need to copy over the items to be able to show in both tray menu and stand-alone tray folder.
            // ToolStripItems can only have one parent
            var menuItem = new ContextMenuStrip();
            menuItem.Items.AddRange(CreateFolderStructure(FolderPath).DropDownItems);
            return menuItem;
        }


        public void CreateNotifyIcon()
        {
            TrayIcon = new NotifyIcon()
            {
                Text = GetFolderOrDriveName(FolderPath),
                ContextMenuStrip = CreateContextMenu(),
                Visible = true
            };
            SetIcon(IconPath, IconIndex);
            TrayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            // Make it open the context menu on mouse left click
            if (e.Button == MouseButtons.Left)
            {
                typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(TrayIcon, null);
            }
        }

        private void ScanFolder(string path, ToolStripMenuItem parent)
        {
            // Get folders, loop though and add them as menu items
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                // Skip symlinks
                if (new DirectoryInfo(dir).LinkTarget != null) continue;
                // Skip hidden folders
                if (IsHidden(dir)) continue;

                parent.DropDownItems.Add(CreateFolderStructure(dir));
            }

            var files = Directory.GetFiles(path);
            // Get files
            if (files.Length > 100)
            {
                // To many files to draw rows for
                string text = $@"{files.Length} files in folder";
                parent.DropDownItems.Add(
                    new ToolStripLabel()
                    {
                        Text = text,
                        Enabled = false,
                        Font = new Font(SystemFonts.DefaultFont, FontStyle.Regular),
                        Width = text.Length * 5 // Make sure the label does not crop the text
                    });
            }
            else
            {
                // Loop though and add them as rows
                foreach (var file in files)
                {
                    // Skip hidden files
                    if (IsHidden(file)) continue;

                    parent.DropDownItems.Add(Path.GetFileName(file),
                        Icon.ExtractAssociatedIcon(file)?.ToBitmap(),
                        (sender, args) => LaunchApp(file));
                }
            }

            // If no directories or files exist in folder, add an informative row.
            if (dirs.Length == 0 && files.Length == 0)
            {
                parent.DropDownItems.Add(
                    new ToolStripLabel()
                    {
                        Text = @"Empty",
                        Enabled = false,
                        Font = new Font(SystemFonts.DefaultFont, FontStyle.Italic)
                    });
            }
        }

        private static bool IsHidden(string file)
        {
            return (File.GetAttributes(file) & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        private static ToolStripMenuItem CreateFolderHeaderRow(string path)
        {
            ToolStripMenuItem headerItem = new ToolStripMenuItem()
            {
                Text = GetFolderOrDriveName(path),
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
                MessageBox.Show($"Failed to launch \"{Path.GetFileName(path)}\"\n: {e.Message}", "Execute failed");
            }
        }

        private static string GetFolderOrDriveName(string path)
        {
            var name = Path.GetPathRoot(path)!.Equals(path, StringComparison.OrdinalIgnoreCase)
                ? path.TrimEnd(Path.DirectorySeparatorChar)
                : Path.GetFileName(path);
            return name;
        }

        public void SetIcon(string path, int index)
        {
            if (TrayIcon == null) return;
            Icon? icon;
            try
            {
                icon = Icon.ExtractIcon(path, index);
            }
            catch (Exception e)
            {
                // Icon does not exist or could not be loaded, reset to default icon
                TrayIcon.Icon = Properties.Resources.ApplicationIcon;
                return;
            }

            IconPath = path;
            IconIndex = index;
            TrayIcon.Icon = icon;
        }

        private void Redraw()
        {
            if (TrayIcon != null) TrayIcon.ContextMenuStrip = CreateContextMenu();
        }

        public void Dispose()
        {
            watcher.Dispose();
            if (TrayIcon == null) return;
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }

        //private static void OnChanged(object sender, FileSystemEventArgs e)
        //{
        // No need to watch changes
        //}

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            // Check for newly created files and subfolders

            Redraw();
            // Notify that changes is made
            OnFileStructureChanged();
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            // Check if files or folders has been deleted

            Redraw();
            // Notify that changes is made
            OnFileStructureChanged();
        }

        private void OnRenamed(object sender, FileSystemEventArgs e)
        {
            // Check if file or folder has been renamed

            Redraw();
            // Notify that changes is made
            OnFileStructureChanged();
        }
    }
}
