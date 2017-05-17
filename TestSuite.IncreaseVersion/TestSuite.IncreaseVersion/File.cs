using System.IO;
using IoFile = System.IO.File;

namespace TestSuite.IncreaseVersion
{
    public class File : IFile
    {
        private string path;

        public string Contents { get; set; }

        public File(string path)
        {
            if (!IoFile.Exists(path))
                throw new FileNotFoundException($"Could not find file: {path}.");

            this.path = path;
            this.Contents = IoFile.ReadAllText(path);
        }
        
        public void Save()
        {
            IoFile.WriteAllText(this.path, this.Contents);
        }
    }
}