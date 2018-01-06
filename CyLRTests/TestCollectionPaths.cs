using CyLR;
using NUnit.Framework;
using System.IO;
using CyLR.read;
using System.Linq;
using System.Collections;

namespace CyLRTests
{
    [TestFixture]
    public class TestCollectionPaths
    {
        string TempFolder;

        [SetUp]
        public void CreateTempDirectory()
        {
            TempFolder = Path.Combine(Path.GetTempPath(), TestContext.CurrentContext.Random.GetString());

            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), TempFolder));
        }

        [Test]
        public void GetPath()
        {
            var fileName = Path.GetRandomFileName();
            var testFilePath = Path.Combine(TempFolder, fileName);
            File.WriteAllText(testFilePath, "Test");
            Arguments args = new Arguments(new[] { $"{testFilePath}" });
            var collected = CollectionPaths.GetPaths(new NativeFileSystem(), args, Enumerable.Empty<string>());

            Assert.Contains(testFilePath, (ICollection)collected);
        }

        [Test]
        public void GetLongPath()
        {
            var fileName = Path.GetRandomFileName();
            var directoryPath = CreateLongDirectory();
            var testFilePath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(@"\\?\" + testFilePath, "Test");
            Arguments args = new Arguments(new[] { $"{directoryPath}" });
            var collected = CollectionPaths.GetPaths(new NativeFileSystem(), args, Enumerable.Empty<string>());

            Assert.Contains(directoryPath, (ICollection)collected);
        }

        string CreateLongDirectory()
        {
            string longName = TempFolder;
            while (longName.Length < 260)
            {
                longName = Path.Combine(longName, TestContext.CurrentContext.Random.GetString());
            }
            
            Directory.CreateDirectory(@"\\?\" + longName);
            return longName;
        }
    }
}