using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace polyA
{
    public class Program
    {
        private const bool writeOutput = false;
        private const string allSamFile = @"G:\code\csep527\RNAseq\readoutput.sam";
        private const string outputFileFormat = @"G:\code\csep527\RNAseq\readoutput_{0}.sam";
        public const int MinimumPolyATailLength = 5;
        public const double RnaAccuracyRate = (14.0 / 15.0);

        private static bool AlignmentFilter(AlignmentLine alignmentLine)
        {
            int totalCount = 0;
            int countOfA = 0;
            int aTailLength = 0;

            if (alignmentLine.Cigar.Equals("*"))
            {
                return true;
            }

            char[] sequence = alignmentLine.Sequence.ToCharArray();

            for (int i = sequence.Length - 1; i >= 0; i--)
            {
                if (sequence[i] == 'N')
                {
                    continue;
                }

                totalCount++;

                if (sequence[i] == 'A')
                {
                    countOfA++;
                    double currentTailRatioOfA = ((double)countOfA) / totalCount;

                    if (currentTailRatioOfA > Program.RnaAccuracyRate)
                    {
                        aTailLength = countOfA;
                    }
                }

                if (totalCount - countOfA > (1-Program.RnaAccuracyRate) * 75)
                {
                    break;
                }
            }

            if (aTailLength >= Program.MinimumPolyATailLength)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        static void Main(string[] args)
        {
            var result = GetCandidates();
            double[,] baseDistribution = GetBaseDistribution();

            Console.WriteLine("WMM0");
            WeightMatrixModel model = new WeightMatrixModel(GetWMM0Distribution(), baseDistribution);
            model.Match(result);
            PrintOutputDistribution(model.OutputDistribution);

            Console.WriteLine("WMM1");
            model = new WeightMatrixModel(GetWMM1Distribution(), baseDistribution);
            model.Match(result);
            PrintOutputDistribution(model.OutputDistribution);

            Console.WriteLine("WMM2");
            model = new WeightMatrixModel(model.OutputDistribution, baseDistribution);
            model.Match(result);
            PrintOutputDistribution(model.OutputDistribution);
        }

        private static void PrintOutputDistribution(double[,] outputDistribution)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Console.Write(Math.Round(outputDistribution[i, j],4));
                    Console.Write("\t");
                }
                Console.WriteLine();
            }
        }

        private static double[,] GetBaseDistribution()
        {
            double[,] baseDistribution = new double[4, 6];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    baseDistribution[i, j] = 0.25;
                }
            }

            return baseDistribution;
        }

        private static double[,] GetWMM0Distribution()
        {
            double[,] distribution = new double[4, 6];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    distribution[i, j] = 0.0;
                }
            }

            distribution[0, 0] = 1.0;
            distribution[0, 1] = 1.0;
            distribution[3, 2] = 1.0;
            distribution[0, 3] = 1.0;
            distribution[0, 4] = 1.0;
            distribution[0, 5] = 1.0;

            return distribution;
        }

        private static double[,] GetWMM1Distribution()
        {
            double[,] distribution = new double[4, 6];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    distribution[i, j] = 0.05;
                }
            }

            distribution[0, 0] = 0.85;
            distribution[0, 1] = 0.85;
            distribution[3, 2] = 0.85;
            distribution[0, 3] = 0.85;
            distribution[0, 4] = 0.85;
            distribution[0, 5] = 0.85;

            return distribution;
        }

        private static List<AlignmentLine> GetCandidates()
        {
            int totalCount = 0;
            int sequenceCount = 0;
            string line;
            List<AlignmentLine> result = new List<AlignmentLine>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Read the file and display it line by line.
            StreamReader samFile = new StreamReader(allSamFile);
            StreamWriter outputFile = null;

            if (writeOutput)
            {
                string outputFileName = string.Format(outputFileFormat, DateTime.UtcNow.Ticks);
                Console.WriteLine("Writing to {0}", outputFileName);
                outputFile = new StreamWriter(outputFileName);
            }

            while ((line = samFile.ReadLine()) != null)
            {
                if (line.StartsWith("@"))
                {
                    if (writeOutput)
                    {
                        outputFile.WriteLine(line);
                    }
                    continue;
                }

                totalCount++;
                if (line.StartsWith("ERR"))
                {
                    AlignmentLine alignment = AlignmentLine.GetAlignmentLine(line, Program.AlignmentFilter);

                    if (alignment == null)
                    {
                        continue;
                    }

                    alignment.FixUnidentifiedReads();

                    if (alignment.NumMismatches <= 2)
                    {
                        continue;
                    }

                    alignment.FindCleavageSite();
                    if (alignment.CleavageSite < 0)
                    {
                        continue;
                    }

                    if (writeOutput)
                    {
                        outputFile.Write(line);
                        outputFile.WriteLine("\t{0}", alignment.CleavageMarkedSequence);
                    }

                    result.Add(alignment);
                    sequenceCount++;
                }
            }

            samFile.Close();

            if (writeOutput)
            {
                outputFile.Close();
            }

            Console.WriteLine("Condition Minimum A-Tail Length: {0}, RNA Accuracy: {1}", MinimumPolyATailLength,RnaAccuracyRate);
            Console.WriteLine("{0} candidates selected out of {1} reads examined. Time taken(s):{2}", sequenceCount, totalCount, stopwatch.ElapsedMilliseconds / 1000);
            return result;
        }
    }
}
