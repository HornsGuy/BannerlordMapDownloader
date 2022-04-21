using NUnit.Framework;
using MapDownloadAPI;
using System.Collections.Generic;
using System.IO;

namespace MapDownloadAPITests
{
    public class APITests
    {

        [Test]
        public void TestValidJsonParsing()
        {
            string inputJson = "[{\"url\":\"127.0.0.1\",\"mapFolderName\":\"mp_compact\",\"gameTypes\":[\"Siege\"]}," +
                           "{\"url\":\"127.0.0.1/abc\",\"mapFolderName\":\"mp_duel\",\"gameTypes\":[\"Duel\"]}," +
                           "{\"url\":\"127.0.0.1/123\",\"mapFolderName\":\"mp_battle_tdm\",\"gameTypes\":[\"Battle\",\"TDM\"]}]";

            MDAPI api = new MDAPI("");

            List<MapDownloadRequest> expected = new List<MapDownloadRequest>();
            expected.Add(new MapDownloadRequest("127.0.0.1", "mp_compact", new string[] { "Siege" }));
            expected.Add(new MapDownloadRequest("127.0.0.1/abc", "mp_duel", new string[] { "Duel" }));
            expected.Add(new MapDownloadRequest("127.0.0.1/123", "mp_battle_tdm", new string[] { "Battle", "TDM" }));
            List<MapDownloadRequest> actual = api.ParseInputJson(inputJson);
            Assert.AreEqual(expected, actual);
            
        }

        [Test]
        public void TestInvalidJsonParsing()
        {
            string inputJson = "[{\"url\":\"127.0.0.1\",\"mapFolderName\":\"mp_compact\",\"gameTypes\":[\"Siege\"]}," +
                           "{\"url\":\"127.0.0.1/abc\",\"mapFolderName\":\"mp_duel\",\"gameTypes\":[\"Duel\"]}," +
                           "{\"url\":\"127.0.0.1/123\",\"mapFolderName\":\"mp_battle_tdm\"}]]]]]]]";
            MDAPI api = new MDAPI("");
            List<MapDownloadRequest> expected = new List<MapDownloadRequest>();
            List<MapDownloadRequest> actual = api.ParseInputJson(inputJson);
            Assert.AreEqual(expected, actual);
        }

        // TODO: Test MapDownloadRequest validity w/ null values and game types

        void IntegratedCleanup()
        {
            if(Directory.Exists("../../../../TestData/TestOutput/Modules/Native/SceneObj"))
                Directory.Delete("../../../../TestData/TestOutput/Modules/Native/SceneObj", true);
            Directory.CreateDirectory("../../../../TestData/TestOutput/Modules/Native/SceneObj");
        }

        [Test]
        [TestCase("../../../../TestData/ValidTest.json",true)]
        [TestCase("../../../../TestData/InvalidGameType.json", false)]
        [TestCase("../../../../TestData/MissingUrl.json", false)]
        public void IntegratedTest(string inputJsonFile, bool expected)
        {
            IntegratedCleanup();
            MDAPI api = new MDAPI("../../../../TestData/TestOutput");
            string actual = api.DownloadMaps(inputJsonFile);
            Assert.AreEqual(expected, actual=="success");
        }

    }
}