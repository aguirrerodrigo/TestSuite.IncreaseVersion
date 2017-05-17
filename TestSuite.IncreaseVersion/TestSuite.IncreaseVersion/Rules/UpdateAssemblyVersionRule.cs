using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TestSuite.IncreaseVersion.Rules
{
    /// <summary>
    /// Appends or Updates: [assembly: AssemblyVersion("{NewVersion}")]
    /// </summary>
    public class UpdateAssemblyVersionRule : IRule
    {
        public string NewVersion { get; set; }

        public void Execute(IFile file)
        {
            var newAssemblyVersionText = $"[assembly: AssemblyVersion(\"{this.NewVersion}\")]";
            var lines = file.Contents.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var foundAssemblyVersion = false;
            var builder = new StringBuilder();
            foreach(var line in lines)
            {
                var match = Regex.Match(line, "\\s*\\[\\s*assembly\\s*:\\s*AssemblyVersion\\s*\\(\\s*\\\".*\\\"\\s*\\)\\s*\\]\\s*");
                if (match.Success)
                {
                    foundAssemblyVersion = true;
                    builder.AppendLine(newAssemblyVersionText);
                }
                else
                    builder.AppendLine(line);
            }

            if (!foundAssemblyVersion)
                builder.AppendLine(newAssemblyVersionText);

            file.Contents = builder.ToString();
        }
    }
}
