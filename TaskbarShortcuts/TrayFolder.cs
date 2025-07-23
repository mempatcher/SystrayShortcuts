using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TaskbarShortcuts
{
    internal class TrayFolder
    {
        private string folderPath;
        private ToolStripMenuItem baseFolderItem;

        public TrayFolder(string path)
        {
            folderPath = path;
            baseFolderItem = CreateFolderStructure(folderPath);
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
                    (obj, e) => LaunchApp(file));
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

        public ToolStripMenuItem GetFolderItem()
        {
            return baseFolderItem;
        }

        private static ToolStripMenuItem CreateFolderHeaderRow(string path)
        {
            ToolStripMenuItem headerItem = new ToolStripMenuItem()
            {
                Text = Path.GetFileName(path),
                ToolTipText = path,
                Image = Properties.Resources.FolderOpenedImage,
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
            };
            headerItem.Click += (sender, e) => { LaunchApp(path); };
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
    }
}
