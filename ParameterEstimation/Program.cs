using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterEstimation
{
    public class Program
    {
        public static int PrintprobabilitiesNRow = 25;

        static void Main(string[] args)
        {
            int.TryParse(ConfigurationManager.AppSettings["printprobabilitiesNRow"], out PrintprobabilitiesNRow);

            ParameterEstimationOption options = new ParameterEstimationOption();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                string fileName = options.InputFile;

                if (!File.Exists(fileName))
                {
                    throw new ArgumentException(string.Format("File not found {0}", fileName));
                }

                List<double> numbers = GetNuberList(fileName);
                Console.WriteLine("Number of mixture (k) = {0}",options.MixtureCount);
                PrintEstimation(numbers, options.MixtureCount);
                Console.WriteLine("----------------------------------------------------------------------");
            }
        }

        private static void PrintEstimation(List<double> numbers, int numMixtures)
        {
            // For one mixture case directly estimate the mu and sd without EM
            if (numMixtures == 1)
            {
                double average = numbers.Average();
                double sumOfSquares = numbers.Select(val => (val - average) * (val - average)).Sum();
                double sd = Math.Sqrt(sumOfSquares / (numbers.Count - 1));
                double logLikelihood = GetLogLikelihood(numbers, average, sd);
                double bic = 2 * logLikelihood - 2 * Math.Log(numbers.Count);

                Console.Write("mu");
                Console.Write("\t");
                Console.Write("sd");
                Console.Write("\t");
                Console.Write("LogLik");
                Console.Write("\t\t");
                Console.Write("BIC");
                Console.WriteLine();

                Console.Write(average.ToString("#.0000"));
                Console.Write("\t");
                Console.Write(sd.ToString("#.0000"));
                Console.Write("\t");
                Console.Write(logLikelihood.ToString("#.0000"));
                Console.Write("\t");
                Console.Write(bic.ToString("#.0000"));
                Console.WriteLine();
            }
            else
            {
                // Use EM to figure out mu
                EMAlgorithm algo = new EMAlgorithm(numbers, numMixtures);
                algo.Calculate();
            }
        }

        /// <summary>
        /// For a given set of numbers and mu,sd calculate log likelihood
        /// </summary>
        /// <param name="numbers">list of numbers</param>
        /// <param name="average">mu</param>
        /// <param name="sd">standard deviation</param>
        /// <returns>log likelihood</returns>
        private static double GetLogLikelihood(List<double> numbers, double average, double sd)
        {
            double likelihood = 1.0;

            foreach (double number in numbers)
            {
                double density = GuassianDensity(number, average, sd);
                likelihood *= density;
            }

            return Math.Log(likelihood);
        }

        /// <summary>
        /// For a given number, mu and sd calculate density
        /// </summary>
        /// <param name="number"></param>
        /// <param name="average"></param>
        /// <param name="sd"></param>
        /// <returns></returns>
        private static double GuassianDensity(double number, double average, double sd)
        {
            double d = (number - average) / sd;
            double exp = Math.Exp(-1.0 * (d * d) / 2);
            return exp / (sd * Math.Sqrt(2 * Math.PI));
        }

        /// <summary>
        /// Read a list of white space seperated numbers
        /// from a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
