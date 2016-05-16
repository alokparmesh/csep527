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

        public string TraceBack(string sequence)
        {
            StringBuilder[] previousTraceBackSequences = new StringBuilder[this.numHiddenStates];
            double[] previousSequenceProbabilities = new double[this.numHiddenStates];

            StringBuilder[] currentTraceBackSequences;
            double[] currentSequenceProbabilities;

            for (int i = 0; i < this.numHiddenStates; i++)
            {
                previousTraceBackSequences[i] = new StringBuilder();
                previousTraceBackSequences[i].Append(this.HiddenStates[i]);
                previousSequenceProbabilities[i] = this.InitialTransitionProbabilities[i] * this.EmissionProbabilities[i][sequence[0]];
            }

            int sequenceLength = sequence.Length;

            for (int i = 1; i < sequenceLength; i++)
            {
                currentSequenceProbabilities = new double[this.numHiddenStates];
                currentTraceBackSequences = new StringBuilder[this.numHiddenStates];
                char currentEmission = sequence[i];

                for (int k = 0; k < this.numHiddenStates; k++)
                {
                    double maxSequenceProbability = 0.0;
                    for (int j = 0; k < this.numHiddenStates; j++)
                    {
                        double sequenceProbability = previousSequenceProbabilities[k] * this.TransitionProbabilities[k,j] * this.EmissionProbabilities[j][currentEmission];

                        if(sequenceProbability > maxSequenceProbability)
                        {
                            maxSequenceProbability = sequenceProbability;
                        }

                    }

                    currentSequenceProbabilities[k] = maxSequenceProbability;
                }

                previousSequenceProbabilities = currentSequenceProbabilities;
                previousTraceBackSequences = currentTraceBackSequences;
            }
        }
    }
}
