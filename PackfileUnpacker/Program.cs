using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;
using RfgTools.Formats.Packfiles;

namespace PackfileUnpacker
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "Input", Required = true, HelpText = "Input file or folder.")]
            public IEnumerable<string> Input { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Output verbose log messages.")]
            public bool Verbose { get; set; }

            [Option('r', "recursive", Required = false, HelpText = "After unpacking a packfile, unpack any valid packfiles it contained.")]
            public bool Recursive { get; set; }

            [Option('s', "search", Required = false, HelpText = "Unpack packfiles in the provided folder and it's subfolders.")]
            public bool Search { get; set; }
        }

        static void Main(string[] args)
        {
			//Todo: Add warning when unpacking large amount of files or folders
			//Todo: Add timer
			//Todo: Add support for custom unpack folder/location

			Parser.Default.ParseArguments<Program.Options>(args).WithParsed(delegate (Program.Options options)
			{
				Console.WriteLine("Debug options output:");
				Console.WriteLine("Input: {0}", options.Input);
				Console.WriteLine("Verbose: {0}", options.Verbose);
				Console.WriteLine("Recursive: {0}", options.Recursive);
				Console.WriteLine("Search: {0}", options.Search);

				using var errorStream = new FileStream("PackfileUnpackerErrors.txt", FileMode.OpenOrCreate);
				using var error = new StreamWriter(errorStream);

				//Loop through all input paths
				foreach (string input in options.Input)
				{
					//Handle path being a directory
					if (Directory.Exists(input))
					{
						Console.WriteLine("Input exists and is a directory");
						FileInfo[] inputFolder = new FileInfo[0];
						if (options.Search)
						{
							inputFolder = new DirectoryInfo(input).GetFiles("*", SearchOption.AllDirectories);
						}
						else
						{
							inputFolder = new DirectoryInfo(input).GetFiles();
						}

						//Extract all packfiles in folder
						foreach (FileInfo file in inputFolder)
						{
							if (file.Extension == ".vpp_pc" || file.Extension == ".str2_pc")
							{
								string inputPath = file.FullName;
								string outputPath = file.DirectoryName + "\\Unpack\\" + file.Name + "\\";

								//Extract packfile
								try
								{
									Packfile packfile = new Packfile(options.Verbose);
									packfile.ReadMetadata(inputPath);
									packfile.ExtractFileData(outputPath);
								}
								catch (Exception ex)
								{
									//If encounter error, move to next file
                                    error.WriteLine($"Exception caught while unpacking {file.Name}: {ex.Message} ... Skipping to next file.");
									continue;
								}

                                //Extract contents of file if recursive extracting
                                if (options.Recursive)
								{
									foreach (FileInfo subfile in new DirectoryInfo(outputPath).GetFiles())
									{
										if (subfile.Extension == ".vpp_pc" || subfile.Extension == ".str2_pc")
										{
											try
											{
												string subInputPath = subfile.FullName;
												string subOutputPath = subfile.DirectoryName + "\\Subfiles\\" + subfile.Name + "\\";
												Packfile packfile2 = new Packfile(options.Verbose);
												packfile2.ReadMetadata(subInputPath);
												packfile2.ExtractFileData(subOutputPath);
											}
											catch (Exception ex2)
											{
                                                //If encounter error, move to next file
                                                error.WriteLine($"Exception caught while unpacking {subfile.Name}: {ex2.Message} ... Skipping to next file.");
                                                continue;
                                            }
										}
									}
								}
							}
						}
					}
					else if (File.Exists(input)) //Handle path being a file path
					{
						Console.WriteLine("Input exists and is a file!");
						FileInfo PackfileInfo = new FileInfo(input);
						string InputPath = input;
						string OutputPath = PackfileInfo.DirectoryName + "\\Unpack\\" + PackfileInfo.Name + "\\";

						//Extract file
						try
						{
							Packfile packfile3 = new Packfile(options.Verbose);
							packfile3.ReadMetadata(InputPath);
							packfile3.ExtractFileData(OutputPath);
						}
						catch (Exception ex3)
						{
                            //If encounter error, move to next file
                            error.WriteLine($"Exception caught while unpacking {PackfileInfo.Name}: {ex3.Message} ... Skipping to next file.");
                            continue;
						}
						//Extract contents of file if recursive extracting
						if (options.Recursive)
						{
							foreach (FileInfo file2 in new DirectoryInfo(OutputPath).GetFiles())
							{
								if (file2.Extension == ".vpp_pc" || file2.Extension == ".str2_pc")
								{
									try
									{
										string subInputPath2 = file2.FullName;
										string subOutputPath2 = file2.DirectoryName + "\\Subfiles\\" + file2.Name + "\\";
										Packfile packfile4 = new Packfile(options.Verbose);
										packfile4.ReadMetadata(subInputPath2);
										packfile4.ExtractFileData(subOutputPath2);
									}
									catch (Exception ex4)
									{
                                        //If encounter error, move to next file
                                        error.WriteLine($"Exception caught while unpacking {file2.Name}: {ex4.Message} ... Skipping to next file.");
                                        continue;
									}
								}
							}
						}
					}
				}
			});
            Console.WriteLine("Done!");
		}
    }
}
