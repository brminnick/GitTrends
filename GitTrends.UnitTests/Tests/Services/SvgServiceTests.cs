using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace GitTrends.UnitTests
{
    class SvgServiceTests : BaseTest
    {
        [TestCase("AABBCCDD")]
        public void GetColorStringMap_InalidHex(string hex)
        {
            //Arrange
            var expectedColorMap = new Dictionary<string, string>
            {
                { "#000000", hex }
            };

            //Act
            Assert.Throws<ArgumentException>(() => SvgService.GetColorStringMap(hex));

            //Assert
        }

        [Test]
        public void GetColorStringMap_ValidHex()
        {
            //Arrange
            const string hex = "#AABBCCFF";
            var expectedColorMap = new Dictionary<string, string>
            {
                { "#000000", hex }
            };

            //Act
            var actualColorMap = SvgService.GetColorStringMap(hex);

            //Assert
            Assert.AreEqual(expectedColorMap, actualColorMap);
        }

        [Test]
        public void GetResourcePathTest_ValidSVGName()
        {
            //Arrange
            const string svgName = "star.svg";
            const string expectedResourcePath = "GitTrends.Resources.SVGs." + svgName;

            //Act
            var actualResourcePath = SvgService.GetResourcePath(svgName);

            //Assert
            Assert.AreEqual(expectedResourcePath, actualResourcePath);
        }

        [TestCase("star")]
        [TestCase("star.png")]
        public void GetResourcePathTest_InalidSVGName(string svgName)
        {
            //Arrange
            var expectedResourcePath = "GitTrends.Resources.SVGs." + svgName;

            //Act
            Assert.Throws<ArgumentException>(() => SvgService.GetResourcePath(svgName));

            //Assert
        }

        [Test]
        public void GetFullPathTest_ValidSVGName()
        {
            //Arrange
            const string svgName = "star.svg";
            const string expectedResourcePath = "resource://GitTrends.Resources.SVGs." + svgName;

            //Act
            var actualResourcePath = SvgService.GetFullPath(svgName);

            //Assert
            Assert.AreEqual(expectedResourcePath, actualResourcePath);
        }

        [TestCase("star")]
        [TestCase("star.png")]
        public void GetFullPathTest_InalidSVGName(string svgName)
        {
            //Arrange

            //Act
            Assert.Throws<ArgumentException>(() => SvgService.GetResourcePath(svgName));

            //Assert
        }
    }
}
