using System;
using System.Diagnostics;

namespace Dotnet.Test.Runner
{
    class Program
    {
        static ConsoleColor OriginalColor = Console.ForegroundColor;
        static ConsoleColor Color = ConsoleColor.Black;
        static bool PreParsing = false;
        static bool TreatNextAsError = false;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Black;
            var i = 0;
            bool verbosityChecked = false;
            string[] arguments = new string[args.Length];
            Array.Copy(args, 0, arguments, 0, args.Length);
            while(i< arguments.Length || !verbosityChecked)
            {
                if (arguments[i].Equals("-v") || arguments[i].Equals("--verbosity"))
                {
                    verbosityChecked = true;
                    var verbosityMode = arguments[i + 1];
                    if (verbosityMode.Equals("p"))
                    {
                        PreParsing = true;
                        arguments[i + 1] = "n";
                    }
                }
                i++;
            }
            var argumentsAsString = string.Join(' ', arguments);
            
            var dotnetProcess = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", argumentsAsString)
                {
                    RedirectStandardOutput = true
                }
            };
            if (PreParsing)
            {dotnetProcess.OutputDataReceived += DotnetProcess_OutputDataReceived;
            }
            Color = Console.ForegroundColor;
            dotnetProcess.Start();
            if (PreParsing)
            {
                dotnetProcess.BeginOutputReadLine();
            }
            dotnetProcess.WaitForExit();
            dotnetProcess.OutputDataReceived -= DotnetProcess_OutputDataReceived;
            Console.ForegroundColor = OriginalColor;
        }
        private static void DotnetProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (PreParsing)
            {
                Color = TreatNextAsError ? ConsoleColor.Red : ConsoleColor.White;
                if (!(e?.Data is null))
                {
                    if (e.Data.Contains(" V ") || e.Data.Contains(" X ") || TreatNextAsError)
                    {
                        var message = e.Data;
                        if (message.Contains(" V "))
                        {
                            Color = ConsoleColor.Green;
                            TreatNextAsError = false;
                            // successful test
                        }
                        if (message.Contains(" X "))
                        {
                            //unsuccessful test
                            Color = ConsoleColor.Red;
                            TreatNextAsError = true;
                        }
                        Console.ForegroundColor = Color;
                        Console.WriteLine($"{message}");
                    }
                    else
                    {
                        if (e.Data.Contains("Test Run") || e.Data.Contains("Total ") || e.Data.Contains("Passed:") || e.Data.Contains("Failed:"))
                        {
                            Color = OriginalColor;
                            Console.ForegroundColor = Color;
                            Console.WriteLine(e.Data);
                        }
                    }
                }
            }
        }
    }
}
