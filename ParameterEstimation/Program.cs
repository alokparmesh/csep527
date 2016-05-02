using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterEstimation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Only one parameter expected");
            }

            string fileName = args[0];

            if (!File.Exists(fileName))
            {
                throw new ArgumentException(string.Format("File not found {0}", fileName));
            }

            List<double> numbers = GetNuberList(fileName);

            PrintEstimation(numbers, int.Parse(args[1]));
        }

        private static void PrintEstimation(List<double> numbers, int numMixtures)
        {
            if (numMixtures == 1)
            {
                double average = numbers.Average();
                double sumOfSquares = numbers.Select(val => (val - average) * (val - average)).Sum();
                double sd = Math.Sqrt(sumOfSquares / (numbers.Count - 1));
                Console.WriteLine(average.ToString("#.00000"));
                Console.WriteLine(sd.ToString("#.00000"));

                double product = GetLikelihood(numbers, average, sd);

                Console.WriteLine(Math.Log(product).ToString("#.00000"));
                Console.WriteLine(2 *Math.Log(product) - 2 * Math.Log(numbers.Count));
            }
            else
            {
                EMAlgorithm algo = new EMAlgorithm(numbers, numMixtures);
                algo.Calculate();
            }
        }

        private static double GetLikelihood(List<double> numbers, double average, double sd)
        {
            double product = 1.0;

            foreach (double number in numbers)
            {
                double Likelihood = GuassianLikelihood(number, average, sd);
                product *= Likelihood;
                
                /*
                double Likelihood1 = GuassianLikelihood(number, 21, 1) / 3;
                double Likelihood2 = GuassianLikelihood(number, 46, 1) / 3;
                double Likelihood3 = GuassianLikelihood(number, 55, 1) / 3;
                product *= (Likelihood1+Likelihood2+Likelihood3);
                */
            }

            return product;
        }

        private static double GuassianLikelihood(double number, double average, double sd)
        {
            double d = (number - average) / sd;
            double exp = Math.Exp(-1.0 * (d * d) / 2);
            return exp / (sd * Math.Sqrt(2 * Math.PI));
        }

        private static List<double> GetNuberList(string fileName)
        {
            List<double> numbers = new List<double>();

            using (var sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (s == null || string.IsNullOrWhiteSpace(s))
                    {
                        // empty or comment;
                        continue;
                    }

                    string[] numberStringArray = s.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                    foreach (string numberString in numberStringArray)
                    {
                        double number;
                        if (double.TryParse(numberString, out number))
                        {
                            numbers.Add(number);
                        }
                    }
                }
            }

            return numbers;
        }
    }
}
