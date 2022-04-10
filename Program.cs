namespace FabricTemplateBuilder;

using ChaseLabs.CLConfiguration.List;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;

public class Program
{
    static void Main(string[] args)
    {
        string exe = AppDomain.CurrentDomain.BaseDirectory;
        string templates = Directory.CreateDirectory(Path.Combine(exe, "templates")).FullName;
        Console.WriteLine($"All templates have to be located in \"{templates}\"!");
        if (Directory.GetFiles(templates, "*.zip", SearchOption.TopDirectoryOnly).Length == 0)
        {
            Console.WriteLine("\nNo Templates found...\n");
            Console.Write("Would you like to download the latest templates from github? [Y/n]: ");
            string downloadConfirm = Console.ReadLine();
            if (string.IsNullOrEmpty(downloadConfirm) || downloadConfirm.ToLower().StartsWith("y"))
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/FabricMC/fabric-example-mod/branches");
                requestMessage.Headers.Add("User-Agent", "FabricTemplateBuilder");
                HttpResponseMessage responseMessage = client.SendAsync(requestMessage).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    string string_json = responseMessage.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrWhiteSpace(string_json))
                    {
                        JArray json = JsonConvert.DeserializeObject<JArray>(string_json);
                        Parallel.ForEach(json, j =>
                        {
                            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://github.com/FabricMC/fabric-example-mod/archive/{j["name"]}.zip");
                            requestMessage.Headers.Add("User-Agent", "FabricTemplateBuilder");
                            HttpResponseMessage responseMessage = client.SendAsync(requestMessage).Result;
                            if (responseMessage.IsSuccessStatusCode)
                            {
                                string name = j["name"].ToString().Equals("master") ? "1.16" : (string)j["name"];
                                Console.WriteLine($"Downloading {name}...");
                                FileStream stream = new(Path.Combine(templates, $"{name}.zip"), FileMode.OpenOrCreate);
                                responseMessage.Content.CopyToAsync(stream).Wait();
                                stream.Flush();
                                stream.Dispose();
                                stream.Close();
                            }
                            else
                            {
                                Console.Error.WriteLine("Had an issue retrieving response from github!!!");
                            }
                        });
                    }
                }
                else
                {
                    Console.Error.WriteLine("Had an issue retrieving response from github!!!");
                    Console.WriteLine("Exiting.....");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Exiting.....");
                return;
            }
        }
        ConfigManager manager = new(Path.Combine(exe, "config", "app.cfg"));
        manager.Add("version", new FileInfo(Directory.GetFiles(templates, "*.zip", SearchOption.TopDirectoryOnly).First()).Name.Replace(".zip", ""));
        manager.Add("base directory", ".");
        manager.Add("author", "Me!");
        manager.Add("description", "This is an example description! Tell everyone what your mod is about!");
        manager.Add("mod_name", "Example Mod");
        manager.Add("mod_id", "modid");
        manager.Add("package", "net.fabricmc");
        manager.Add("entryPoint", "Main");
        manager.Add("ide", "vscode");
        string version = manager.GetConfigByKey("version").Value;
        string baseDirectory = args.Length == 0 ? Path.GetFullPath(".") : Path.GetFullPath(args[0]);
        string author = manager.GetConfigByKey("author").Value;
        string description = manager.GetConfigByKey("description").Value;
        string mod_name = manager.GetConfigByKey("mod_name").Value;
        string mod_id = manager.GetConfigByKey("mod_id").Value;
        string package = manager.GetConfigByKey("package").Value;
        string entryPoint = manager.GetConfigByKey("entryPoint").Value;
        string ide = manager.GetConfigByKey("ide").Value;
        string archiveName = $"{mod_id}-{version}-fabric";
        string mcUsername = "dev";
        Console.WriteLine("\n\nAdd '!' before every question to save as default");
        Console.WriteLine("Leave blank for default value\n");


        // Minecraft Version
        Console.Write($"What version of Minecraft {(string.IsNullOrWhiteSpace(version) ? "" : $"(Default is \"{version}\")")}: ");
        version = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(version.TrimStart('!')))
        {
            version = manager.GetConfigByKey("version").Value;
        }
        else if (version.StartsWith("!"))
        {
            version = manager.GetConfigByKey("version").Value = version.TrimStart('!');
        }

        // Minecraft Version
        Console.Write($"What is your Minecraft Username {(string.IsNullOrWhiteSpace(mcUsername) ? "" : $"(Default is \"{mcUsername}\")")}: ");
        string tmpmcUsername = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(tmpmcUsername.TrimStart('!')))
        {
            mcUsername = tmpmcUsername;
        }

        //Author Name
        Console.Write($"What is the authors name {(string.IsNullOrWhiteSpace(author) ? "" : $"(Default is \"{author}\")")}: ");
        author = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(author.TrimStart('!')))
        {
            author = manager.GetConfigByKey("author").Value;
        }
        else if (author.StartsWith("!"))
        {
            author = manager.GetConfigByKey("author").Value = author.TrimStart('!');
        }

        //Mod Name
        Console.Write($"What is the Mod Name {(string.IsNullOrWhiteSpace(mod_name) ? "" : $"(Default is \"{mod_name}\")")}: ");
        mod_name = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(mod_name.TrimStart('!')))
        {
            mod_name = manager.GetConfigByKey("mod_name").Value;
        }
        else if (mod_name.StartsWith("!"))
        {
            mod_name = manager.GetConfigByKey("mod_name").Value = mod_name.TrimStart('!');
        }

        //Description
        Console.Write($"What is the description {(string.IsNullOrWhiteSpace(description) ? "" : $"(Default is \"{description}\")")}: ");
        description = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(description.TrimStart('!')))
        {
            description = manager.GetConfigByKey("description").Value;
        }
        else if (description.StartsWith("!"))
        {
            description = manager.GetConfigByKey("description").Value = description.TrimStart('!');
        }

        //Mod ID
        Console.Write($"What is the Mod ID {(string.IsNullOrWhiteSpace(mod_id) ? "" : $"(Default is \"{mod_id}\")")}: ");
        mod_id = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(mod_id.TrimStart('!')))
        {
            mod_id = manager.GetConfigByKey("mod_id").Value;
        }
        else if (mod_id.StartsWith("!"))
        {
            mod_id = manager.GetConfigByKey("mod_id").Value = mod_id.TrimStart('!');
        }

        //Package
        Console.Write($"What is the Organization Namespace {(string.IsNullOrWhiteSpace(package) ? "" : $"(Default is \"{package}\")")}: ");
        package = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(package.TrimStart('!')))
        {
            package = manager.GetConfigByKey("package").Value;
        }
        else if (package.StartsWith("!"))
        {
            package = manager.GetConfigByKey("package").Value = package.TrimStart('!');
        }

        //EntryPoint
        Console.Write($"What is the EntryPoint Class Name {(string.IsNullOrWhiteSpace(entryPoint) ? "" : $"(Default is \"{entryPoint}\")")}: ");
        entryPoint = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(entryPoint.TrimStart('!')))
        {
            entryPoint = manager.GetConfigByKey("entryPoint").Value;
        }
        else if (entryPoint.StartsWith("!"))
        {
            entryPoint = manager.GetConfigByKey("entryPoint").Value = entryPoint.TrimStart('!');
        }

        //archiveName
        archiveName = $"{mod_id}-{version}-fabric";
        Console.Write($"What is the Archive Name {(string.IsNullOrWhiteSpace(archiveName) ? "" : $"(Default is \"{archiveName}\")")}: ");
        string tmpArchiveName = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(tmpArchiveName.TrimStart('!')))
        {
            archiveName = tmpArchiveName;
        }
        //IDE
        Console.Write($"What is the IDE idea/vscode/eclipse {(string.IsNullOrWhiteSpace(ide) ? "" : $"(Default is \"{ide}\")")}: ");
        ide = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(ide.TrimStart('!')))
        {
            ide = manager.GetConfigByKey("ide").Value;
        }
        else if (ide.StartsWith("!"))
        {
            ide = manager.GetConfigByKey("ide").Value = ide.TrimStart('!');
        }


        //Base Directory
        baseDirectory = Directory.CreateDirectory(Path.Combine(baseDirectory, mod_name)).FullName;

        // Extraction
        Thread.Sleep(1000);
        string tmp = Directory.CreateDirectory(Path.Combine(exe, "tmp")).FullName;
        Directory.Delete(tmp, true);
        tmp = Directory.CreateDirectory(Path.Combine(exe, "tmp")).FullName;
        ZipFile.ExtractToDirectory(Path.Combine(templates, version + ".zip"), tmp, true);
        Console.WriteLine("Extracting...");
        Thread.Sleep(1000);

        Parallel.ForEach(Directory.GetFiles(Directory.GetDirectories(tmp)[0], "*", SearchOption.AllDirectories), file =>
        {
            string new_file = Path.GetFullPath(Path.Combine(baseDirectory, Path.GetRelativePath(Directory.GetDirectories(tmp)[0], file)));
            Directory.CreateDirectory(Directory.GetParent(new_file).FullName);
            File.Copy(file, new_file, true);
        });
        Console.WriteLine("Cleaning...");
        Thread.Sleep(500);
        Directory.Delete(tmp, true);

        // Update fabric.mod.json
        Console.WriteLine("Populating fabric.mod.json...");
        string fabric_file = Path.Combine(baseDirectory, "src", "main", "resources", "fabric.mod.json");
        JObject fabric = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(fabric_file));
        fabric["id"] = mod_id;
        fabric["name"] = mod_name;
        fabric["description"] = description;
        fabric["authors"][0] = author;
        fabric["entrypoints"]["main"][0] = $"{package}.{mod_id}.{entryPoint}";
        fabric["icon"] = $"assets/{mod_id}/icon.png";
        fabric["mixins"][0] = $"{mod_id}.mixins.json";
        File.WriteAllText(fabric_file, JsonConvert.SerializeObject(fabric));
        Thread.Sleep(500);

        // Update modid.mixin.json
        Console.WriteLine($"Populating {mod_id}.mixins.json...");
        string mixin_file = Path.Combine(baseDirectory, "src", "main", "resources", "modid.mixins.json");
        JObject mixin = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(mixin_file));
        mixin["package"] = $"{package}.mixin";
        mixin["client"][0].Remove();
        File.WriteAllText(mixin_file, JsonConvert.SerializeObject(mixin));
        Thread.Sleep(500);

        // Update gradlew.properties
        Console.WriteLine("Updating gradle.properties...");
        string gradle = File.ReadAllText(Path.Combine(baseDirectory, "gradle.properties"));
        StringBuilder builder = new StringBuilder();
        foreach (string s in gradle.Split('\n'))
        {
            if (s.Contains("mod_version"))
            {
                builder.Append($"mod_version = 0.0.1\n".Trim());
                builder.Append('\n');
            }
            else if (s.Contains("maven_group"))
            {
                builder.Append($"maven_group = {package}.{mod_id}\n".Trim());
                builder.Append('\n');
            }
            else if (s.Contains("archives_base_name"))
            {
                builder.Append($"archives_base_name = {archiveName}\n".Trim());
                builder.Append('\n');
            }
            else
            {
                builder.AppendLine(s.Trim());
            }
        }

        File.WriteAllText(Path.Combine(baseDirectory, "gradle.properties"), builder.ToString());


        Thread.Sleep(500);

        // Move Files
        Console.WriteLine("Moving Icon...");
        File.Move(Path.Combine(baseDirectory, "src", "main", "resources", "assets", "modid", "icon.png"), Path.Combine(Directory.CreateDirectory(Path.Combine(baseDirectory, "src", "main", "resources", "assets", mod_id)).FullName, "icon.png"));
        Console.WriteLine("Moving Mixin...");
        File.Move(Path.Combine(baseDirectory, "src", "main", "resources", "modid.mixins.json"), Path.Combine(baseDirectory, "src", "main", "resources", $"{mod_id}.mixins.json"));
        string java_file = Path.Combine(Directory.CreateDirectory(Path.Combine(baseDirectory, "src", "main", "java", Path.Combine($"{package}.{mod_id}".Split('.')))).FullName, $"{entryPoint}.java");
        Console.WriteLine("Moving Java Files...");
        File.Move(Path.Combine(baseDirectory, "src", "main", "java", "net", "fabricmc", "example", "ExampleMod.java"), java_file, true);
        Thread.Sleep(500);

        // Writing to Java File
        Console.WriteLine($"Writing to {entryPoint}.java");
        string java = File.ReadAllText(java_file);
        java = java.Replace("ExampleMod", entryPoint);
        java = java.Replace("package net.fabricmc.example;", $"package {package}.{mod_id};");
        java = java.Replace("modid", mod_id);
        File.WriteAllText(java_file, java);
        Thread.Sleep(500);
        Console.WriteLine("Cleaning...");
        //Directory.Delete(Path.Combine(baseDirectory, "src", "main", "resources", "assets", "modid"), true);
        //Directory.Delete(Path.Combine(baseDirectory, "src", "main", "java", "net"), true);



        // Gradlew Commands
        Console.WriteLine($"Building Environment for {ide}");
        string gradlew = Path.GetFullPath(Path.Combine(baseDirectory, "gradlew"));
        Process p = new()
        {
            StartInfo = new()
            {
                FileName = gradlew,
                Arguments = ide,
                UseShellExecute = OperatingSystem.IsWindows(),
                CreateNoWindow = !OperatingSystem.IsWindows(),
                WorkingDirectory = Path.GetFullPath(baseDirectory),
            },

        };
        p.Start();
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            Console.Error.WriteLine($"Unable to setup environment for {ide}");
            return;
        }
        Thread.Sleep(500);


        Console.WriteLine($"Generating Sources...");
        p = new()
        {
            StartInfo = new()
            {
                FileName = gradlew,
                Arguments = "genSources",
                UseShellExecute = OperatingSystem.IsWindows(),
                CreateNoWindow = !OperatingSystem.IsWindows(),
                WorkingDirectory = Path.GetFullPath(baseDirectory),
            },

        };
        p.Start();
        p.WaitForExit();
        Thread.Sleep(500);


        Console.WriteLine($"Building...");
        p = new()
        {
            StartInfo = new()
            {
                FileName = gradlew,
                Arguments = "build",
                UseShellExecute = OperatingSystem.IsWindows(),
                CreateNoWindow = !OperatingSystem.IsWindows(),
                WorkingDirectory = Path.GetFullPath(baseDirectory),
            },

        };
        p.Start();
        p.WaitForExit();
        Thread.Sleep(500);

        string launch_file = Path.Combine(baseDirectory, ".vscode", "launch.json");
        if (File.Exists(launch_file))
        {
            Console.WriteLine("Writing Launch Settings...");
            JObject launch = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(launch_file));
            launch["configurations"][0]["cwd"] = $"{launch["configurations"][0]["cwd"]}/client";
            launch["configurations"][0]["args"] = $"--username={mcUsername}";
            launch["configurations"][1]["cwd"] = $"{launch["configurations"][1]["cwd"]}/server";
            Directory.CreateDirectory(Path.Combine(baseDirectory, "run", "client"));
            Directory.CreateDirectory(Path.Combine(baseDirectory, "run", "server"));
            File.WriteAllText(launch_file, JsonConvert.SerializeObject(launch));
            Thread.Sleep(500);
        }
        if (ide.Equals("vscode"))
        {
            Console.WriteLine($"Launching VSCode...");
            p = new()
            {
                StartInfo = new()
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "Local", "Programs", "Microsoft VS Code", "Code.exe"),
                    Arguments = Path.GetFullPath(baseDirectory),
                },

            };
            //p.Start();
        }

        Console.WriteLine("Done...");
    }
}