using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Laconic.Codingame.Tests.NetworkCabling
{
    [TestFixture]
    public class ProgramTests
    {
        private class TestData
        {
            public readonly object[] Files =
            {
                new[] {"NetworkCabling/in1.txt", "NetworkCabling/out1.txt"},
                new[] {"NetworkCabling/in2.txt", "NetworkCabling/out2.txt"},
                new[] {"NetworkCabling/in3.txt", "NetworkCabling/out3.txt"},
                new[] {"NetworkCabling/in4.txt", "NetworkCabling/out4.txt"},
                new[] {"NetworkCabling/in5.txt", "NetworkCabling/out5.txt"},
                new[] {"NetworkCabling/in6.txt", "NetworkCabling/out6.txt"},
                new[] {"NetworkCabling/in7.txt", "NetworkCabling/out7.txt"},
                new[] {"NetworkCabling/in8.txt", "NetworkCabling/out8.txt"},
                new[] {"NetworkCabling/in9.txt", "NetworkCabling/out9.txt"},
            };
        }

        [TestCaseSource(typeof(TestData), "Files")]
        public void Main_In_Out(string inFile, string outFile)
        {
            var sw = new StringWriter();
            Console.SetIn(File.OpenText(inFile));
            Console.SetOut(sw);

            Codingame.NetworkCabling.Program.Main();

            Assert.That(long.Parse(sw.ToString()), Is.EqualTo(long.Parse(File.ReadLines(outFile).First())));
        }
    }
}
