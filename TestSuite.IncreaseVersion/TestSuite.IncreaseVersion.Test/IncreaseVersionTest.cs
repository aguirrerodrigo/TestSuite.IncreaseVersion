using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestSuite.IncreaseVersion.Test
{
    [TestClass]
    public class IncreaseVersionTest
    {
        private IncreaseVersion increaseVersion = new IncreaseVersion();

        [TestMethod]
        public void TestSaveFiles()
        {
            // Arrange
            var file1 = Mock.Of<IFile>();
            var file2 = Mock.Of<IFile>();
            increaseVersion.Files.Add(file1);
            increaseVersion.Files.Add(file2);
            increaseVersion.Rules.Add(Mock.Of<IRule>());

            // Act
            increaseVersion.Execute();

            // Assert
            Mock.Get(file1).Verify(f => f.Save(), Times.Once);
            Mock.Get(file2).Verify(f => f.Save(), Times.Once);
        }

        [TestMethod]
        public void TestExecuteRules()
        {
            // Arrange
            var rule1 = Mock.Of<IRule>();
            var rule2 = Mock.Of<IRule>();
            var file = Mock.Of<IFile>();
            increaseVersion.Files.Add(file);
            increaseVersion.Rules.Add(rule1);
            increaseVersion.Rules.Add(rule2);

            // Act
            increaseVersion.Execute();

            // Assert
            Mock.Get(rule1).Verify(r => r.Execute(file), Times.Once);
            Mock.Get(rule2).Verify(r => r.Execute(file), Times.Once);
        }
    }
}
