namespace TestSuite.IncreaseVersion
{
    public interface IFile
    {
        string Contents { get; set; }
        void Save();
    }
}
