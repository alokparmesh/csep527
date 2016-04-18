using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SequenceAligner
{
    public enum AlignmentType
    {
        Local,
        Global
    }

    public class SequenceAlignerOption
    {
        [OptionList("sequences", Separator = ',', HelpText = "first sequence's accession")]
        public List<string> Sequences { get; set; }

        [OptionList("accessions", Separator =',', HelpText = "first sequence's accession")]
        public List<string> Accessions { get; set; }

        [Option("alignmentType", DefaultValue = AlignmentType.Local, HelpText = "alignment type local or global")]
        public AlignmentType AlignmentType { get; set; }

        [Option("gapInitiationCost", DefaultValue = -4, HelpText = "gap initiation cost")]
        public int GapInitiationCost { get; set; }

        [Option("gapExtensionCost", DefaultValue = -4, HelpText = "gap extension cost")]
        public int GapExtensionCost { get; set; }

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
            usage.AppendLine(@"--accession1=""P68871"" --accession2=""Q14SN0""");           
            return usage.ToString();
        }
    }
}
