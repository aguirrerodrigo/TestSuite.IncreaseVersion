using System.Collections.Generic;

namespace TestSuite.IncreaseVersion
{
    public class IncreaseVersion
    {
        public ICollection<IFile> Files { get; } = new HashSet<IFile>();
        public ICollection<IRule> Rules { get; } = new HashSet<IRule>();

        public void Execute()
        {
            foreach (var file in this.Files)
            {
                foreach (var rule in this.Rules)
                {
                    rule.Execute(file);
                }
                file.Save();
            }
        }
    }
}
