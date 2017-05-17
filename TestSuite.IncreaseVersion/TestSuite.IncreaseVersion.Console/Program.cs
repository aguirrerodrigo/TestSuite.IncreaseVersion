using System;
using System.Collections.Generic;
using System.IO;
using TestSuite.IncreaseVersion.Rules;
using console = System.Console;

namespace TestSuite.IncreaseVersion.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = GetDirectory(args);
            var version = GetVersion(args);
            var increaseVersion = CreateIncreaseVersion(version);

            var files = GetFilesRecursive(directory);            
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

        private static string GetDirectory(string[] args)
        {
            var result = default(string);
            if (args.Length < 1)
            {
                console.Write("Input parent directory: ");
                result = console.ReadLine();
            }
            else
                result = args[0];

            if (!Directory.Exists(result))
                throw new DirectoryNotFoundException($"Could not find directory: {result}.");

            return result;
        }

        private static string GetVersion(string[] args)
        {
            var result = default(string);
            if (args.Length < 2)
            {
                console.Write("Input version: ");
                result = console.ReadLine();
            }
            else
                result = args[1];

            return result;
        }

        private static IncreaseVersion CreateIncreaseVersion(string version)
        {
            var result = new IncreaseVersion();
            result.Rules.Add(new RemoveAssemblyFileVersionRule());
            result.Rules.Add(new RemoveCommentedAssemblyFileVersionRule());
            result.Rules.Add(new RemoveCommentedAssemblyVersionRule());

            var updateAssemblyVersionRule = new UpdateAssemblyVersionRule();
            updateAssemblyVersionRule.NewVersion = version;
            result.Rules.Add(updateAssemblyVersionRule);

            return result;
        }
        
        private static IEnumerable<string> GetFilesRecursive(string path)
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
