using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Should;
using TestSuite.IncreaseVersion.Rules;

namespace TestSuite.IncreaseVersion.Test.Rules
{
    [TestClass]
    public class UpdateAssemblyVersionRuleTest
    {
        private IFile file = Mock.Of<IFile>();
        private IncreaseVersion increaseVersion = new IncreaseVersion();
        private UpdateAssemblyVersionRule rule = new UpdateAssemblyVersionRule();

        [TestInitialize]
        public void Setup()
        {
            Mock.Get(file).SetupProperty(f => f.Contents);
            increaseVersion.Files.Add(file);
            increaseVersion.Rules.Add(rule);
        }

        [TestMethod]
        public void TestAppend()
        {
            // Arrange
            file.Contents = "File\r\nFile1";
            rule.NewVersion = "1.0.0.0";

            // Act
            increaseVersion.Execute();

            // Assert
            file.Contents.ShouldEqual("File\r\nFile1\r\n[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n");
        }

        [TestMethod]
        public void TestUpdate()
        {
            // Arrange
            file.Contents = "File\r\n[ assembly :  AssemblyVersion  ( \" 1.2.3.4 \" )   ]  \r\nFile1";
            rule.NewVersion = "1.1.0.0";

            // Act
            increaseVersion.Execute();

            // Assert
            file.Contents.ShouldEqual("File\r\n[assembly: AssemblyVersion(\"1.1.0.0\")]\r\nFile1\r\n");
        }
    }
}