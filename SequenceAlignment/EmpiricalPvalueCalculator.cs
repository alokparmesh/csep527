using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Calculate empirical pvalue by permuting and counting sequences which on alignment 
    /// result in equal or better than given score  
    /// </summary>
    public class EmpiricalPvalueCalculator
    {
        private static Random random = new Random();
        private ISequenceAligner aligner;
        public EmpiricalPvalueCalculator(ISequenceAligner aligner)
        {
            this.aligner = aligner;
        }

        public double CalculatePValue(Sequence seq1, Sequence seq2, int optimalScore, int maxTrials)
        {
            double betterScore = 0.0;
            for (int i = 0; i < maxTrials; i++)
            {
                Sequence randSeq = EmpiricalPvalueCalculator.Permutate(seq2);
                var result = this.aligner.Align(seq1, randSeq, false);

                if(result.Score >= optimalScore)
                {
                    betterScore++;
                }
            }

            return (betterScore + 1) / (maxTrials + 1);
        }

        private static Sequence Permutate(Sequence seq2)
        {            
            char[] charArray = seq2.SequenceString.ToCharArray();

            for (int i = charArray.Length -1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = charArray[j];
                charArray[j] = charArray[i];
                charArray[i] = temp;
            }

            return new Sequence(seq2.Accession, new string(charArray));
        }
    }
}
