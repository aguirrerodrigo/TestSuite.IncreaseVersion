using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Should;
using TestSuite.IncreaseVersion.Rules;

namespace TestSuite.IncreaseVersion.Test.Rules
{
    [TestClass]
    public class RemoveCommentedAssemblyFileVersionRuleTest
    {
        private IFile file = Mock.Of<IFile>();
        private IncreaseVersion increaseVersion = new IncreaseVersion();
        private RemoveCommentedAssemblyFileVersionRule rule = new RemoveCommentedAssemblyFileVersionRule();

        [TestInitialize]
        public void Setup()
        {
            Mock.Get(file).SetupProperty(f => f.Contents);
            increaseVersion.Files.Add(file);
            increaseVersion.Rules.Add(rule);
        }
        
        [TestMethod]
        public void TestRemove()
        {
            // Arrange
            file.Contents = "File\r\n //  [ assembly :  AssemblyFileVersion  ( \" 1.2.3.4 \" )   ]  \r\nFile1";

            // Act
            increaseVersion.Execute();

            // Assert
            file.Contents.ShouldEqual("File\r\nFile1\r\n");
        }
    }
}