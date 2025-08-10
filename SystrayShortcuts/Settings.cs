using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystrayShortcuts
{
    internal static class Settings
    {
        private record FolderEntry(string FolderPath, string IconPath, int IconIndex);

        private static readonly string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SystrayShortcuts");

        private static readonly string filename = "Settings.json";

        public static void Load(ref List<TrayFolder> folders)
        {
            string settingsFile = Path.Combine(folderPath, filename);
            if (!File.Exists(settingsFile))
            {
                return;
            }
            string json = File.ReadAllText(settingsFile);
            List<FolderEntry> entries = Deserialize(json);
            foreach (FolderEntry entry in entries)
            {
                folders.Add(new TrayFolder(entry.FolderPath, entry.IconPath, entry.IconIndex));
            }
        }

        public static void Update(List<TrayFolder> folders)
        {
            List<FolderEntry> entries = new List<FolderEntry>();
            foreach (var trayFolder in folders)
            {
                entries.Add(new FolderEntry(trayFolder.FolderPath, trayFolder.IconPath, trayFolder.IconIndex));
            }

            string json = Serialize(entries);
            Directory.CreateDirectory(folderPath);
            string settingsFile = Path.Combine(folderPath, filename);
            File.WriteAllText(settingsFile, json);
        }

        private static string Serialize(List<FolderEntry> entries)
        {
            return JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        }

        private static List<FolderEntry> Deserialize(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<FolderEntry>>(json);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error loading settings file: {e.Message}", "Systray Shortcuts");
                return new List<FolderEntry>();
            }
        }
    }
}
