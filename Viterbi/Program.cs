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
            ViterbiAlgorithm algo = new ViterbiAlgorithm(2);
            char loaded = 'L';
            char fair = 'F';

            algo.HiddenStates[0] = loaded;
            algo.HiddenStates[1] = fair;

            algo.InitialTransitionProbabilities[0] = 0.52;
            algo.InitialTransitionProbabilities[1] = 0.48;

            algo.TransitionProbabilities[0, 0] = 0.60;
            algo.TransitionProbabilities[0, 1] = 0.40;
            algo.TransitionProbabilities[1, 0] = 0.17;
            algo.TransitionProbabilities[1, 1] = 0.83;

            Dictionary<char, double> loadedProbabilities = new Dictionary<char, double>();
            loadedProbabilities.Add('1', 0.1);
            loadedProbabilities.Add('2', 0.1);
            loadedProbabilities.Add('3', 0.1);
            loadedProbabilities.Add('4', 0.1);
            loadedProbabilities.Add('5', 0.1);
            loadedProbabilities.Add('6', 0.5);
            algo.EmissionProbabilities[0] = loadedProbabilities;

            Dictionary<char, double> fairProbabilities = new Dictionary<char, double>();
            fairProbabilities.Add('1', 1.0/6.0);
            fairProbabilities.Add('2', 1.0/6.0);
            fairProbabilities.Add('3', 1.0/6.0);
            fairProbabilities.Add('4', 1.0/6.0);
            fairProbabilities.Add('5', 1.0/6.0);
            fairProbabilities.Add('6', 1.0/6.0);
            algo.EmissionProbabilities[1] = fairProbabilities;

            Console.WriteLine(algo.TraceBack("316664"));
        }
    }
}
