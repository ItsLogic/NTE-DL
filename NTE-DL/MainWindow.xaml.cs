using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace NTE_DL
{
    public partial class MainWindow : Window
    {
        private dynamic VersionsList;
        public MainWindow()
        {
            InitializeComponent();
            LoadDropdownItems();
        }

        private async void LoadDropdownItems()
        {
            List<string> items = await FetchItemsFromApi();
            DropdownMenu.ItemsSource = items;
        }

        private async Task<List<string>> FetchItemsFromApi()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://raw.githubusercontent.com/ItsLogic/NTE-DL/refs/heads/data/versions.json");
                response.EnsureSuccessStatusCode();
                string ResponseString = await response.Content.ReadAsStringAsync();
                VersionsList = JObject.Parse(ResponseString);
                List<string> Versions = new List<string>();
                foreach (var version in VersionsList)
                {
                    Versions.Add(version.Name);
                }
                return new List<string>(Versions);
            }
        }

private void ApplyButton_Click(object sender, RoutedEventArgs e)
{
    string SelectedVersion = DropdownMenu.SelectedItem as string;
    if (SelectedVersion != null)
    {
        string configFilePath = "NTELauncher/Config/Config.ini";
        string newUrl = $"https://raw.githubusercontent.com/ItsLogic/NTE-DL/refs/heads/data/{VersionsList[SelectedVersion].path}/";
        
        var lines = File.ReadAllLines(configFilePath);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("VersionInfoFileURL") || lines[i].StartsWith("BackupServer") || lines[i].StartsWith("BackupVersionURL"))
            {
                lines[i] = $"{lines[i].Split('=')[0]}={newUrl + "Version.ini"}";
            }
        }
        File.WriteAllLines(configFilePath, lines);
        
        string patcherConfigPath = "NTELauncher/ResFilesM/1289/PatcherConfig/PatcherConfig.json";
        var patcherConfigJson = File.ReadAllText(patcherConfigPath);
        dynamic patcherConfig = JsonConvert.DeserializeObject(patcherConfigJson);
        patcherConfig.gameResUrl[0] = newUrl;
        var updatedPatcherConfigJson = JsonConvert.SerializeObject(patcherConfig, Formatting.Indented);
        File.WriteAllText(patcherConfigPath, updatedPatcherConfigJson);
        
        string updateTempPath = "NTELauncher/updatetemp";
        if (Directory.Exists(updateTempPath))
        {
            DirectoryInfo di = new DirectoryInfo(updateTempPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        
        string userDataPath = "NTELauncher/UserData";
        if (Directory.Exists(userDataPath))
        {
            DirectoryInfo di = new DirectoryInfo(userDataPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        MessageBox.Show($"Launcher will now download {SelectedVersion}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
    }
}