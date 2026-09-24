namespace LavaEngine.Core;

using LavaEngine.Log;
using LavaEngine.Core.Window.OpenGLWindow;
using LavaEngine.Core.Window.VulkanWindow;
using System;
using System.Diagnostics;
using System.IO; // Added missing using for File/Directory operations
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using LavaEngine.Core.ScriptManagerCore;
using System.Collections.Generic;
using System.Linq;

public class Core
{
    public int CompleProject(string ProjectPath)
    {
        Log.Logger logger = new Log.Logger();
        logger.Info("Starting Lava Engine");

        // Loading engine configuration
        logger.Trace("Reading engine configuration...");
        XmlDocument engine_config = new XmlDocument();
        engine_config.Load("Config.xml");
        
        string engine_version = engine_config.SelectSingleNode("Config/Version")?.InnerText;
        logger.Trace($"Read configuration: engine version: {engine_version}");
        
        string debug_mode = engine_config.SelectSingleNode("Config/Debug_Mode")?.InnerText;
        logger.Trace($"Read configuration: debug mode: {debug_mode}");

        if (debug_mode == "1")
        {
            logger.SetMinLogLevel(Logger.LogLevel.Trace);
            logger.Trace("Debug mode enabled. All messages will be logged.");
        }
        else if (debug_mode == "0")
        {
            logger.SetMinLogLevel(Logger.LogLevel.Info);
            logger.Info("Debug mode disabled, Trace and Debug messages will not be logged.");
        }

        // Load project
        logger.Trace("Reading file...");
        
        // Check if source file exists
        if (!File.Exists(ProjectPath))
        {
            logger.Error("Source file does not exist");
            return -1;
        }
        else
        {
            logger.Info("Found project file successfully.");
        }

        logger.Info("Start compiling project");

        // Compile the project
        logger.Trace("Loading project file...");
        XmlDocument proj = new XmlDocument();
        proj.Load(ProjectPath);
        logger.Info("Loaded project file.");

        if (!IsXmlNodeRoot(proj, "LavaProject"))
        {
            logger.Error("Invalid project file format. Root node must be <LavaProject>.");
            return -1;
        }

        XmlNode lproj = proj.SelectSingleNode("LavaProject");

        // Check if necessary nodes are present
        if (lproj.SelectSingleNode("ProjConfiguration") == null)
        {
            logger.Error("Invalid project file format. <ProjConfiguration> node is missing.");
            return -1;
        }
        else if (lproj.SelectSingleNode("Scene") == null)
        {
            logger.Error("Invalid project. No game scenes are found.");
            return -1;
        }

        // Read project configuration
        logger.Trace("Reading project configuration...");
        XmlNode projConfig = lproj.SelectSingleNode("ProjConfiguration");

        // Check necessary nodes in project configuration
        if (projConfig.SelectSingleNode("ProjName") == null)
        {
            logger.Error("Project name not set.");
            return -1;
        }
        else if (projConfig.SelectSingleNode("RenderingAPI") == null)
        {
            logger.Error("Rendering API not set.");
        }
        else if (projConfig.SelectSingleNode("CompilingMode") == null)
        {
            logger.Error("Compiling mode not set.");
            return -1;
        }

        logger.Trace($"Read project configuration: Project name: {projConfig.SelectSingleNode("ProjName").InnerText}");
        logger.Trace($"Read project configuration: Rendering API: {projConfig.SelectSingleNode("RenderingAPI").InnerText}");
        logger.Trace($"Read project configuration: Compiling mode: {projConfig.SelectSingleNode("CompilingMode").InnerText}");

        // Check if scripts/ folder is in the project path
        logger.Trace("Checking scripts folder...");
        string projectDir = Path.GetDirectoryName(ProjectPath);
        
        if (!Directory.Exists(Path.Combine(projectDir, "scripts")))
        {
            logger.Error("Scripts folder not found in project directory.");
            return -1;
        }

        // Check if necessary exists
        if (!File.Exists(Path.Combine(projectDir, "scripts/ProjectScripts/projectInit.cs")))
        {
            logger.Error("projectInit.cs not found.");
            return -1;
        } else if (!File.Exists(Path.Combine(projectDir, "scripts/ProjectScripts/projectDestroy.cs")))
        {
            logger.Error("projectDestroy.cs not found.");
            return -1;
        }


        logger.Info("Scripts folder found. Init script has been created.");

        logger.Trace("Checking dotnet version...");
        if (!IsDotNet9OrHigherInstalled())
        {
            logger.Error("Dotnet 9 or higher is not installed.");
            return -1;
        }
        logger.Trace("Dotnet 9 or higher is installed.");

        string projectName = projConfig.SelectSingleNode("ProjName").InnerText;
        string solutionPath = Path.Combine(projectDir, $"{projectName}.slnx");

        if (!File.Exists(solutionPath))
        {
            logger.Info("Solution has not been created yet. The Engine will create it.");
            logger.Warning("Do not edit any file generated by the engine. Or your project will be broken.");

            // Create slnx solution
            logger.Trace("Creating solution file...");
            var processInfo1 = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new sln -n {projectName}",
                WorkingDirectory = projectDir,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processInfo1))
            {
                process.WaitForExit();
            }
            logger.Info("Created solution file");

