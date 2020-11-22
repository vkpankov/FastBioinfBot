using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastBioinfBot.BioinfToolWrappers
{
    internal static class FastQCWrapper
    {
        public static bool ProcessFastqFile(string fileName, out string allResultsFileName, out string resultsInHtmlFileName)
        {
            var fastqcPath = "BioinformaticsTools/FastQC";
            var curdir = System.Environment.CurrentDirectory;
            string fastQCRun = $"-Xmx250m -classpath {fastqcPath};./{fastqcPath}/sam-1.103.jar;./{fastqcPath}/jbzip2-0.9.jar uk.ac.babraham.FastQC.FastQCApplication {fileName}";
            var processResult = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "java.exe",
                    Arguments = fastQCRun,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            processResult.Start();
            processResult.WaitForExit();
            string stdout = processResult.StandardError.ReadToEnd();
            string path = Path.GetDirectoryName(fileName);
            if (stdout.Contains("complete for"))
            {
                allResultsFileName = path +"\\" + Path.GetFileNameWithoutExtension(fileName) + "_fastqc.zip";
                resultsInHtmlFileName = path + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_fastqc.html";
                return true;
            }
            else
            {
                allResultsFileName = "";
                resultsInHtmlFileName = "";
                return false;
            }

        }
    }
}
