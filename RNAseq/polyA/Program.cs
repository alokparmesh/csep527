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
    class Program
    {
        private const bool writeOutput = false;
        private const string allSamFile = @"G:\code\csep527\RNAseq\readoutput.sam";
        private const string outputFileFormat = @"G:\code\csep527\RNAseq\readoutput_{0}.sam";
        private const string regexCigar = @"[0-9]+[MIDNSHPX=]";

        private static bool AlignmentFilter(AlignmentLine alignmentLine)
        {
            int totalCount = 0;
            int countOfA = 0;
            int aTailLength = 0;
            double threshold = (14.0 / 15.0);

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

                    if (currentTailRatioOfA > threshold)
                    {
                        aTailLength = countOfA;
                    }
                }

                if (totalCount - countOfA > 5)
                {
                    break;
                }
            }

            if (aTailLength >= 5)
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
            int totalCount = 0;
            int sequenceCount = 0;
            string line;
            Dictionary<int, int> alignmentHisto = new Dictionary<int, int>();
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

                    if(alignment.NumMismatches <=2 )
                    {
                        continue;
                    }

                    if (writeOutput)
                    {
                        outputFile.WriteLine(line);
                    }

                    

                    /*
                    if(!alignment.Cigar.Equals("75M"))
                    {
                        bool print = false;
                        foreach(Match m in Regex.Matches(alignment.Cigar, regexCigar))
                        {
                            char cigarStep = m.Value[m.Value.Length -1];
                            if ( cigarStep != 'M' && cigarStep !='I' && cigarStep != 'D')
                            {
                                print = true;
                            }
                        }

                       if(print)
                            Console.WriteLine(alignment.Cigar);
                    }
                    */
                    
                    int bucket = alignment.AlignmentScore;

                    if (!alignmentHisto.ContainsKey(bucket))
                    {
                        alignmentHisto[bucket] = 0;
                    }

                    alignmentHisto[bucket] += 1;
                    
                    sequenceCount++;
                }
            }

            samFile.Close();

            if (writeOutput)
            {
                outputFile.Close();
            }

            // Suspend the screen.
            Console.WriteLine("Found suitable {0} out of {1}, time:{2}", sequenceCount, totalCount, stopwatch.ElapsedMilliseconds / 1000);
            foreach (var item in alignmentHisto)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);
            }
            Console.ReadLine();
        }
    }
}
