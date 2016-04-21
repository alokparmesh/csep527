using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SequenceAligner
{
    /// <summary>
    /// Main program which checks options and sets up alignment calculation
    /// </summary>
    public class Program
    {
        public static int PrintBlockLength = 60;
        public static int Permutations = 1000;
        public static string CostOutputFile = "costMatrix.csv";

        static void Main(string[] args)
        {
            int.TryParse(ConfigurationManager.AppSettings["printBlockLength"],out PrintBlockLength);
            int.TryParse(ConfigurationManager.AppSettings["permutations"], out Permutations);
            CostOutputFile = ConfigurationManager.AppSettings["costOutputFile"];

            SequenceAlignerOption options = new SequenceAlignerOption();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Sequence sequence1 = null;
                Sequence sequence2 = null;

                if (options.Sequences != null)
                {
                    if (options.Sequences.Count > 0 && options.Sequences.Count != 2)
                    {
                        throw new Exception("Exactly 2 sequences supported");
                    }
                    else
                    {
                        sequence1 = new Sequence("Sequence 1", options.Sequences[0].Trim().ToUpper());
                        sequence2 = new Sequence("Sequence 2", options.Sequences[1].Trim().ToUpper());
                    }
                }

                if (options.Accessions != null)
                {
                    if (options.Accessions.Count > 0 && options.Accessions.Count != 2)
                    {
                        throw new Exception("Exactly 2 accession supported");
                    }
                    else
                    {
                        sequence1 = GetSequence(options.Accessions[0].Trim());
                        sequence2 = GetSequence(options.Accessions[1].Trim());
                    }
                }

                if(sequence1 == null || sequence2 == null)
                {
                    throw new Exception("Missing sequence or accession");
                }

                IScoreProvider subProvider = new SubstitutionScoreProvider(options.ScoreType);

                ISequenceAligner aligner;
                if (options.AlignmentType == AlignmentType.Local)
                {
                    aligner = new LocalAligner(subProvider, options.GapCost);
                }
                else
                {
                    aligner = new GlobalAligner(subProvider, options.GapCost);
                }

                IAlignmentResult result = aligner.Align(sequence1, sequence2, true);
                Console.WriteLine(string.Format("Comparing {0} to {1}",sequence1.Accession,sequence2.Accession));
                Console.WriteLine(string.Format("Optimal Score: {0}", result.Score));
                Console.WriteLine("Alignment:");
                result.PrintAlignment(Program.PrintBlockLength);

                if (options.Full)
                {
                    aligner.OutpuCostMatrix(Program.CostOutputFile);
                }

                if(options.PValue)
                {
                    EmpiricalPvalueCalculator emp = new EmpiricalPvalueCalculator(aligner);
                    var pValue = emp.CalculatePValue(sequence1, sequence2, result.Score, Program.Permutations);
                    Console.WriteLine(string.Format("p-value: {0}", pValue));
                }
            }
        }

        private static Sequence GetSequence(string accession)
        {
            string seq = null;
            if (SequenceRetriever.TryRetrieveSequence(accession, out seq))
            {
                return new Sequence(accession, seq);
            }
            else
            {
                throw new Exception(string.Format("Could not find or download sequence for accession {0}", accession));
            }
        }
    }
}
