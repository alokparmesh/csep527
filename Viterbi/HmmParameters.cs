using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public class HmmParameters
    {
        public readonly int numHiddenStates;

        public Dictionary<char, double>[] EmissionProbabilities { get; private set; }

        public double[,] TransitionProbabilities { get; private set; }

        public double[] InitialTransitionProbabilities { get; private set; }

        public HmmParameters(int numHiddenStates)
        {
            this.numHiddenStates = numHiddenStates;
            this.TransitionProbabilities = new double[numHiddenStates, numHiddenStates];
            this.InitialTransitionProbabilities = new double[numHiddenStates];
            this.EmissionProbabilities = new Dictionary<char, double>[numHiddenStates];
        }
    }
}
