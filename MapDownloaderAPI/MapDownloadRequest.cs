using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MapDownloadAPI
{
    /// <summary>
    /// The info needed to download and install a new map for use by multiplayer servers
    /// </summary>
    public class MapDownloadRequest
    {
        /// <summary>
        /// URL to the zip file containing the map files found in "Modules\Native\SceneObj\<MapName>
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// The name of the folder found in "Modules\Native\SceneObj"
        /// </summary>
        public string mapFolderName { get; set; }

        /// <summary>
        /// List of game types this map supports
        /// </summary>
        public string[] gameTypes { get; set; }

        public MapDownloadRequest(string url, string mapFolderName, string[] gameTypes)
        {
            this.url = url;
            this.mapFolderName = mapFolderName;
            this.gameTypes = gameTypes;
        }

        static readonly List<string> validGameTypes = new List<string>( new string[]{ "TeamDeathmatch", "Battle", "Captain", "Skirmish", "FreeForAll", "Duel", "Siege" });

        public string getValidGameTypes()
        {
            return string.Join(", ", validGameTypes);
        }

        public bool gameTypeValid()
        {
            foreach (string type in gameTypes)
            {
                if (!validGameTypes.Contains(type))
                {
                    return false;
                }
            }
            return true;
        }

        public bool isValid()
        {
            return url != null && mapFolderName != null && gameTypes != null;
        }

        public override bool Equals(object? obj)
        {
            if(obj != null && obj.GetType() == GetType())
            {
                return ((MapDownloadRequest)obj).url.Equals(url)
                    && ((MapDownloadRequest)obj).mapFolderName.Equals(mapFolderName)
                    && Enumerable.SequenceEqual(((MapDownloadRequest)obj).gameTypes,gameTypes);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            string gameTypesString = "null";
            if(gameTypes != null)
            {
                gameTypesString = string.Join(", ",gameTypes);
            }
            return "url: " + url + ", folderName: " + mapFolderName + ", gameTypes: " + gameTypesString;
        }
    }
}
