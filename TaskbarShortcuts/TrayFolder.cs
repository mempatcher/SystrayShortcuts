using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarShortcuts
{
    internal class TrayFolder
    {
        private string folderPath;
        private ToolStripMenuItem baseFolderItem;
        public string Name { get; }

        public TrayFolder(string path)
        {
            folderPath = path;
            Name = Path.GetFileName(folderPath);
            baseFolderItem = new ToolStripMenuItem()
            {
                Text = Name,
                Image = Properties.Resources.FolderClosedImage,
            };
            ScanFolder(folderPath, baseFolderItem);
        }

        private void ScanFolder(string path, ToolStripMenuItem parent)
        {
            parent.DropDownItems.Add(CreateFolderHeaderRow(path));
            parent.DropDownItems.Add(new ToolStripSeparator());

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {

            }

            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                parent.DropDownItems.Add(
                    new ToolStripLabel()
                    {
                        Text = "Empty",
                        Enabled = false,
                        Font = new Font(SystemFonts.DefaultFont, FontStyle.Italic)
                    });
            }
            foreach (var file in files)
            {
                parent.DropDownItems.Add(Path.GetFileName(file),
                    Icon.ExtractAssociatedIcon(file)?.ToBitmap(),
                    (obj, e) => LaunchApp(file));
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
