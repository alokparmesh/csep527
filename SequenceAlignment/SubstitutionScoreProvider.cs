using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Substitution score interface
    /// </summary>
    public interface IScoreProvider
    {
        int GetScore(char aminoAcid1, char aminoAcid2);
    }

    /// <summary>
    /// Get the substitution score by reading text file which contains the score in matrix format
    /// with # as comments
    /// </summary>
    public class SubstitutionScoreProvider : IScoreProvider
    {
        public const string BLOSUM62 = "BLOSUM62";
        public const string BLOSUM50 = "BLOSUM50";

        private string scoreType;
        private Dictionary<char, int> positionDictionary;
        private int[,] costMatrix;
        public SubstitutionScoreProvider(string scoreType)
        {
            if (!BLOSUM62.Equals(scoreType) && !BLOSUM50.Equals(scoreType))
            {
                throw new Exception("Unsupported substitution score type requested");
            }

            this.scoreType = scoreType;
            Init();
        }

        public int GetScore(char aminoAcid1, char aminoAcid2)
        {
            return costMatrix[positionDictionary[aminoAcid1], positionDictionary[aminoAcid2]];
        }

        public void Print()
        {
            int rowLength = this.costMatrix.GetLength(0);
            int colLength = this.costMatrix.GetLength(1);

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < colLength; j++)
                {
                    Console.Write(string.Format("{0} ", this.costMatrix[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        private void Init()
        {
            this.positionDictionary = new Dictionary<char, int>();
            bool headerRow = true;
            string fileName = string.Format("{0}.txt", scoreType);
            if (File.Exists(fileName))
            {
                var sr = new StreamReader(fileName);
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    if (s == null || string.IsNullOrWhiteSpace(s) || s.Trim().StartsWith("#"))
                    {
                        // empty or comment;
                        continue;
                    }

                    // first non-empty row is header
                    if (headerRow)
                    {
                        headerRow = false;
                        string[] aminoAcids = s.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < aminoAcids.Length; i++)
                        {
                            this.positionDictionary.Add(aminoAcids[i][0], i);
                            this.costMatrix = new int[aminoAcids.Length, aminoAcids.Length];
                        }
                    }
                    else
                    {
                        string[] arr = s.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        int rowPosition = this.positionDictionary[arr[0][0]];
                        for (int i = 1; i < arr.Length; i++)
                        {
                            this.costMatrix[rowPosition, i - 1] = Convert.ToInt32(arr[i]);
                        }
                    }
                }

                sr.Close();
            }
            else
            {
                throw new Exception(string.Format("Missing file {0}", fileName));
            }
        }
    }
}
