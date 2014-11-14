using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Laconic.Codingame.BenderTheMoneyMachine.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        private class TestData
        {
            public readonly object[] Files =
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
        }

        [TestCaseSource(typeof(TestData), "Files")]
        public void UnitOfWork_Scenario_ExpectedBehaviour(string inFile, string outFile)
        {
            var sw = new StringWriter();
            Console.SetIn(File.OpenText(inFile));
            Console.SetOut(sw);

            Program.Main();

            Assert.That(int.Parse(sw.ToString()), Is.EqualTo(int.Parse(File.ReadLines(outFile).First())));
        }
    }
}
