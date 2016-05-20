using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    /// <summary>
    /// Result of Viterbi HMM
    /// </summary>
    public class HmmResult
    {
        /// <summary>
        /// Emitted Sequence
        /// </summary>
        public string EmittedSequence { get; private set; }

        /// <summary>
        /// Hidden States
        /// </summary>
        public int[] HiddenStates { get; set; }

        /// <summary>
        /// Log probability of Viterbi path
        /// </summary>
        public double LogProbability { get; private set; }

        public HmmResult(string emittedSequence, double logProbability)
        {
            if (string.IsNullOrEmpty(emittedSequence))
            {
                throw new ArgumentException("emittedSequence");
            }

            this.EmittedSequence = emittedSequence;
            this.LogProbability = logProbability;
            this.HiddenStates = new int[emittedSequence.Length];
        }

        public void UpdateParameters(HmmParameters hmmParameters)
        {
            double[,] transitions = new double[hmmParameters.numHiddenStates, hmmParameters.numHiddenStates];
            long[] totalTransitionCount = new long[hmmParameters.numHiddenStates];
            long[] totalEmissionCount = new long[hmmParameters.numHiddenStates];

            Dictionary<char, double>[] emissions = new Dictionary<char, double>[hmmParameters.numHiddenStates];
            for (int i = 0; i < emissions.Length; i++)
            {
                emissions[i] = new Dictionary<char, double>();
            }
            emissions[HiddenStates[0]][this.EmittedSequence[0]] = 1;

            for (int i = 1; i < this.EmittedSequence.Length; i++)
            {
                transitions[this.HiddenStates[i - 1], this.HiddenStates[i]] += 1;
                totalTransitionCount[this.HiddenStates[i - 1]] += 1;

                if (!emissions[HiddenStates[i]].ContainsKey(this.EmittedSequence[i]))
                {
                    emissions[HiddenStates[i]][this.EmittedSequence[i]] = 0;
                }

                emissions[HiddenStates[i]][this.EmittedSequence[i]] += 1;
                totalEmissionCount[HiddenStates[i]] += 1;
            }

            for (int i = 0; i < hmmParameters.numHiddenStates; i++)
            {
                hmmParameters.EmissionProbabilities[i] = new Dictionary<char, double>();
                foreach (var item in emissions[i])
                {
                    hmmParameters.EmissionProbabilities[i][item.Key] = item.Value / totalEmissionCount[i];
                }
            }

            for (int i = 0; i < hmmParameters.numHiddenStates; i++)
            {
                for (int j = 0; j < hmmParameters.numHiddenStates; j++)
                {
                    hmmParameters.TransitionProbabilities[i, j] = transitions[i, j] / totalTransitionCount[i];
                }
            }                      
        }
    }
}
