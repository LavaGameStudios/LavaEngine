// Program.cs
using System;
using System.Text.RegularExpressions;
using static LavaEngine.Engine; 

namespace TargetProjectRunner
{
    class Program
    {
        // ANSI color codes for terminal output
        private const string RedColor = "\x1b[91m";
        private const string GreenColor = "\x1b[92m";
        private const string ResetColor = "\x1b[0m";

        static int Main(string[] args)
        {
            // Check if no arguments provided
            if (args.Length == 0)
            {
                PrintError("Error: No target project");
                return 1;
            }

            // Check if first argument is --target
            if (args[0] != "--target")
            {
                PrintError($"Error: Unknown command: {args[0]}");
                return 1;
            }

            // Must have exactly 2 arguments: --target and filename
            if (args.Length == 1)
            {
                PrintError("Error: No target project");
                return 1;
            }

            // Check if there are extra arguments after target value
            if (args.Length > 2)
            {
                PrintError($"Error: Unknown command: {args[2]}");
                return 1;
            }

            string targetProject = args[1];

            // Validate .lproj extension
            if (!Regex.IsMatch(targetProject, @"^.+\.lproj$", RegexOptions.IgnoreCase))
            {
                PrintError("Error: Invalid project file");
                return 1;
            }

            // Success output in green
            PrintSuccess($"Info: Choosed target project: {targetProject}");

            // throw the project file to the engine to compile and run
            LavaEngine.Engine engine = new LavaEngine.Engine();
            engine.Compile(targetProject);
            return 0;
        }

        // Print error message in red
        private static void PrintError(string message)
        {
            if (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major < 10)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.Error.WriteLine($"{RedColor}{message}{ResetColor}");
            }
        }

        // Print success message in green
        private static void PrintSuccess(string message)
        {
            if (OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major < 10)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ForegroundColor = originalColor;
            }
            else
            {
                Console.WriteLine($"{GreenColor}{message}{ResetColor}");
            }
        }
    }
}