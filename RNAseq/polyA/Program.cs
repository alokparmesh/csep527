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
        //private const string allSamFile = @"G:\code\csep527\RNAseq\readoutput.sam";
        private const string outputFileFormat = @"G:\code\csep527\RNAseq\readoutput_{0}.sam";

        public const int MinimumPolyATailLength = 5;
        public const double RnaAccuracyRate = (14.0 / 15.0);
        public const double TailMatchRatio = 0.4;

        /// <summary>
        /// Filter condition to primary filtering
        /// </summary>
        /// <param name="alignmentLine"></param>
        /// <returns></returns>
        private static bool AlignmentFilter(AlignmentLine alignmentLine)
        {
            int totalCount = 0;
            int countOfA = 0;
            int aTailLength = 0;

            // Skip unmapped sequences
            if (alignmentLine.Cigar.Equals("*"))
            {
                return true;
            }

            char[] sequence = alignmentLine.Sequence.ToCharArray();

            // Ensure read has minimum length AA...AA tail with defined read accuracy
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

                // Skip if too many non A's encountered (i.e. greater than permitted by read errors)
                if (totalCount - countOfA > (1 - Program.RnaAccuracyRate) * 75)
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

        /// <summary>
        /// Main method to select candidates and run various model
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Get candidates from file
            var result = GetCandidates(args[0]);

            Stopwatch stopwatch = Stopwatch.StartNew();

            double[,] baseDistribution = GetBaseDistribution();

            Console.WriteLine("WMM0 Model");
            WeightMatrixModel model = new WeightMatrixModel(GetWMM0Distribution(), baseDistribution);
            model.Match(result);
            Console.WriteLine();

            Console.WriteLine("WMM1 Model");
            model = new WeightMatrixModel(GetWMM1Distribution(), baseDistribution);
            model.Match(result);
            Console.WriteLine();

            Console.WriteLine("WMM2 Model");
            //PrintOutputDistribution(model.OutputDistribution);
            model = new WeightMatrixModel(model.OutputDistribution, baseDistribution);
            model.Match(result);
            Console.WriteLine();

            Console.WriteLine("Time taken(s) to run 3 WMMs : {0}", stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine();
        }

        private static void PrintOutputDistribution(double[,] outputDistribution)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Console.Write(Math.Round(outputDistribution[i, j], 4));
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

        private static List<AlignmentLine> GetCandidates(string fileName)
        {
            Console.WriteLine("Candidate Selection Conditions");
            Console.WriteLine("Condition Minimum A-Tail Length: {0}", MinimumPolyATailLength);
            Console.WriteLine("Read Accuracy percentage : {0:0.00}", RnaAccuracyRate * 100);
            Console.WriteLine("Minimum poly-A tail to genome mismatch percentage : {0:0.00}", (1 - TailMatchRatio) * 100);
            Console.WriteLine();

            int totalCount = 0;
            int sequenceCount = 0;
            string line;
            List<AlignmentLine> result = new List<AlignmentLine>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Read the file
            StreamReader samFile = new StreamReader(fileName);
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

                    // Fix read errors at the beginning ,end or in middle by ignoring them for mismatch numbers
                    // We are optimistic that those match the reference sequence
                    alignment.FixUnidentifiedReads();

                    // Skip if read and reference genome match pretty much
                    if (alignment.NumMismatches <= 2)
                    {
                        continue;
                    }

                    // Find the polyA tail and its cleavage site
                    alignment.FindCleavageSite();

                    // Skip if could not find polyA tail
                    if (alignment.CleavageSite < 0)
                    {
                        continue;
                    }

                    if (writeOutput)
                    {
                        outputFile.Write(line);
                        outputFile.WriteLine("\t{0}\t{1}", alignment.CleavageMarkedSequence, alignment.ReferenceSequence);
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

            Console.WriteLine("{0} candidates selected out of {1} reads examined.\nTime taken(s) to select candidates:{2}", sequenceCount, totalCount, stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine();
            return result;
        }
    }
}
