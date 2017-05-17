using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TestSuite.IncreaseVersion.Rules
{
    /// <summary>
    /// Removes: [assembly: AssemblyFileVersion("1.0.*")]
    /// </summary>
    public class RemoveAssemblyFileVersionRule : IRule
    {
        public void Execute(IFile file)
        {
            var lines = file.Contents.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "\\s*\\[\\s*assembly\\s*:\\s*AssemblyFileVersion\\s*\\(\\s*\\\".*\\\"\\s*\\)\\s*\\]\\s*");
                if (!match.Success)
                {
                    builder.AppendLine(line);
                }
            }

            file.Contents = builder.ToString();
        }
    }
}
