using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace CyLR
{
    internal static class CollectionPaths
    {
        private static List<string> AllFiles;
        private static List<string> tempPaths;

        
        private static IEnumerable<string> GetAllFiles(string path)
        {
            var allcollect = new List<string>
            {
                // Plist Files (including Opera / Safari browser history)
                @".plist",
                // Shell History
                @".bash_history",
                // FireFox browser history
                @".sh_history",
                @"places.sqlite",
            };
            if (Platform.IsUnixLike()) // Linux and Mac Collection
            {
                // System Files
                allcollect.Add(@"/root/.bash_history");
                allcollect.Add(@"/var/log");
                allcollect.Add(@"/private/var/log/");
                allcollect.Add(@"/.fseventsd");
                allcollect.Add(@"/etc/hosts.allow");
                allcollect.Add(@"/etc/hosts.deny");
                allcollect.Add(@"/etc/hosts");
                allcollect.Add(@"/System/Library/StartupItems");
                allcollect.Add(@"/System/Library/LaunchAgents");
                allcollect.Add(@"/System/Library/LaunchDaemons");
                allcollect.Add(@"/Library/LaunchAgents");
                allcollect.Add(@"/Library/LaunchDaemons");
                allcollect.Add(@"/Library/StartupItems");
                allcollect.Add(@"/etc/passwd");
                allcollect.Add(@"/etc/group");
                // Chrome Browser
                allcollect.Add(@"Support/Google/Chrome/Default/History");
                allcollect.Add(@"Support/Google/Chrome/Default/Cookies");
                allcollect.Add(@"Support/Google/Chrome/Default/Bookmarks");
                allcollect.Add(@"Support/Google/Chrome/Default/Extensions");
                allcollect.Add(@"Support/Google/Chrome/Default/Last");
                allcollect.Add(@"Support/Google/Chrome/Default/Shortcuts");
                allcollect.Add(@"Support/Google/Chrome/Default/Top");
                allcollect.Add(@"Support/Google/Chrome/Default/Visited");
            }
            else // Windows Collection
            {
                // System Files
                allcollect.Add(@"%SYSTEMROOT%\System32\drivers\etc\hosts");
                allcollect.Add(@"%SYSTEMROOT%\SchedLgU.Txt");
                allcollect.Add(@"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup");
                allcollect.Add(@"%SYSTEMROOT%\System32\config");
                allcollect.Add(@"%SYSTEMROOT%\System32\winevt\logs");
                allcollect.Add(@"%SYSTEMROOT%\Prefetch");
                allcollect.Add(@"%SYSTEMROOT%\Tasks");
                allcollect.Add(@"%SYSTEMROOT%\System32\LogFiles\W3SVC1");
                allcollect.Add(@"%SystemDrive%\$MFT");
                allcollect.Add(@"ntuser.dat");
                // Chrome Browser
                allcollect.Add(@"Chrome\User Data\Default\Default\History");
                allcollect.Add(@"Chrome\User Data\Default\Default\Cookies");
                allcollect.Add(@"Chrome\User Data\Default\Default\Bookmarks");
                allcollect.Add(@"Chrome\User Data\Default\Default\Extensions");
                allcollect.Add(@"Chrome\User Data\Default\Default\Last");
                allcollect.Add(@"Chrome\User Data\Default\Default\Shortcuts");
                allcollect.Add(@"Chrome\User Data\Default\Default\Top");
                allcollect.Add(@"Chrome\User Data\Default\Default\Visited");
            }

            var allFiles = Directory.GetFiles(@path);
            foreach (var file in allFiles)
            {
                yield return file;
            }

            foreach (var filepath in allFiles)
            {
                if (allcollect.Any(filepath.Contains))
                {
                    yield return filepath;
                }
            }

        }

        private static IEnumerable<string> RunCommand(string OSCommand, string CommandArgs)
        {
            var newPaths = new List<string> { };
            var proc = new Process
            { 
                StartInfo = new ProcessStartInfo
                {
                    FileName = OSCommand,
                    Arguments = CommandArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                yield return  proc.StandardOutput.ReadLine();
            };
        }
        public static List<string> GetPaths(Arguments arguments, List<string> additionalPaths)
        {
            var defaultPaths = new List<string> { };

            foreach (var file in Directory.GetDirectories(path).SelectMany(GetAllFiles))
                yield return file;
            if (Platform.IsUnixLike())
            {
                defaultPaths = GetAllFiles(@"/");
            }
            else
            {
                defaultPaths = GetAllFiles(@"%SYSTEMROOT%");
            }    


                // Fix any spaces to work with MacOS naming conventions
            defaultPaths = tempPaths.ConvertAll(stringToCheck => stringToCheck.Replace(" ", " "));
            }
            var paths = new List<string>(additionalPaths);

            if (arguments.CollectionFilePath != ".")
            {
                if (File.Exists(arguments.CollectionFilePath))
                {
                    paths.AddRange(File.ReadAllLines(arguments.CollectionFilePath).Select(Environment.ExpandEnvironmentVariables));
                }
                else
                {
                    Console.WriteLine("Error: Could not find file: {0}", arguments.CollectionFilePath);
                    Console.WriteLine("Exiting");
                    throw new ArgumentException();
                }
            }

            if (arguments.CollectionFiles != null)
            {
                paths.AddRange(arguments.CollectionFiles);
            }

            if (paths.Count == 1)
            {
                if (paths[0] == "")
                {
                    return defaultPaths;
                }
            }
            return paths.Any() ? paths : defaultPaths;
        }
    }
}
