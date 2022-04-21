using MapDownloaderAPI.Sources;
using System.Text.Json;
using System.IO.Compression;
using System.Xml;

namespace MapDownloadAPI
{

    /// <summary>
    /// Used to download maps and install them for use by bannerlord client
    /// </summary>
    // TODO: Figure out testing private methods so we can change the protection on functions
    public class MDAPI
    {
        /// <summary>
        /// Path to the bannerlord directory
        /// </summary>
        string bannerLordPath;

        public MDAPI(string bannerLordPath)
        {
            this.bannerLordPath = bannerLordPath;
        }

        public string DownloadMaps(string inputJsonPath)
        {
            string jsonContents = File.ReadAllText(inputJsonPath);

            // Convert the json to a list of objects
            List<MapDownloadRequest> requests = ParseInputJson(jsonContents);
            if (requests.Count == 0)
            {
                return "Input JSON was not valid";
            }

            // Validate the objects parsed from the json
            int count = 1;
            foreach (MapDownloadRequest request in requests)
            {

                if (!request.isValid())
                {
                    return "Json Request #" + count + " is invalid. Possibly missing data: " + request.ToString();
                }

                if (!request.gameTypeValid())
                {
                    return "Json Request #" + count + " is invalid. One or more gametypes were invalid: " + request.ToString() + "\nValid game types are: " + request.getValidGameTypes();
                }

                count++;
            }

            // Perform the download and install for each request
            foreach (MapDownloadRequest request in requests)
            {
                string error = DownloadAndInstallMap(request);
                if (error != "success")
                {
                    return error;
                }
            }

            return "success";
        }

        public List<MapDownloadRequest> ParseInputJson(string jsonContents)
        {
            List<MapDownloadRequest> result;
            try
            {
#pragma warning disable CS8604 // Possible null reference argument.
                result = new List<MapDownloadRequest>(JsonSerializer.Deserialize<MapDownloadRequest[]>(JsonDocument.Parse(jsonContents)));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            catch (Exception ex)
            {
                result = new List<MapDownloadRequest>();
            }

            return result;
        }

        public string DownloadAndInstallMap(MapDownloadRequest request)
        {

            string downloadedFile = Path.GetTempPath() + request.mapFolderName + ".zip";

            // Download map from the url and store it in temp
            WebSource.DownloadZippedFiles(request.url, downloadedFile);

            // Extract the zip to "Mount & Blade II Bannerlord\Modules\Native\SceneObj"
            ZipFile.ExtractToDirectory(downloadedFile, bannerLordPath + @"\Modules\Native\SceneObj", true);

            // Delete the downloaded zip file just to be kosher
            File.Delete(downloadedFile);

            // Verify that the folder in the request matches an existing directory in Mount & Blade II Bannerlord\Modules\Native\SceneObj
            if (!Directory.Exists(bannerLordPath + @"\Modules\Native\SceneObj\" + request.mapFolderName))
            {
                return "'mapFolderName' does not match the extracted folder. Json Request Info: " + request.ToString();
            }

            // Update the XML to have the new scene with given game types
            UpdateMultiplayerSceneXml(request.mapFolderName, request.gameTypes);

            return "success";
        }

        public string UpdateMultiplayerSceneXml(string mapFolderName, string[] gameTypes)
        {
            string xmlDocPath = bannerLordPath + @"\Modules\Native\ModuleData\Multiplayer\MultiplayerScenes.xml";

            if (!File.Exists(xmlDocPath))
            {
                return "MultiplayerScenes.xml does not exist at path: " + bannerLordPath + @"\Modules\Native\ModuleData\Multiplayer\";
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(File.ReadAllText(xmlDocPath));

            XmlNode? existingScene = xmlDocument.SelectSingleNode("/MultiplayerScenes/Scene[@name='" + mapFolderName + "']");

            // If scene exists, we remove it
            if (existingScene != null)
            {
                xmlDocument.FirstChild?.RemoveChild(existingScene);
            }

            // Add new scene
            XmlElement scene = xmlDocument.CreateElement("Scene");
            scene.SetAttribute("name", mapFolderName);

            // Add the game types to the new scene
            foreach (string gameType in gameTypes)
            {
                XmlElement type = xmlDocument.CreateElement("GameType");
                type.SetAttribute("name", gameType);
                scene.AppendChild(type);
            }

            // Add new scene to xml tree and save
            xmlDocument.FirstChild?.AppendChild(scene);
            xmlDocument.Save(xmlDocPath);

            return "success";
        }

    }
}