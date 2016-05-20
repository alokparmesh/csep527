using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public static class ViterbiAlgorithm
    {
        /// <summary>
        /// Find Viterbi path and traceback
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="hmmParameters"></param>
        /// <returns></returns>
        public static HmmResult Estimate(string sequence, HmmParameters hmmParameters)
        {
            if (string.IsNullOrEmpty(sequence))
            {
                throw new ArgumentException("sequence");
            }

            int sequenceLength = sequence.Length;
            int finalState = -1;
            int[,] maxStates = new int[sequenceLength, hmmParameters.numHiddenStates];

            double[] previousSequenceProbabilities = new double[hmmParameters.numHiddenStates];
            double[] currentSequenceProbabilities = new double[hmmParameters.numHiddenStates];

            for (int i = 0; i < hmmParameters.numHiddenStates; i++)
            {
                maxStates[0, i] = i;
                previousSequenceProbabilities[i] = Math.Log(hmmParameters.InitialTransitionProbabilities[i]) + Math.Log(hmmParameters.EmissionProbabilities[i][sequence[0]]);
            }

            for (int i = 1; i < sequenceLength; i++)
            {
                char currentEmission = sequence[i];

                for (int k = 0; k < hmmParameters.numHiddenStates; k++)
                {
                    double maxSequenceProbability = double.MinValue;
                    int maxState = -1;

                    for (int j = 0; j < hmmParameters.numHiddenStates; j++)
                    {
                        double sequenceProbability = previousSequenceProbabilities[j]
                            + Math.Log(hmmParameters.TransitionProbabilities[j, k]) + Math.Log(hmmParameters.EmissionProbabilities[k][currentEmission]);

                        if (sequenceProbability > maxSequenceProbability)
                        {
                            maxSequenceProbability = sequenceProbability;
                            maxState = j;
                        }
                    }

                    currentSequenceProbabilities[k] = maxSequenceProbability;
                    maxStates[i, k] = maxState;
                }

                previousSequenceProbabilities = currentSequenceProbabilities;
                currentSequenceProbabilities = new double[hmmParameters.numHiddenStates];
            }

            double maxTotalProbability = double.MinValue;

            for (int k = 0; k < hmmParameters.numHiddenStates; k++)
            {
                if (previousSequenceProbabilities[k] > maxTotalProbability)
                {
                    maxTotalProbability = previousSequenceProbabilities[k];
                    finalState = k;
                }
            }

            return TraceBack(finalState, maxTotalProbability, maxStates, sequence, hmmParameters);
        }

        /// <summary>
        /// Traceback the hidden states from stored max states
        /// </summary>
        /// <param name="finalState"></param>
        /// <param name="finalLogProbability"></param>
        /// <param name="maxStates"></param>
        /// <param name="sequence"></param>
        /// <param name="hmmParameters"></param>
        /// <returns></returns>
        private static HmmResult TraceBack(int finalState, double finalLogProbability, int[,] maxStates, string sequence, HmmParameters hmmParameters)
        {
            HmmResult hmmResult = new HmmResult(sequence, finalLogProbability);

            int i = hmmResult.EmittedSequence.Length;
            int currentState = finalState;

            while (i > 0)
            {
                hmmResult.HiddenStates[i - 1] = currentState;
                i--;
                currentState = maxStates[i, currentState];
            }

            return hmmResult;
        }
    }
}
