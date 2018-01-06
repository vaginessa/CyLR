using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using CyLR.read;

namespace CyLR
{
    internal static class CollectionPaths
    {
        private static bool IsWantedFile(string filepath)
        {
            var allcollect = new List<string>
            {
                // Plist Files (including Opera / Safari browser history)
                @".plist",
                // Shell History
                @".bash_history",
                @".sh_history",
                // FireFox browser history
                @"places.sqlite",
                @"downloads.sqlite"
            };
            if (Platform.IsUnixLike()) // Linux and Mac Collection
            {
                // System Files
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
                allcollect.Add(@"/etc/rc.d");
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
                string rootpath = @Environment.ExpandEnvironmentVariables("%SYSTEMROOT%").Substring(0, 3);
                // System Files
                allcollect.Add(@"\System32\drivers\etc\hosts");
                allcollect.Add(@"\SchedLgU.Txt");
                allcollect.Add(@"\Microsoft\Windows\Start Menu\Programs\Startup");
                allcollect.Add(@"\System32\config");
                allcollect.Add(@"\System32\winevt\logs");
                allcollect.Add(@"\Prefetch");
                allcollect.Add(@"\Tasks");
                allcollect.Add(@"\System32\LogFiles\W3SVC1");
                allcollect.Add(@"\$MFT");
                allcollect.Add(@"ntuser.dat");
                allcollect.Add(@"NTUSER.DAT");
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

            return allcollect.Any(filepath.Contains);
        }

        private static IEnumerable<string> GetAllFiles(IFileSystem fileSystem, string @path)
        {
            IEnumerable<string> allFiles = Enumerable.Empty<string>();
            try
            {
                allFiles = fileSystem.GetFilesFromPath(@path);
            }
            catch (UnauthorizedAccessException)
            {
                //Console.WriteLine("1: Failed to read files in '{0}' due to insufficient privilages.", path);
                allFiles = Enumerable.Empty<string>();
            }

            foreach (var @file in allFiles)
            {
                if (IsWantedFile(@file)) { yield return @file; }
            }

            IEnumerable<string> allDirectories = Enumerable.Empty<string>();
            try
            {
                allDirectories = Directory.GetDirectories(@path);
            }
            catch (UnauthorizedAccessException)
            {
                //Console.WriteLine("2: Failed to read files in '{0}' due to insufficient privilages.", path);
                allDirectories = Enumerable.Empty<string>();
            }

            foreach (var @file in allDirectories.SelectMany((dir)=>GetAllFiles(fileSystem, dir)))
            {
                yield return @file;
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
        public static List<string> GetPaths(IFileSystem fileSystem, Arguments arguments, List<string> additionalPaths)
        {
            var defaultPaths = new List<string> { };
            string rootpath = @Environment.ExpandEnvironmentVariables("%SYSTEMROOT%").Substring(0, 3);
            if (Platform.IsUnixLike()) {rootpath = @"/";}

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
            else 
            {
                defaultPaths.AddRange(GetAllFiles(fileSystem, @rootpath));
            }
            if (paths.Count == 1)
            {
                if (paths[0] == "")
                {
                    defaultPaths.AddRange(GetAllFiles(fileSystem, @rootpath));
                    return defaultPaths;
                }
            }
            return paths.Any() ? paths : defaultPaths;
        }
    }
}
