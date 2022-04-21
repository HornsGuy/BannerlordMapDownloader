using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using Ookii.Dialogs.Wpf;
using MapDownloadAPI;
using System.Diagnostics;

namespace BannerLordMapDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string settingsPath;

        string blPath;

        public MainWindow()
        {
            settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\BMD\";
            Directory.CreateDirectory(settingsPath);
            InitializeComponent();
            blPath = GetBannerlordPath();

            if (blPath == "")
            {
                MessageBox.Show("Bannerlord path not provided. Please restart the application and try again.","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                Environment.Exit(-1);
            }

            // Store the bannerlord path in settings so we don't have to prompt the user again
            StoreBannerlordPath(blPath);

            Log("Bannerlord path found: " + blPath + "\\");
        }

        public void Log(string logEntry)
        {
            TextBox? console = FindName("Console") as TextBox;
            if(console != null)
            {
                console.Text += logEntry + "\n";
            }
            
        }

        public void StoreBannerlordPath(string path)
        {
            string pathFile = settingsPath + "blPath.txt";
            File.WriteAllText(pathFile,path);
        }

        public string LoadBannerlordPathFromSettings()
        {
            string pathFile = settingsPath + "blPath.txt";
            string path = "";
            if(File.Exists(pathFile))
            {
                path = File.ReadAllText(pathFile);
            }    
            return path;
        }

        public string GetSteamBannerlordPath()
        {
            string path = "";

            // Get steam's install directory from registry keys
            string bit32key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
            object val = Registry.GetValue(bit32key, "InstallPath", "");

            if (val == null)
            {
                //try 64 bit
                string bit64Key = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
                val = Registry.GetValue(bit64Key, "InstallPath", "");
                if (val != null)
                {
                    path = val.ToString();
                }
            }
            else
            {
                path = val.ToString();
            }

            if (path != "")
            {
                // Attempt to assemble the bannerlord path for steam
                path += @"\steamapps\common\Mount & Blade II Bannerlord";
                if(!PathIsBannerlordFolder(path))
                {
                    path = "";
                }
            }

            return path;
        }

        public bool PathIsBannerlordFolder(string path)
        {
            return Directory.Exists(path + @"\Modules\Native\SceneObj") && File.Exists(path+ @"\Modules\Native\ModuleData\Multiplayer\MultiplayerScenes.xml");
        }

        /// <summary>
        /// Gets the bannerlord path, or prompts the user to navigate to it
        /// </summary>
        /// <returns>The path to the bannerlord install directory without a trailing \</returns>
        public string GetBannerlordPath()
        {
            // Try steam first
            string path = GetSteamBannerlordPath();
            //string path = "";

            // Try to load the path from settings
            if(path == "")
            {
                path = LoadBannerlordPathFromSettings();
            }

            if(path == "")
            {
                // We couldn't find the bannerlord path, have the user find it for us
                MessageBox.Show("Unable to find Bannerlord folder. Please navigate to and select the folder where Bannerlord is installed.","Unable to Find Bannerlord Directory",MessageBoxButton.OK,MessageBoxImage.Information);

                VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();

                folderBrowserDialog.Description = "Select the Bannerlord Folder";
                folderBrowserDialog.UseDescriptionForTitle = true;
                bool? result = folderBrowserDialog.ShowDialog();
                if (result != null && (bool)result)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    if (PathIsBannerlordFolder(selectedPath))
                    {
                        path = selectedPath;
                    }
                }
            }

            return path;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files (*.json)|*.json";

            openFileDialog.Title = "Select Map Data Json File";
            openFileDialog.DefaultExt = ".json";

            bool? result = openFileDialog.ShowDialog();
            if(result != null && (bool)result)
            {
                string jsonFilename = openFileDialog.FileName;
                MDAPI mDAPI = new MDAPI(blPath);
                string error = mDAPI.DownloadMaps(jsonFilename);
                if(error != "success")
                {
                    Log("!!!");
                    Log(error);
                    Log("!!!");
                }
                else
                {
                    Log("Successfuly installed map from '" + jsonFilename + "'");
                }
            }

        }
    }
}
