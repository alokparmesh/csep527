using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ParameterEstimation
{
    /// <summary>
    /// Various options to alignment
    /// </summary>
    public class ParameterEstimationOption
    {
        [Option("inputfile", DefaultValue = "input.txt", HelpText = "input file")]
        public string InputFile { get; set; }

        [Option("mixtureCount", DefaultValue = 3, HelpText = "number of mixtures")]
        public int MixtureCount { get; set; }

        /// <summary>
        /// Gets the usage.
        /// </summary>
        /// <returns>Usage</returns>
        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("ParameterEstimation.exe");
            usage.AppendLine(@"--inputfile=inputfilename --mixtureCount=4");
            return usage.ToString();
        }
    }
}
