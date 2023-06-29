// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PChecker;
using PChecker.IO.Debugging;
using Plang.Parser;

namespace Plang.Options
{
    internal sealed class PCheckerOptions
    {
        /// <summary>
        /// The command line parser to use.
        /// </summary>
        private readonly CommandLineArgumentParser Parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="PCheckerOptions"/> class.
        /// </summary>
        internal PCheckerOptions()
        {
            Parser = new CommandLineArgumentParser("p check",
                "The P checker enables systematic exploration of a specified P test case, it generates " +
                "a reproducible bug-trace if a bug is found, and also allows replaying a bug-trace.");

            var basicOptions = Parser.GetOrCreateGroup("Basic", "Basic options");
            basicOptions.AddPositionalArgument("path", "Path to the compiled file to check for correctness (*.dll)."+
                " If this option is not passed, the compiler searches for a *.dll file in the current folder").IsRequired = false;
            var modes = basicOptions.AddArgument("mode", "md", "Choose a checker mode (options: bugfinding, verification, coverage, pobserve). (default: bugfinding)");
            modes.AllowedValues = new List<string>() { "bugfinding", "verification", "coverage", "pobserve" };
            modes.IsHidden = true;
            basicOptions.AddArgument("testcase", "tc", "Test case to explore");

            var basicGroup = Parser.GetOrCreateGroup("Basic", "Basic options");
            basicGroup.AddArgument("timeout", "t", "Timeout in seconds (disabled by default)", typeof(uint));
            basicGroup.AddArgument("memout", null, "Memory limit in Giga bytes (disabled by default)", typeof(double)).IsHidden = true;
            basicGroup.AddArgument("outdir", "o", "Dump output to directory (absolute or relative path)");
            basicGroup.AddArgument("verbose", "v", "Enable verbose log output during exploration", typeof(bool));
            basicGroup.AddArgument("debug", "d", "Enable debugging", typeof(bool)).IsHidden = true;
            
            var exploreGroup = Parser.GetOrCreateGroup("explore", "Systematic exploration options");
            exploreGroup.AddArgument("iterations", "i", "Number of schedules to explore", typeof(uint));
            exploreGroup.AddArgument("max-steps", "ms", @"Max scheduling steps to be explored during systematic exploration (by default 10,000 unfair and 100,000 fair steps). You can provide one or two unsigned integer values", typeof(uint)).IsMultiValue = true;
            exploreGroup.AddArgument("fail-on-maxsteps", null, "Consider it a bug if the test hits the specified max-steps", typeof(bool));
            exploreGroup.AddArgument("liveness-temperature-threshold", null, "Specify the liveness temperature threshold is the liveness temperature value that triggers a liveness bug", typeof(uint)).IsHidden = true;
            
            var schedulingGroup = Parser.GetOrCreateGroup("scheduling", "Search prioritization options");
            schedulingGroup.AddArgument("sch-random", null, "Choose the random scheduling strategy (this is the default)", typeof(bool));
            schedulingGroup.AddArgument("sch-feedback", null, "Choose the feedback scheduling strategy", typeof(bool));
            schedulingGroup.AddArgument("sch-2stagefeedback", null, "Choose the 2 stage feedback scheduling strategy", typeof(bool));

            schedulingGroup.AddArgument("sch-feedbackpct", null, "Choose the 2 stage feedback scheduling strategy", typeof(uint));
            schedulingGroup.AddArgument("sch-2stagefeedbackpct", null, "Choose the 2 stage feedback scheduling strategy", typeof(uint));

            schedulingGroup.AddArgument("sch-probabilistic", "sp", "Choose the probabilistic scheduling strategy with given probability for each scheduling decision where the probability is " +
                                                                   "specified as the integer N in the equation 0.5 to the power of N.  So for N=1, the probability is 0.5, for N=2 the probability is 0.25, N=3 you get 0.125, etc.", typeof(uint));
            schedulingGroup.AddArgument("sch-pct", null, "Choose the PCT scheduling strategy with given maximum number of priority switch points", typeof(uint));
            schedulingGroup.AddArgument("sch-fairpct", null, "Choose the fair PCT scheduling strategy with given maximum number of priority switch points", typeof(uint));
            schedulingGroup.AddArgument("sch-rl", null, "Choose the reinforcement learning (RL) scheduling strategy", typeof(bool)).IsHidden = true;
            var schCoverage = schedulingGroup.AddArgument("sch-coverage", null, "Choose the scheduling strategy for explicit-state search in coverage mode (options: random, dfs, learn). (default: learn)");
            schCoverage.AllowedValues = new List<string>() { "random", "dfs", "learn" };
            schCoverage.IsHidden = true;

            var replayOptions = Parser.GetOrCreateGroup("replay", "Replay and debug options");
            replayOptions.AddArgument("replay", "r", "Schedule file to replay");
            
            var advancedGroup = Parser.GetOrCreateGroup("advanced", "Advanced options");
            advancedGroup.AddArgument("explore", null, "Keep testing until the bound (e.g. iteration or time) is reached", typeof(bool));
            advancedGroup.AddArgument("seed", null, "Specify the random value generator seed", typeof(uint));
            advancedGroup.AddArgument("graph-bug", null, "Output a DGML graph of the iteration that found a bug", typeof(bool));
            advancedGroup.AddArgument("graph", null, "Output a DGML graph of all test iterations whether a bug was found or not", typeof(bool));
            advancedGroup.AddArgument("xml-trace", null, "Specify a filename for XML runtime log output to be written to", typeof(bool));
            advancedGroup.AddArgument("unbiased", null, "Use unbiased sampling based on input pattern", typeof(bool));
            advancedGroup.AddArgument("pattern", null, "The path to the pattern file.", typeof(string));

        }

