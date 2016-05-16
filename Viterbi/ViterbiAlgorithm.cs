using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public class ViterbiAlgorithm
    {
        private readonly int numHiddenStates;
        public Dictionary<char,double>[] EmissionProbabilities { get; set; }

        public double[,] TransitionProbabilities { get; set; }

        public double[] InitialTransitionProbabilities { get; set; }

        public char[] HiddenStates { get; set; }

        public ViterbiAlgorithm(int numHiddenStates)
        {
            this.numHiddenStates = numHiddenStates;
            this.HiddenStates = new char[numHiddenStates];
            this.TransitionProbabilities = new double[numHiddenStates, numHiddenStates];
            this.InitialTransitionProbabilities = new double[numHiddenStates];
            this.EmissionProbabilities = new Dictionary<char, double>[numHiddenStates];
        }

        public string Estimate(string sequence)
        {
            if(string.IsNullOrEmpty(sequence))
            {
                throw new ArgumentException("sequence");
            }

            int sequenceLength = sequence.Length;

            int[,] maxStates = new int[sequenceLength, this.numHiddenStates];

            double[] previousSequenceProbabilities = new double[this.numHiddenStates];
            double[] currentSequenceProbabilities = new double[this.numHiddenStates];

            for (int i = 0; i < this.numHiddenStates; i++)
            {
                maxStates[0, i] = i;
                previousSequenceProbabilities[i] = this.InitialTransitionProbabilities[i] * this.EmissionProbabilities[i][sequence[0]];
            }

            for (int i = 1; i < sequenceLength; i++)
            {
                char currentEmission = sequence[i];

                for (int k = 0; k < this.numHiddenStates; k++)
                {
                    double maxSequenceProbability = 0.0;
                    int maxState = -1;

                    for (int j = 0; j < this.numHiddenStates; j++)
                    {
                        double sequenceProbability = previousSequenceProbabilities[j] * this.TransitionProbabilities[j,k] * this.EmissionProbabilities[k][currentEmission];

                        if(sequenceProbability > maxSequenceProbability)
                        {
                            maxSequenceProbability = sequenceProbability;
                            maxState = j;
                        }
                    }

                    currentSequenceProbabilities[k] = maxSequenceProbability;
                    maxStates[i, k] = maxState;
                }

                previousSequenceProbabilities = currentSequenceProbabilities;
            }

            double maxTotalProbability = 0.0;
            int finalState = -1;

            for (int k = 0; k < this.numHiddenStates; k++)
            {
                if (currentSequenceProbabilities[k] > maxTotalProbability)
                {
                    maxTotalProbability = currentSequenceProbabilities[k];
                    finalState = k;
                }
            }

            return TraceBack(maxStates, finalState);
        }

        private string TraceBack(int[,] maxStates, int finalState)
        {
            StringBuilder sb = new StringBuilder();
            int i = maxStates.GetLength(0);
            int currentState = finalState;

            while(i > 0)
            {
                sb.Append(this.HiddenStates[currentState]);
                i--;
                currentState = maxStates[i, finalState];
            }
            var cArray = sb.ToString().ToCharArray();
            cArray.Reverse();
            return new string(cArray);
        }
    }
}
