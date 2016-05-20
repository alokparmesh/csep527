using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viterbi
{
    public class HmmParameters
    {
        /// <summary>
        /// Number of hidden states
        /// </summary>
        public readonly int numHiddenStates;

        /// <summary>
        /// Emission probabilites
        /// </summary>
        public Dictionary<char, double>[] EmissionProbabilities { get; private set; }

        /// <summary>
        /// Transition probabilities among hidden states
        /// </summary>
        public double[,] TransitionProbabilities { get; private set; }

        /// <summary>
        /// Initial transition parameters
        /// </summary>
        public double[] InitialTransitionProbabilities { get; private set; }

        public HmmParameters(int numHiddenStates)
        {
            this.numHiddenStates = numHiddenStates;
            this.TransitionProbabilities = new double[numHiddenStates, numHiddenStates];
            this.InitialTransitionProbabilities = new double[numHiddenStates];
            this.EmissionProbabilities = new Dictionary<char, double>[numHiddenStates];
        }

        /// <summary>
        /// Print parameters
        /// </summary>
        public void Print()
        {
            Console.WriteLine("Transitions");
            for (int i = 0; i < TransitionProbabilities.GetLength(0); i++)
            {
                Console.WriteLine("State #{0}", i);
                for (int j = 0; j < TransitionProbabilities.GetLength(1); j++)
                {
                    Console.Write("to {0} : {1}\t", j, TransitionProbabilities[i,j].ToString("#.000000"));
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("Emissions");
            for (int i = 0; i < EmissionProbabilities.Length; i++)
            {
                Console.WriteLine("State #{0}",i);
                var list = EmissionProbabilities[i].Keys.ToList();
                list.Sort();

                foreach (var item in list)
                {
                    Console.Write("{0} : {1}\t", item, EmissionProbabilities[i][item].ToString("#.000000"));
                }
                Console.WriteLine();
            }
        }
    }
}
