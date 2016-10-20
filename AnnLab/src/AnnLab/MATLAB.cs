using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnLab
{
    public class MATLAB
    {

        public static string AddPath { get; set; }

        public static bool RunScript(string logfile, string funccall, params string[] funcargs)
        {
            List<string> matlabCode = new List<string>
            {
                "try,",
                    "addpath('" + AddPath.Replace("\\", "\\\\") + "');",
                    funccall + "(" + string.Join(", ", funcargs) + ");",
                    "exit(0);",
                "catch ME,",
                    "disp(getReport(ME));",
                    "exit(1);",
                "end"
            };
            List<string> args = new List<string>
            {
                "-nosplash",
                "-nodesktop",
                "-noFigureWindows",
                "-minimize",
                "-wait",
                "-logfile \"" + logfile + "\"",
                "-r \"" + string.Join(" ", matlabCode) + "\""
            };

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            startInfo.FileName = "matlab";
            startInfo.Arguments = string.Join(" ", args);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            if (process.ExitCode == 0 && File.Exists(logfile))
                File.Delete(logfile);
            return process.ExitCode == 0;
        }

    }
}
