using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SequenceAligner
{
    /// <summary>
    /// Type of alignment
    /// </summary>
    public enum AlignmentType
    {
        Local,
        Global
    }

    /// <summary>
    /// Various options to alignment
    /// </summary>
    public class SequenceAlignerOption
    {
        [OptionList("sequences", Separator = ',', HelpText = "two sequences for comparision")]
        public List<string> Sequences { get; set; }

        [OptionList("accessions", Separator =',', HelpText = "two accessions for comparision")]
        public List<string> Accessions { get; set; }

        [Option("alignmentType", DefaultValue = AlignmentType.Local, HelpText = "alignment type local or global")]
        public AlignmentType AlignmentType { get; set; }

        [Option("scoretype", DefaultValue = "BLOSUM62", HelpText = "blosum score type")]
        public string ScoreType { get; set; }

        [Option("gapCost", DefaultValue = -4, HelpText = "gap cost")]
        public int GapCost { get; set; }

        [Option("pValue", DefaultValue = false, HelpText = "calculate p-value")]
        public bool PValue { get; set; }

        [Option("full", DefaultValue = false, HelpText = "print full matrix")]
        public bool Full { get; set; }

        /// <summary>
        /// Gets the usage.
        /// </summary>
        /// <returns>Usage</returns>
        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("SequenceAligner.exe");
            usage.AppendLine(@"--sequences=""deadly, ddgearlyk"" --alignmentType=Global --pValue --full");           
            return usage.ToString();
        }
    }
}
