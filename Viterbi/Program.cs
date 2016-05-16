using System;
using System.Collections.Generic;
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

            ViterbiAlgorithm algo = new ViterbiAlgorithm(2);
            char low = 'L';
            char high = 'H';

            algo.HiddenStates[0] = low;
            algo.HiddenStates[1] = high;

            algo.InitialTransitionProbabilities[0] = 0.9999;
            algo.InitialTransitionProbabilities[1] = 1.0 - algo.InitialTransitionProbabilities[0];

            algo.TransitionProbabilities[0, 0] = 0.9999;
            algo.TransitionProbabilities[0, 1] = 1 - algo.TransitionProbabilities[0, 0];
            algo.TransitionProbabilities[1, 0] = 0.01;
            algo.TransitionProbabilities[1, 1] = 1 - algo.TransitionProbabilities[1, 0];

            Dictionary<char, double> lowProbabilities = new Dictionary<char, double>();
            lowProbabilities.Add('A', 0.25);
            lowProbabilities.Add('C', 0.25);
            lowProbabilities.Add('G', 0.25);
            lowProbabilities.Add('T', 0.25);
            algo.EmissionProbabilities[0] = lowProbabilities;

            Dictionary<char, double> highProbabilities = new Dictionary<char, double>();
            highProbabilities.Add('A', 0.20);
            highProbabilities.Add('C', 0.30);
            highProbabilities.Add('G', 0.30);
            highProbabilities.Add('T', 0.20);
            algo.EmissionProbabilities[1] = highProbabilities;

            //algo.Estimate("GCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGCGCGCGCCCCCCCGCGC");          
            algo.Estimate(seq);

            //Console.WriteLine(algo.TraceBack());
            PrintCpGIsland(algo.TraceBack(), high);
        }

        private static void PrintCpGIsland(string stateSequence, char state)
        {
            int currentpos = 1;
            int cpgIslandStart = -1;
            int cpgIslandLength = 0;
            foreach (var item in stateSequence)
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
            ViterbiAlgorithm algo = new ViterbiAlgorithm(2);
            char loaded = 'L';
            char fair = 'F';

            algo.HiddenStates[0] = loaded;
            algo.HiddenStates[1] = fair;

            algo.InitialTransitionProbabilities[0] = 0.30;
            algo.InitialTransitionProbabilities[1] = 1.0 - algo.InitialTransitionProbabilities[0];

            algo.TransitionProbabilities[0, 0] = 0.90;
            algo.TransitionProbabilities[0, 1] = 1 - algo.TransitionProbabilities[0, 0];
            algo.TransitionProbabilities[1, 0] = 0.05;
            algo.TransitionProbabilities[1, 1] = 1 - algo.TransitionProbabilities[1, 0];

            Dictionary<char, double> loadedProbabilities = new Dictionary<char, double>();
            loadedProbabilities.Add('1', 0.1);
            loadedProbabilities.Add('2', 0.1);
            loadedProbabilities.Add('3', 0.1);
            loadedProbabilities.Add('4', 0.1);
            loadedProbabilities.Add('5', 0.1);
            loadedProbabilities.Add('6', 0.5);
            algo.EmissionProbabilities[0] = loadedProbabilities;

            Dictionary<char, double> fairProbabilities = new Dictionary<char, double>();
            fairProbabilities.Add('1', 1.0 / 6.0);
            fairProbabilities.Add('2', 1.0 / 6.0);
            fairProbabilities.Add('3', 1.0 / 6.0);
            fairProbabilities.Add('4', 1.0 / 6.0);
            fairProbabilities.Add('5', 1.0 / 6.0);
            fairProbabilities.Add('6', 1.0 / 6.0);
            algo.EmissionProbabilities[1] = fairProbabilities;

            algo.Estimate("1111166666");
            Console.WriteLine(algo.TraceBack());
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
