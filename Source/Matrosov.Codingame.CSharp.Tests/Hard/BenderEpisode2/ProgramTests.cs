using System;
using System.IO;
using System.Linq;
using Matrosov.Codingame.CSharp.Hard.BenderEpisode2;
using NUnit.Framework;

namespace Matrosov.Codingame.CSharp.Tests.Hard.BenderEpisode2
{
    [TestFixture]
    public class ProgramTests
    {
        private static readonly string TestCasesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Hard/BenderEpisode2/TestCases");

        private static readonly object[] TestCases =
        {
            new[] {"in01.txt", "out01.txt"},
            new[] {"in02.txt", "out02.txt"},
            new[] {"in03.txt", "out03.txt"},
            new[] {"in04.txt", "out04.txt"},
            new[] {"in05.txt", "out05.txt"},
            new[] {"in06.txt", "out06.txt"},
            new[] {"in07.txt", "out07.txt"},
            new[] {"in08.txt", "out08.txt"},
            new[] {"in09.txt", "out09.txt"},
        };

        [TestCaseSource(nameof(TestCases))]
        public void Main_In_Out(string inFile, string outFile)
        {
            var sw = new StringWriter();
            Console.SetIn(File.OpenText(Path.Combine(TestCasesDirectory, inFile)));
            Console.SetOut(sw);

            Program.Main();

            Assert.That(int.Parse(sw.ToString()), Is.EqualTo(int.Parse(File.ReadLines(Path.Combine(TestCasesDirectory, outFile)).First())));
        }
    }
}
