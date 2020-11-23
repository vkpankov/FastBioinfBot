using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastBioinfBot.BioinfToolWrappers
{
    public static class TrimmomaticWrapper
    {
        public static bool ProcessFilesByTrimmomatic
            (TrimmomaticInputParams inputParams)
        {
             
            var trimmomaticPath = "BioinformaticsTools/Trimmomatic";
            var curdir = System.Environment.CurrentDirectory;
            //java -jar trimmomatic-0.39.jar PE test_1.fastq test_2.fastq outforward_paired.7z forward_unpaired.7z outreverse_paired.7z reverse_unpaired.7z LEADING:3 TRAILING:3 MINLEN:36
            string trimmomaticCmd = "";
            if (inputParams.Mode == "Single end")
            {
                trimmomaticCmd = $"-jar {trimmomaticPath}/trimmomatic-0.39.jar SE -phred33 \"{inputParams.ForwardReadsFileName}\" forward_paired.7z LEADING:{inputParams.Leading} TRAILING:{inputParams.Trailing} MINLEN:{inputParams.MinLen}";
            }
            else
            {
                trimmomaticCmd = $"-jar {trimmomaticPath}/trimmomatic-0.39.jar PE ] {inputParams.ForwardReadsFileName} {inputParams.ReverseReadsFileName} forward_paired.7z forward_unpaired.7z reverse_paired.7z reverse_unpaired.7z LEADING:{inputParams.Leading} TRAILING:{inputParams.Trailing} MINLEN:{inputParams.MinLen}";
            }
            //$"-jar {trimmomaticPath}/trimmomatic-0.39.jar {mode} {forwardFileName} {reverseFileName} ";
            var processResult = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "java.exe",
                    Arguments = trimmomaticCmd,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            processResult.Start();
            processResult.WaitForExit();
            string stdout = processResult.StandardError.ReadToEnd();
            if (stdout.Contains("successfully"))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
