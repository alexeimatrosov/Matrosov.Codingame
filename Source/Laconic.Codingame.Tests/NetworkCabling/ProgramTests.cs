using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Laconic.Codingame.Tests.NetworkCabling
{
    [TestFixture]
    public class ProgramTests
    {
        private static readonly string TestCasesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NetworkCabling");

        private static readonly object[] TestCases =
        {
            new[] {"in1.txt", "out1.txt"},
            new[] {"in2.txt", "out2.txt"},
            new[] {"in3.txt", "out3.txt"},
            new[] {"in4.txt", "out4.txt"},
            new[] {"in5.txt", "out5.txt"},
            new[] {"in6.txt", "out6.txt"},
            new[] {"in7.txt", "out7.txt"},
            new[] {"in8.txt", "out8.txt"},
            new[] {"in9.txt", "out9.txt"},
        };

        [TestCaseSource(nameof(TestCases))]
        public void Main_In_Out(string inFile, string outFile)
        {
            var sw = new StringWriter();
            Console.SetIn(File.OpenText(Path.Combine(TestCasesDirectory, inFile)));
            Console.SetOut(sw);

            Codingame.NetworkCabling.Program.Main();

            Assert.That(long.Parse(sw.ToString()), Is.EqualTo(long.Parse(File.ReadLines(Path.Combine(TestCasesDirectory, outFile)).First())));
        }
    }
}
