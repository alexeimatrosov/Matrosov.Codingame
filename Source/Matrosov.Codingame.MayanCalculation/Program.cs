using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.MayanCalculation
{
    public class Program
    {
        static void Main()
        {
            var numericalSystem = NumericalSystem.ReadFromInput();
        
            var a = numericalSystem.ReadNumber();
            var b = numericalSystem.ReadNumber();

            long result = 0;
            var operation = Console.ReadLine();
            switch(operation)
            {
                case "+": result = a + b; break;
                case "-": result = a - b; break;
                case "*": result = a * b; break;
                case "/": result = a / b; break;
            }

            Console.Error.WriteLine("{0} {1} {2} = {3}", a, operation, b, result);

            var representation = numericalSystem.GetRepresentation(result);
            foreach (var s in representation)
            {
                Console.WriteLine(s);
            }
        }
    }

    public class NumericalSystem
    {
        private const int Base = 20;
        private readonly int _digitWidth;
        private readonly int _digitHeight;
        private readonly string[][] _representation;

        private NumericalSystem(int digitWidth, int digitHeight, string[][] representation)
        {
            _digitWidth = digitWidth;
            _digitHeight = digitHeight;
            _representation = representation;
        }

        public long ReadNumber()
        {
            var numberOfLines = int.Parse(Console.ReadLine());
            var lines = new string[numberOfLines];
            for (var i = 0; i < numberOfLines; i++)
            {
                lines[i] = Console.ReadLine();
            }

            var digitsCount = numberOfLines/_digitHeight;
            var digits = Enumerable.Range(0, digitsCount).Select(x => RecognizeDigit(lines, x)).ToArray();

            long result = 0;
            for (var i = 0; i < digitsCount; i++)
            {
                result += digits[i]*(long) Math.Pow(20, digitsCount - 1 - i);
            }

            return result;
        }

        private int RecognizeDigit(string[] lines, int digitIndex)
        {
            for (var i = 0; i < Base; i++)
            {
                var isMatch = true;
                for (var j = 0; j < _digitHeight && isMatch; j++)
                {
                    isMatch = string.Equals(lines[digitIndex*_digitHeight + j], _representation[i][j]);
                }

                if (isMatch)
                {
                    return i;
                }
            }

            return -1;
        }

        public IList<string> GetRepresentation(long number)
        {
            var result = new List<string>();
            var quotient = number;

            do
            {
                var remainder = quotient % Base;
                quotient = quotient / Base;

                for (var i = 0; i < _digitHeight; i++)
                {
                    result.Insert(i, _representation[remainder][i]);
                }
            } while (quotient > 0);

            return result;
        }

        public static NumericalSystem ReadFromInput()
        {
            var inputs = Console.ReadLine().Split(' ');
            var digitWidth = int.Parse(inputs[0]);
            var digitHeight = int.Parse(inputs[1]);
            
            var representation = new string[Base][];
            for (var i = 0; i < Base; i++)
            {
                representation[i] = new string[digitHeight];
            }

            for (var j = 0; j < digitHeight; j++)
            {
                var line = Console.ReadLine();
                for (var i = 0; i < Base; i++)
                {
                    representation[i][j] = line.Substring(i*digitWidth, digitWidth);
                }
            }

            return new NumericalSystem(digitWidth, digitHeight, representation);
        }
    }
}