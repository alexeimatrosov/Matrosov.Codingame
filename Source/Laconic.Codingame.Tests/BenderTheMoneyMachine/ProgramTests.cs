using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Laconic.Codingame.Tests.BenderTheMoneyMachine
{
    [TestFixture]
    public class ProgramTests
    {
        private class TestData
        {
            public readonly object[] Files =
            {
                new[] {"BenderTheMoneyMachine/in01.txt", "BenderTheMoneyMachine/out01.txt"},
                new[] {"BenderTheMoneyMachine/in02.txt", "BenderTheMoneyMachine/out02.txt"},
                new[] {"BenderTheMoneyMachine/in03.txt", "BenderTheMoneyMachine/out03.txt"},
                new[] {"BenderTheMoneyMachine/in04.txt", "BenderTheMoneyMachine/out04.txt"},
                new[] {"BenderTheMoneyMachine/in05.txt", "BenderTheMoneyMachine/out05.txt"},
                new[] {"BenderTheMoneyMachine/in06.txt", "BenderTheMoneyMachine/out06.txt"},
                new[] {"BenderTheMoneyMachine/in07.txt", "BenderTheMoneyMachine/out07.txt"},
                new[] {"BenderTheMoneyMachine/in08.txt", "BenderTheMoneyMachine/out08.txt"},
                new[] {"BenderTheMoneyMachine/in09.txt", "BenderTheMoneyMachine/out09.txt"},
            };
        }

        [TestCaseSource(typeof(TestData), "Files")]
        public void Main_In_Out(string inFile, string outFile)
        {
            var sw = new StringWriter();
            Console.SetIn(File.OpenText(inFile));
            Console.SetOut(sw);

            Codingame.BenderTheMoneyMachine.Program.Main();

            Assert.That(int.Parse(sw.ToString()), Is.EqualTo(int.Parse(File.ReadLines(outFile).First())));
        }
    }
}
