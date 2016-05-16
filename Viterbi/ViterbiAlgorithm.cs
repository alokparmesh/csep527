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
        private int finalState;
        private int[,] maxStates;
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

        public void Estimate(string sequence)
        {            
            if (string.IsNullOrEmpty(sequence))
            {
                throw new ArgumentException("sequence");
            }

            int sequenceLength = sequence.Length;
            this.finalState = -1;
            this.maxStates = new int[sequenceLength, this.numHiddenStates];

            double[] previousSequenceProbabilities = new double[this.numHiddenStates];
            double[] currentSequenceProbabilities = new double[this.numHiddenStates];

            for (int i = 0; i < this.numHiddenStates; i++)
            {
                maxStates[0, i] = i;
                previousSequenceProbabilities[i] = Math.Log(this.InitialTransitionProbabilities[i]) + Math.Log(this.EmissionProbabilities[i][sequence[0]]);
            }

            for (int i = 1; i < sequenceLength; i++)
            {
                char currentEmission = sequence[i];

                for (int k = 0; k < this.numHiddenStates; k++)
                {
                    double maxSequenceProbability = double.MinValue;
                    int maxState = -1;

                    for (int j = 0; j < this.numHiddenStates; j++)
                    {
                        double sequenceProbability = previousSequenceProbabilities[j] + Math.Log(this.TransitionProbabilities[j,k]) + Math.Log(this.EmissionProbabilities[k][currentEmission]);

                        if(sequenceProbability > maxSequenceProbability)
                        {
                            maxSequenceProbability = sequenceProbability;
                            maxState = j;
                        }
                    }

                    currentSequenceProbabilities[k] = maxSequenceProbability;
                    this.maxStates[i, k] = maxState;
                }

                previousSequenceProbabilities = currentSequenceProbabilities;
                currentSequenceProbabilities = new double[this.numHiddenStates];
            }

            double maxTotalProbability = double.MinValue;

            for (int k = 0; k < this.numHiddenStates; k++)
            {
                if (previousSequenceProbabilities[k] > maxTotalProbability)
                {
                    maxTotalProbability = previousSequenceProbabilities[k];
                    this.finalState = k;
                }
            }
        }

        public string TraceBack()
        {
            StringBuilder sb = new StringBuilder();
            int i = this.maxStates.GetLength(0);
            int currentState = finalState;

            while(i > 0)
            {
                sb.Append(this.HiddenStates[currentState]);
                i--;
                currentState = this.maxStates[i, currentState];
            }

            var cArray = sb.ToString().ToCharArray().Reverse();            
            return new string(cArray.ToArray());
        }
    }
}
