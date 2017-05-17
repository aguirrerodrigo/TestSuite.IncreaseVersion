using System.Collections.Generic;

namespace TestSuite.IncreaseVersion
{
    public class IncreaseVersion
    {
        public ICollection<IFile> Files { get; } = new List<IFile>();
        public ICollection<IRule> Rules { get; } = new List<IRule>();

        public void Execute()
        {
            foreach(var rule in this.Rules)
            {
                foreach(var file in this.Files)
                {
                    rule.Execute(file);
                    file.Save();
                }
            }
        }
    }
}
