using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace MapDownloaderAPI.Sources
{
    public static class WebSource
    {

        public static void DownloadZippedFiles(string url, string destinationPath)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, destinationPath);
            }
        }
    }
}
