using System;
using System.Collections.Generic;
using System.IO;
using TestSuite.IncreaseVersion.Rules;
using console = System.Console;

namespace TestSuite.IncreaseVersion.Console
{
    class Program
    {
        private static string directory;
        private static string version;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                console.Write("Input parent directory: ");
                directory = console.ReadLine();
            }
            else
                directory = args[0];

            if (args.Length < 2)
            {
                console.Write("Input version: ");
                version = console.ReadLine();
            }
            else
                directory = args[1];

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Could not find directory: {directory}.");

            var files = GetFilesRecursive(directory);

            var increaseVersion = new IncreaseVersion();
            increaseVersion.Rules.Add(new RemoveAssemblyFileVersionRule());
            increaseVersion.Rules.Add(new RemoveCommentedAssemblyFileVersionRule());
            increaseVersion.Rules.Add(new RemoveCommentedAssemblyVersionRule());
            var updateAssemblyVersionRule = new UpdateAssemblyVersionRule();
            updateAssemblyVersionRule.NewVersion = version;
            increaseVersion.Rules.Add(updateAssemblyVersionRule);
            foreach(var file in files)
            {
                var filename = Path.GetFileName(file);
                if (string.Equals(filename, "AssemblyInfo.cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    console.WriteLine(file);
                    increaseVersion.Files.Add(new File(file));
                }
            }

            increaseVersion.Execute();
            console.WriteLine($"Update Version(\"{version}\") Complete!");
        }

        static IEnumerable<string> GetFilesRecursive(string path)
        {
            var files = new List<string>(Directory.GetFiles(path));
            foreach(var dir in Directory.GetDirectories(path))
            {
                files.AddRange(GetFilesRecursive(dir));
            }
            return files;
        }
    }
}
