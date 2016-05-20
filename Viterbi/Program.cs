using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public class Program
    {
        public static int TotalIteration = 10;
        public static int HitsToPrint = 5;

        public static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                throw new ArgumentException("Please provide filename as argument");
            }
            int.TryParse(ConfigurationManager.AppSettings["totalIteration"], out TotalIteration);
            int.TryParse(ConfigurationManager.AppSettings["hitsToPrint"], out HitsToPrint);

            TestCpGIsland(args[0]);
            //TestDishonestCasino();
        }

        /// <summary>
        /// Setup initial parameter and print hits
        /// </summary>
        /// <param name="fileName"></param>
        private static void TestCpGIsland(string fileName)
        {
            string seq = GetSequence(fileName);

            HmmParameters hmmParameters = new HmmParameters(2);

            hmmParameters.InitialTransitionProbabilities[0] = 0.9999;
            hmmParameters.InitialTransitionProbabilities[1] = 1.0 - hmmParameters.InitialTransitionProbabilities[0];

            hmmParameters.TransitionProbabilities[0, 0] = 0.9999;
            hmmParameters.TransitionProbabilities[0, 1] = 1 - hmmParameters.TransitionProbabilities[0, 0];
            hmmParameters.TransitionProbabilities[1, 0] = 0.01;
            hmmParameters.TransitionProbabilities[1, 1] = 1 - hmmParameters.TransitionProbabilities[1, 0];

            Dictionary<char, double> lowProbabilities = new Dictionary<char, double>();
            lowProbabilities.Add('A', 0.25);
            lowProbabilities.Add('C', 0.25);
            lowProbabilities.Add('G', 0.25);
            lowProbabilities.Add('T', 0.25);
            hmmParameters.EmissionProbabilities[0] = lowProbabilities;

            Dictionary<char, double> highProbabilities = new Dictionary<char, double>();
            highProbabilities.Add('A', 0.20);
            highProbabilities.Add('C', 0.30);
            highProbabilities.Add('G', 0.30);
            highProbabilities.Add('T', 0.20);
            hmmParameters.EmissionProbabilities[1] = highProbabilities;

            for (int iteration = 1; iteration <= TotalIteration; iteration++)
            {
                Console.WriteLine("Iteration #{0}", iteration);
                Console.WriteLine("-----------");
                var st = Stopwatch.StartNew();
                hmmParameters.Print();
                Console.WriteLine();
                var result = ViterbiAlgorithm.Estimate(seq, hmmParameters);

                Console.WriteLine("Log probability : {0}", result.LogProbability.ToString("#.000000"));
                Console.WriteLine();
                PrintCpGIsland(result, 1, iteration == TotalIteration ? int.MaxValue : HitsToPrint);
                result.UpdateParameters(hmmParameters);
                st.Stop();
                //Console.WriteLine("ElapsedMilliseconds {0}", st.ElapsedMilliseconds);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Print Hits
        /// </summary>
        /// <param name="stateSequence">HMM State Sequence</param>
        /// <param name="state">State for which hits are calculated</param>
        /// <param name="hitsToPrint">Number of hits to print</param>
        private static void PrintCpGIsland(HmmResult stateSequence, int state, int hitsToPrint)
        {
            int currentpos = 1;
            int cpgIslandStart = -1;
            int cpgIslandEnd = -1;
            int totalHits = 0;
            foreach (var item in stateSequence.HiddenStates)
            {
                if(item.Equals(state))
                {
                    if(cpgIslandStart > 0)
                    {
                        cpgIslandEnd = currentpos;
                    }
                    else
                    {
                        cpgIslandStart = currentpos;
                        cpgIslandEnd = currentpos;
                    }
                }
                else
                {
                    if (cpgIslandStart > 0)
                    {
                        totalHits++;
                        if (totalHits <= hitsToPrint)
                        {
                            Console.WriteLine("[{0}]\tStart: {1},\tEnd: {2},\tLength: {3}", totalHits, cpgIslandStart, cpgIslandEnd, cpgIslandEnd - cpgIslandStart + 1);
                        }
                        cpgIslandStart = -1;
                        cpgIslandEnd = -1;
                       
                    }                    
                }
                currentpos++;
            }

            if (cpgIslandStart > 0)
            {
                totalHits++;
                if (totalHits <= hitsToPrint)
                {
                    Console.WriteLine("[{0}]\tStart: {1},\tEnd: {2},\tLength: {3}", totalHits, cpgIslandStart, cpgIslandEnd, cpgIslandEnd - cpgIslandStart + 1);
                }
                cpgIslandStart = -1;
                cpgIslandEnd = -1;               
            }

            Console.WriteLine("Total Hits : {0}", totalHits);
        }

        /// <summary>
        /// Test case of loaded die
        /// </summary>
        private static void TestDishonestCasino()
        {
            HmmParameters hmmParameters = new HmmParameters(2);            

            hmmParameters.InitialTransitionProbabilities[0] = 0.30;
            hmmParameters.InitialTransitionProbabilities[1] = 1.0 - hmmParameters.InitialTransitionProbabilities[0];

            hmmParameters.TransitionProbabilities[0, 0] = 0.90;
            hmmParameters.TransitionProbabilities[0, 1] = 1 - hmmParameters.TransitionProbabilities[0, 0];
            hmmParameters.TransitionProbabilities[1, 0] = 0.05;
            hmmParameters.TransitionProbabilities[1, 1] = 1 - hmmParameters.TransitionProbabilities[1, 0];

            Dictionary<char, double> loadedProbabilities = new Dictionary<char, double>();
            loadedProbabilities.Add('1', 0.1);
            loadedProbabilities.Add('2', 0.1);
            loadedProbabilities.Add('3', 0.1);
            loadedProbabilities.Add('4', 0.1);
            loadedProbabilities.Add('5', 0.1);
            loadedProbabilities.Add('6', 0.5);
            hmmParameters.EmissionProbabilities[0] = loadedProbabilities;

            Dictionary<char, double> fairProbabilities = new Dictionary<char, double>();
            fairProbabilities.Add('1', 1.0 / 6.0);
            fairProbabilities.Add('2', 1.0 / 6.0);
            fairProbabilities.Add('3', 1.0 / 6.0);
            fairProbabilities.Add('4', 1.0 / 6.0);
            fairProbabilities.Add('5', 1.0 / 6.0);
            fairProbabilities.Add('6', 1.0 / 6.0);
            hmmParameters.EmissionProbabilities[1] = fairProbabilities;

            foreach(var item in ViterbiAlgorithm.Estimate("1111166666", hmmParameters).HiddenStates)
            {
                Console.Write(item);
            }
        }

        /// <summary>
        /// Get sequence from fasta file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetSequence(string fileName)
        {
            string seq = null;
            if (SequenceRetriever.TryRetrieveSequence(fileName, out seq))
            {
                StringBuilder sb = new StringBuilder();
                HashSet<char> ACGT = new HashSet<char> { 'A', 'C', 'G', 'T' };
                foreach (var item in seq)
                {
                    if(ACGT.Contains(Char.ToUpper(item)))
                    {
                        sb.Append(Char.ToUpper(item));
                    }
                    else
                    {
                        sb.Append('T');
                    }
                }
                return sb.ToString();
            }
            else
            {
                throw new Exception(string.Format("Could not find file {0}", fileName));
            }
        }
    }
}
