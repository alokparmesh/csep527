using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    public static class StringExtensions
    {
        /// <summary>
        /// Reverse the string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }

        /// <summary>
        /// Helper method to align string start for indexes
        /// </summary>
        /// <param name="firstSeqIndex"></param>
        /// <param name="secondSeqIndex"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> GetIndexStrings(int firstSeqIndex, int secondSeqIndex)
        {
            int fL = firstSeqIndex.ToString().Length;
            int sL = secondSeqIndex.ToString().Length;
            int maxL = Math.Max(fL, sL);
            return Tuple.Create(
                string.Concat(new String(' ', maxL - fL), firstSeqIndex.ToString(), " "),
                new String(' ', maxL + 1),
                string.Concat(new String(' ', maxL - sL), secondSeqIndex.ToString(), " ")
                );
        }
    }
}
