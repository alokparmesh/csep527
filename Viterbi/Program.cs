using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestCpGIsland();
            //TestDishonestCasino();
        }

        private static void TestCpGIsland()
        {
            string seq = GetSequence("NC_000909.fna");

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

            //algo.Estimate("GCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGC");  
            //Console.WriteLine(algo.TraceBack());
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int iteration = 0; iteration < 10; iteration++)
            {
                if(iteration == 9)
                {
                    stopwatch.Stop();
                }

                var result = ViterbiAlgorithm.Estimate(seq, hmmParameters);
                if (iteration == 9)
                {
                    PrintCpGIsland(result, 1);
                }
                result.UpdateParameters(hmmParameters);
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        private static void PrintCpGIsland(HmmResult stateSequence, int state)
        {
            int currentpos = 1;
            int cpgIslandStart = -1;
            int cpgIslandLength = 0;
            foreach (var item in stateSequence.HiddenStates)
            {
                if(item.Equals(state))
                {
                    if(cpgIslandStart > 0)
                    {
                        cpgIslandLength++;
                    }
                    else
                    {
                        cpgIslandStart = currentpos;
                        cpgIslandLength = 1;                        
                    }
                }
                else
                {
                    if (cpgIslandStart > 0)
                    {
                        Console.WriteLine("Start {0}, Length {1}", cpgIslandStart, cpgIslandLength);
                        cpgIslandStart = -1;
                        cpgIslandLength = 0;
                    }                    
                }
                currentpos++;
            }

            if (cpgIslandStart > 0)
            {
                Console.WriteLine("Start {0}, Length {1}", cpgIslandStart, cpgIslandLength);
                cpgIslandStart = -1;
                cpgIslandLength = 0;
            }
        }

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
