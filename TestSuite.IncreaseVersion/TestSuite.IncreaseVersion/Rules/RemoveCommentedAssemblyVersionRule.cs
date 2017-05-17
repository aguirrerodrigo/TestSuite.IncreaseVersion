﻿using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TestSuite.IncreaseVersion.Rules
{
    /// <summary>
    /// Removes: // [assembly: AssemblyVersion("1.0.*")]
    /// </summary>
    public class RemoveCommentedAssemblyVersionRule : IRule
    {
        public void Execute(IFile file)
        {
            var lines = file.Contents.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "\\s*\\/\\/\\s*\\[\\s*assembly\\s*:\\s*AssemblyVersion\\s*\\(\\s*\\\".*\\\"\\s*\\)\\s*\\]\\s*");
                if (!match.Success)
                {
                    builder.AppendLine(line);
                }
            }

            file.Contents = builder.ToString();
        }
    }
}
