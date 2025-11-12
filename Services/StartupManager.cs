using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using WinBootSelfStarting.Models;

namespace WinBootSelfStarting.Services
{
    public static class StartupManager
    {
        private const string RunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string DisabledKey = "Software\\WinBootSelfStarting\\DisabledRun";

        private static string StartupFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        private static string DisabledStartupFolder => Path.Combine(Path.GetDirectoryName(StartupFolderPath) ?? StartupFolderPath, "WinBootSelfStarting_Disabled");

        public static List<StartupEntry> ListEntries()
        {
            var list = new List<StartupEntry>();

            // Registry enabled
            using (var hkcu = Registry.CurrentUser.OpenSubKey(RunKey, false))
            {
                if (hkcu != null)
                {
                    foreach (var name in hkcu.GetValueNames())
                    {
                        var val = Convert.ToString(hkcu.GetValue(name)) ?? "";
                        list.Add(new StartupEntry { Id = name, Name = name, Command = val, Location = StartupLocation.Registry });
                    }
                }
            }

            // Registry disabled (our disabled store)
            using (var dk = Registry.CurrentUser.OpenSubKey(DisabledKey, false))
            {
                if (dk != null)
                {
                    foreach (var name in dk.GetValueNames())
                    {
                        var val = Convert.ToString(dk.GetValue(name)) ?? "";
                        list.Add(new StartupEntry { Id = name, Name = name, Command = val, Location = StartupLocation.DisabledRegistry });
                    }
                }
            }

            // Startup folder enabled
            try
            {
                if (Directory.Exists(StartupFolderPath))
                {
                    foreach (var f in Directory.GetFiles(StartupFolderPath))
                    {
                        var fi = new FileInfo(f);
                        list.Add(new StartupEntry { Id = fi.Name, Name = fi.Name, Command = fi.FullName, Location = StartupLocation.StartupFolder });
                    }
                }
            }
            catch { }

            // Disabled startup files
            try
            {
                if (Directory.Exists(DisabledStartupFolder))
                {
                    foreach (var f in Directory.GetFiles(DisabledStartupFolder))
                    {
                        var fi = new FileInfo(f);
                        list.Add(new StartupEntry { Id = fi.Name, Name = fi.Name, Command = fi.FullName, Location = StartupLocation.DisabledFolder });
                    }
                }
            }
            catch { }

            return list;
        }

        public static bool AddRegistryEntry(string name, string command)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (key == null)
                    {
                        using (var created = Registry.CurrentUser.CreateSubKey(RunKey))
                        {
                            created.SetValue(name, command);
                        }
                    }
                    else
                    {
                        key.SetValue(name, command);
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public static bool RemoveEntry(StartupEntry entry)
        {
            try
            {
                switch (entry.Location)
                {
                    case StartupLocation.Registry:
                        using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                        {
                            key?.DeleteValue(entry.Id, false);
                        }
                        break;
                    case StartupLocation.DisabledRegistry:
                        using (var dk = Registry.CurrentUser.OpenSubKey(DisabledKey, true))
                        {
                            dk?.DeleteValue(entry.Id, false);
                        }
                        break;
                    case StartupLocation.StartupFolder:
                        if (File.Exists(entry.Command)) File.Delete(entry.Command);
                        break;
                    case StartupLocation.DisabledFolder:
                        if (File.Exists(entry.Command)) File.Delete(entry.Command);
                        break;
                }
                return true;
            }
            catch { return false; }
        }

        public static bool DisableEntry(StartupEntry entry)
        {
            try
            {
                switch (entry.Location)
                {
                    case StartupLocation.Registry:
                        // Move to our disabled key
                        using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                        {
                            var val = key?.GetValue(entry.Id);
                            if (val != null)
                            {
                                using (var dk = Registry.CurrentUser.CreateSubKey(DisabledKey))
                                {
                                    dk.SetValue(entry.Id, val);
                                }
                                key.DeleteValue(entry.Id, false);
                            }
                        }
                        break;
                    case StartupLocation.StartupFolder:
                        Directory.CreateDirectory(DisabledStartupFolder);
                        var dest = Path.Combine(DisabledStartupFolder, Path.GetFileName(entry.Command));
                        if (File.Exists(entry.Command)) File.Move(entry.Command, dest, true);
                        break;
                    default:
                        // already disabled or unknown
                        break;
                }
                return true;
            }
            catch { return false; }
        }

        public static bool EnableEntry(StartupEntry entry)
        {
            try
            {
                switch (entry.Location)
                {
                    case StartupLocation.DisabledRegistry:
                        using (var dk = Registry.CurrentUser.OpenSubKey(DisabledKey, true))
                        {
                            var val = dk?.GetValue(entry.Id);
                            if (val != null)
                            {
                                using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
                                {
                                    key.SetValue(entry.Id, val);
                                }
                                dk.DeleteValue(entry.Id, false);
                            }
                        }
                        break;
                    case StartupLocation.DisabledFolder:
                        var src = entry.Command;
                        if (File.Exists(src))
                        {
                            Directory.CreateDirectory(StartupFolderPath);
                            var dest = Path.Combine(StartupFolderPath, Path.GetFileName(src));
                            File.Move(src, dest, true);
                        }
                        break;
                    default:
                        // already enabled or unknown
                        break;
                }
                return true;
            }
            catch { return false; }
        }
    }
}
