using System;

namespace WinBootSelfStarting.Models
{
    public enum StartupLocation
    {
        Registry,
        StartupFolder,
        DisabledRegistry,
        DisabledFolder
    }

    public class StartupEntry
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
        public StartupLocation Location { get; set; }
        public bool IsEnabled => Location == StartupLocation.Registry || Location == StartupLocation.StartupFolder;
        // For registry entries this is the registry value name; for files it's the file name
        public string Id { get; set; } = "";
    }
}