            // Create project for scripts
            logger.Trace("Creating scripts project...");
            var processInfo2 = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new classlib -n scripts --force",
                WorkingDirectory = projectDir,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processInfo2))
            {
                process.WaitForExit();
            }
            logger.Info("Created scripts project");

            // Add reference
            logger.Trace("Adding LavaEngineScripts reference to scripts project...");
            string scriptCsProjPath = Path.Combine(projectDir, "scripts/scripts.csproj");
            XDocument script_csproj = XDocument.Load(scriptCsProjPath);
            
            // Create a new ItemGroup for the reference
            XElement itemGroup = new XElement("ItemGroup");
            script_csproj.Root.Add(itemGroup);
            
            XElement referenceNode = new XElement("Reference",
                new XAttribute("Include", "LavaEngineScripts"),
                new XElement("HintPath", AppDomain.CurrentDomain.BaseDirectory + "LavaEngineScripts.dll")
            );
            itemGroup.Add(referenceNode);
            script_csproj.Save(scriptCsProjPath);
            
            logger.Info("Added LavaEngineScripts reference to scripts project.");

            // Add scripts project to slnx
            logger.Trace("Adding scripts project to solution...");
            var processInfo3 = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"sln add scripts/scripts.csproj -s",
                WorkingDirectory = projectDir,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = Process.Start(processInfo3))
            {
                process.WaitForExit();
            }
            logger.Info("Added scripts project to solution.");

            // Delete default generated Class1.cs
            logger.Trace("Deleting default generated scripts/Class1.cs...");
            File.Delete(Path.Combine(projectDir, "scripts/Class1.cs"));
        }

        logger.Trace("Reading compiling mode...");
        string CompilingMode = projConfig.SelectSingleNode("CompilingMode").InnerText;

        if (!(CompilingMode == "Debug" || CompilingMode == "Release"))
        {
            logger.Error("Invalid compiling mode.");
            return -1;
        }

        // Compile scripts
        logger.Trace($"Compiling scripts in mode {CompilingMode}...");
        var processInfo4 = new ProcessStartInfo
        {
            FileName = "dotnet",
            // Added --tl:off to disable the new terminal logger (which hides error details)
            // Added -v:n for normal verbosity to show the actual code snippet where the error occurred
            Arguments = $"build scripts/scripts.csproj -c {CompilingMode} -o bin/{CompilingMode}/ --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true --tl:off -v:n",
            WorkingDirectory = projectDir,
            CreateNoWindow = true,
            UseShellExecute = false,
            // Redirect standard output and error streams to capture them in memory
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // Force the output encoding to UTF-8 to fix the Chinese character garbled text issue
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        using (var process = Process.Start(processInfo4))
        {
            // Read the output streams (must be read to prevent the process from hanging)
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            // Wait for the compilation to finish
            process.WaitForExit();

            // Check if compilation failed (ExitCode != 0 means there are errors)
            if (process.ExitCode != 0)
            {
                // Combine standard output and standard error
                string fullLog = output + Environment.NewLine + error;
                
                // Filter out warnings, only keep lines containing "error"
                var errorLines = fullLog
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(line => line.Contains("error", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (errorLines.Any())
                {
                    string finalErrorMessage = string.Join(Environment.NewLine, errorLines);
                    logger.Error($"Failed to Compile Scripts\n{finalErrorMessage}");
                    return -1;
                }
                else
                {
                    // Fallback: if no lines with "error" are found, log the full log
                    logger.Error($"Failed to Compile Scripts with unknown error!\nFull Log:\n{fullLog}");
                    return -1;
                }
            }
        }
        
        // If ExitCode == 0, compilation succeeded, ignore all output (including warnings)
        logger.Info("Successfully compiled scripts.");

        ScriptManager manager = new ScriptManager();

        logger.Trace("Loading LavaProjectInit class...");
        Type init_class = manager.GetScriptClass(manager.LoadScripts(ProjectPath, CompilingMode), 
                                                "LavaProjectInit");
        if (init_class == null)
        {
            logger.Error("can't find LavaProjectInit class");
            return -1;
        }
        logger.Info("Successfully loaded LavaProjectInit class.");

        //--------------------------------------------------------------------------------------//
        //           LIFE                                             LOOP                      //
        //--------------------------------------------------------------------------------------//

        logger.Trace("Calling Init() function");
        var result_init = manager.ExecuteMethod(
            manager.GetMethodFromClass(init_class, "Init"),
            Activator.CreateInstance(init_class)
        );
        logger.Info($"Called init function, result is {result_init}");

        logger.Trace("Caching all lifecycle methods...");
        manager.CacheAllMethods(manager.LoadScripts(ProjectPath, CompilingMode), init_class);
        logger.Info("All lifecycle methods cached successfully.");

        // ----------------------------------------------------
        //                 WINDOW     CREATION
        // ----------------------------------------------------
        
        string rendering_api = projConfig.SelectSingleNode("RenderingAPI").InnerText;
        List<string> valid_apis = new List<string> { "OpenGL", "Vulkan"};

        if (!valid_apis.Contains(rendering_api))
        {
            logger.Critical($"Invalid Rendering api: {rendering_api}");
            return -1;
        }

        if (rendering_api == "OpenGL")
        {
            logger.Info("Creating OpenGL Window and entering main loop...");

            OpenGLWindow gameWindow = new OpenGLWindow();
            gameWindow.CreateWindow(manager); 
        }

        if (rendering_api == "Vulkan")
        {
            logger.Info("Creating Vulkan Window and entering main loop...");

            VulkanWindow gameWindow = new VulkanWindow();
            gameWindow.CreateWindow(manager); 
        }

        logger.Trace("Calling ProjectDestroy() function");
        Type destroy_class = manager.GetScriptClass(manager.LoadScripts(ProjectPath, CompilingMode), 
                                                "LavaProjectDestroy");
        var result_destroy = manager.ExecuteMethod(
            manager.GetMethodFromClass(destroy_class, "Destroy"),
            Activator.CreateInstance(destroy_class)
        );
        logger.Info($"Called destroy function, result is {result_init}");

        logger.Info("Game loop ended");

        logger.Dispose();

        return 0;
    }

    private bool IsXmlNodeRoot(XmlDocument doc, string rootName)
    {
        return doc.DocumentElement.Name == rootName;
    }

    private static bool IsDotNet9OrHigherInstalled()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (string.IsNullOrWhiteSpace(output)) return false;

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    string versionString = line.Split(' ')[0];
                    if (Version.TryParse(versionString, out Version version))
                    {
                        if (version.Major >= 9)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger logger = new Logger();
            logger.Error($"Failed to check dotnet: {ex.Message}");
        }
        return false;
    }
}