        /// <summary>
        /// Parses the command line options and returns a checkerConfiguration.
        /// </summary>
        /// <returns>The CheckerConfiguration object populated with the parsed command line options.</returns>
        internal CheckerConfiguration Parse(string[] args)
        {
            var configuration = CheckerConfiguration.Create();
            try
            {
                var result = Parser.ParseArguments(args);
                foreach (var arg in result)
                {
                    UpdateConfigurationWithParsedArgument(configuration, arg);
                }

                // if P compiled file is not set, then search for the compiled dll/jar file locally
                FindLocalPCompiledFile(configuration);

                SanitizeConfiguration(configuration);
            }
            catch (CommandLineException ex)
            {
                if ((from arg in ex.Result where arg.LongName == "version" select arg).Any())
                {
                    WriteVersion();
                    Environment.Exit(1);
                }
                else
                {
                    Parser.PrintHelp(Console.Out);
                    Error.ReportAndExit(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Parser.PrintHelp(Console.Out);
                Error.ReportAndExit(ex.Message);
            }

            return configuration;
        }

        /// <summary>
        /// Updates the checkerConfiguration with the specified parsed argument.
        /// </summary>
        private static void UpdateConfigurationWithParsedArgument(CheckerConfiguration checkerConfiguration, CommandLineArgument option)
        {
            switch (option.LongName)
            {
                case "outdir":
                    checkerConfiguration.OutputFilePath = (string)option.Value;
                    break;
                case "verbose":
                    checkerConfiguration.IsVerbose = true;
                    break;
                case "debug":
                    checkerConfiguration.EnableDebugging = true;
                    Debug.IsEnabled = true;
                    break;
                case "unbiased":
                    checkerConfiguration.UnbiasedSampling = true;
                    break;
                case "timeout":
                    checkerConfiguration.Timeout = (int)(uint)option.Value;
                    break;
                case "memout":
                    checkerConfiguration.MemoryLimit = (double)option.Value;
                    break;
                case "path":
                    checkerConfiguration.AssemblyToBeAnalyzed = (string)option.Value;
                    break;
                case "mode":
                    switch ((string)option.Value)
                    {
                        case "bugfinding":
                            checkerConfiguration.Mode = CheckerMode.BugFinding;
                            break;
                        case "verification":
                            checkerConfiguration.Mode = CheckerMode.Verification;
                            break;
                        case "coverage":
                            checkerConfiguration.Mode = CheckerMode.Coverage;
                            break;
                        default:
                            Error.CheckerReportAndExit($"Invalid checker mode '{option.Value}'.");
                            break;
                    }
                    break;
                case "testcase":
                    checkerConfiguration.TestCaseName = (string)option.Value;
                    break;
                case "seed":
                    checkerConfiguration.RandomGeneratorSeed = (uint)option.Value;
                    break;
                case "sch-random":
                case "sch-feedback":
                case "sch-2stagefeedback":
                    checkerConfiguration.SchedulingStrategy = option.LongName.Substring(4);
                    break;
                case "sch-probabilistic":
                case "sch-pct":
                case "sch-fairpct":
                case "sch-feedbackpct":
                case "sch-2stagefeedbackpct":
                    checkerConfiguration.SchedulingStrategy = option.LongName.Substring(4);
                    checkerConfiguration.StrategyBound = (int)(uint)option.Value;
                    break;
                case "sch-rl":
                    checkerConfiguration.SchedulingStrategy = option.LongName.Substring(4);
                    checkerConfiguration.IsProgramStateHashingEnabled = true;
                    break;
                case "sch-coverage":
                    checkerConfiguration.SchedulingStrategy = (string)option.Value;
                    break;
                case "replay":
                    {
                        var filename = (string)option.Value;
                        var extension = System.IO.Path.GetExtension(filename);
                        if (!extension.Equals(".schedule"))
                        {
                            Error.CheckerReportAndExit("Please give a valid schedule file " +
                                "'--replay x', where 'x' has extension '.schedule'.");
                        }

                        checkerConfiguration.ScheduleFile = filename;
                        checkerConfiguration.SchedulingStrategy = "replay";
                        checkerConfiguration.EnableColoredConsoleOutput = true;
                        checkerConfiguration.DisableEnvironmentExit = false;
                    }

                    break;
                case "iterations":
                    checkerConfiguration.TestingIterations = (int)(uint)option.Value;
                    break;
                case "graph":
                    checkerConfiguration.IsDgmlGraphEnabled = true;
                    checkerConfiguration.IsDgmlBugGraph = false;
                    break;
                case "graph-bug":
                    checkerConfiguration.IsDgmlGraphEnabled = true;
                    checkerConfiguration.IsDgmlBugGraph = true;
                    break;
                case "xml-trace":
                    checkerConfiguration.IsXmlLogEnabled = true;
                    break;
                case "explore":
                    checkerConfiguration.PerformFullExploration = true;
                    break;
                case "max-steps":
                    {
                        var values = (uint[])option.Value;
                        if (values.Length > 2)
                        {
                            Error.CheckerReportAndExit("Invalid number of options supplied via '--max-steps'.");
                        }

                        var i = values[0];
                        uint j;
                        if (values.Length == 2)
                        {
                            j = values[1];
                            checkerConfiguration.UserExplicitlySetMaxFairSchedulingSteps = true;
                        }
                        else
                        {
                            j = 10 * i;
                        }

                        checkerConfiguration.MaxUnfairSchedulingSteps = (int)i;
                        checkerConfiguration.MaxFairSchedulingSteps = (int)j;
                    }

                    break;
                case "fail-on-maxsteps":
                    checkerConfiguration.ConsiderDepthBoundHitAsBug = true;
                    break;
                case "pattern":
                    checkerConfiguration.Pattern = File.ReadAllText((string) option.Value);
                    break;
                default:
                    throw new Exception(string.Format("Unhandled parsed argument: '{0}'", option.LongName));
            }
        }

        private static void WriteVersion()
        {
            Console.WriteLine("Version: {0}", typeof(PCheckerOptions).Assembly.GetName().Version);
        }

        /// <summary>
        /// Checks the checkerConfiguration for errors and performs post-processing updates.
        /// </summary>
        private static void SanitizeConfiguration(CheckerConfiguration checkerConfiguration)
        {
            if (checkerConfiguration.LivenessTemperatureThreshold == 0 &&
                checkerConfiguration.MaxFairSchedulingSteps > 0)
            {
                checkerConfiguration.LivenessTemperatureThreshold = checkerConfiguration.MaxFairSchedulingSteps / 2;
            }

            if (checkerConfiguration.SchedulingStrategy != "portfolio" &&
                checkerConfiguration.SchedulingStrategy != "random" &&
                checkerConfiguration.SchedulingStrategy != "feedback" &&
                checkerConfiguration.SchedulingStrategy != "feedbackpct" &&
                checkerConfiguration.SchedulingStrategy != "2stagefeedback" &&
                checkerConfiguration.SchedulingStrategy != "2stagefeedbackpct" &&
                checkerConfiguration.SchedulingStrategy != "pct" &&
                checkerConfiguration.SchedulingStrategy != "fairpct" &&
                checkerConfiguration.SchedulingStrategy != "probabilistic" &&
                checkerConfiguration.SchedulingStrategy != "rl" &&
                checkerConfiguration.SchedulingStrategy != "dfs" &&
                checkerConfiguration.SchedulingStrategy != "replay" &&
                checkerConfiguration.SchedulingStrategy != "learn")
            {
                Error.CheckerReportAndExit("Please provide a scheduling strategy (see --sch* options)");
            }

            if (checkerConfiguration.MaxFairSchedulingSteps < checkerConfiguration.MaxUnfairSchedulingSteps)
            {
                Error.CheckerReportAndExit("For the option '-max-steps N[,M]', please make sure that M >= N.");
            }
        }
        

        private static void FindLocalPCompiledFile(CheckerConfiguration checkerConfiguration)
        {
            if (checkerConfiguration.AssemblyToBeAnalyzed == string.Empty)
            {
                CommandLineOutput.WriteInfo(".. Searching for a P compiled file locally in the current folder");
                
                string filePattern =  checkerConfiguration.Mode switch
                {
                    CheckerMode.BugFinding => "*.dll",
                    CheckerMode.Verification => "*-jar-with-dependencies.jar",
                    CheckerMode.Coverage => "*-jar-with-dependencies.jar",
                    _ => "*.dll"
                };
                
                var enumerationOptions = new EnumerationOptions();
                enumerationOptions.RecurseSubdirectories = true;
                enumerationOptions.MaxRecursionDepth = 3;
                
                
                var files = 
                    from file in Directory.GetFiles(Directory.GetCurrentDirectory(), filePattern, enumerationOptions)
                    let info = new FileInfo(file)
                    where (((info.Attributes & FileAttributes.Hidden) ==0)& ((info.Attributes & FileAttributes.System)==0))
                    select file;

                foreach (var fileName in files)
                {
                    if (checkerConfiguration.Mode == CheckerMode.BugFinding)
                    {
                        if (!fileName.Contains("CSharp/"))
                            continue;
                        if (fileName.EndsWith("PCheckerCore.dll") 
                            || fileName.EndsWith("PCSharpRuntime.dll")
                            || fileName.EndsWith("/P.dll")
                            || fileName.EndsWith("/Antlr4.Runtime.dll")
                            || fileName.EndsWith("/p.dll"))
                            continue;
                    }
                    else if (checkerConfiguration.Mode == CheckerMode.Verification || checkerConfiguration.Mode == CheckerMode.Coverage)
                    {
                        if (!fileName.Contains("Symbolic/"))
                            continue;
                    }
                    else
                    {
                        if (!fileName.Contains("Java/"))
                            continue;
                    }
                    checkerConfiguration.AssemblyToBeAnalyzed = fileName;
                    CommandLineOutput.WriteInfo($".. Found a P compiled file: {checkerConfiguration.AssemblyToBeAnalyzed}");
                    break;
                }
                
                if (checkerConfiguration.AssemblyToBeAnalyzed == string.Empty)
                {
                    CommandLineOutput.WriteInfo(
                        $".. No P compiled file {filePattern} found in the current folder: {Directory.GetCurrentDirectory()}");
                    Error.CheckerReportAndExit($"Could not find any {filePattern} file.");
                }
            }
        }
        
    }
}
