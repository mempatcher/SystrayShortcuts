using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskbarShortcuts
{
    
    internal class TrayMain
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        public TrayMain()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Add folder shortcut...", Properties.Resources.AddFolderImage, (obj,e) => AddFolder());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, (obj, e) => Application.Exit());

            trayIcon = new NotifyIcon()
            {
                Text = "Taskbar shortcuts",
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
            trayMenu.Items.Insert(1, tf.GetFolderItem());
        }
    }
}
