using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using RfgTools.Formats.Zones;

namespace ZoneConverter
{
    //Tool for the rfgzone_pc and layer_pc file formats
    class Program
    {
        public class CommandLineOptions
        {
            [Value(0, MetaName = "InputPath", Required = true, HelpText = "Input file path.")]
            public IEnumerable<string> InputPath { get; set; }

            [Option('o', "OutputPath", Required = false, HelpText = "Output file path. Will attempt to create file if it doesn't exist.")]
            public string? OutputPath { get; set; } = null;

            [Option('x', "xml", Required = false, HelpText = "Pass this argument to convert rfgzone_pc or layer_pc files to xml.")]
            public bool ToXml { get; set; }

            [Option('b', "binary", Required = false, HelpText = "Pass this argument to convert xml files to rfgzone_pc for layer_pc files.")]
            public bool ToBinary { get; set; }

            [Option('i', "info", Required = false, HelpText = "Print info about the contents of the zone file.")]
            public bool PrintInfo { get; set; }
        }

        static void Main(string[] args)
        {
#if DEBUG
            //Change this to the test file path you want to use. Useful for quick debugging
            string inPath = @"C:\Users\moneyl\source\repos\RfgZoneTools\Possible badlands barracks files\edits\terr01_10_08_duped2towers_2.rfgzone_pc";
            var zoneFile = new ZonePc();
            zoneFile.ReadFromBinary(inPath);

            string xmlPath = Path.GetDirectoryName(inPath) + "\\" + Path.GetFileNameWithoutExtension(inPath) + "_zone_objects.xml";
            zoneFile.WriteToXmlAndSave(xmlPath);
#endif

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
            {
                string inputPath = options.InputPath.First();

                if (options.ToXml) //Attempt to convert a binary file to xml
                {
                    string absolutePath = Path.GetFullPath(inputPath);
                    if (options.OutputPath != null)
                    {
                        ConvertFileToXml(absolutePath, options.OutputPath);
                    }
                    else
                    {
                        ConvertFileToXml(absolutePath);
                    }
                }
                else if (options.ToBinary) //Attempt to convert an xml file back to binary
                {
                    string absolutePath = Path.GetFullPath(inputPath);
                    if (options.OutputPath != null)
                    {
                        ConvertFileToBinary(absolutePath, options.OutputPath);
                    }
                    else
                    {
                        ConvertFileToBinary(absolutePath);
                    }
                }
                else if(!options.PrintInfo) //For drag and drop behavior, guess what the user wants based on file extension
                {
                    foreach (var path in options.InputPath)
                    {
                        string absolutePath = Path.GetFullPath(path);
                        string extension = Path.GetExtension(absolutePath);
                        if (extension == ".rfgzone_pc" || extension == ".layer_pc")
                        {
                            ConvertFileToXml(absolutePath);
                        }
                        else if (extension == ".xml")
                        {
                            ConvertFileToBinary(absolutePath);
                        }
                    }
                }

                //Print info about the zone files contents
                if (options.PrintInfo)
                {
                    PrintZoneInfo(inputPath);
                }
            });
        }

        private static void ConvertFileToXml(string inputPath, string outPath = null)
        {
            string extension = Path.GetExtension(inputPath);
            if (extension != ".rfgzone_pc" && extension != ".layer_pc")
            {
                Console.WriteLine($"Skipping {Path.GetFileName(inputPath)}. Invalid file extension. Expects extension to be \"layer_pc\" or \"rfgzone_pc\" for binary->xml conversion.");
                return;
            }

            Console.WriteLine($"\nConverting {Path.GetFileName(inputPath)} to xml...");
            var zoneFile = new ZonePc();
            zoneFile.ReadFromBinary(inputPath);

            var xml = zoneFile.WriteToXml();
            if (outPath == null)
            {
                string xmlPath = Path.GetDirectoryName(inputPath) + "\\" + Path.GetFileNameWithoutExtension(inputPath) + ".xml";
                xml.Save(new FileStream(xmlPath, FileMode.Create));
            }
            else
            {
                xml.Save(new FileStream(outPath, FileMode.Create));
            }
            Console.WriteLine("Done!");
        }

        private static void ConvertFileToBinary(string inputPath, string outPath = null)
        {
            Console.WriteLine($"\nConverting {Path.GetFileName(inputPath)} to xml...");
            var zoneFile = new ZonePc();
            zoneFile.ReadFromXml(inputPath);

            if (outPath == null)
            {
                string binPath = Path.GetDirectoryName(inputPath) + "\\" + Path.GetFileNameWithoutExtension(inputPath) + "_generated.rfgzone_pc";
                zoneFile.WriteToBinary(binPath);
            }
            else
            {
                zoneFile.WriteToBinary(outPath);
            }
            Console.WriteLine("Done!");
        }

        private static void PrintZoneInfo(string inputPath)
        {
            var zoneFile = new ZonePc();
            zoneFile.ReadFromBinary(inputPath);

            Console.WriteLine($"{Path.GetFileName(inputPath)}:");
            Console.WriteLine($"    Version: {zoneFile.Version}");
            Console.WriteLine($"    District flags: {zoneFile.DistrictFlags}");
            Console.WriteLine($"    Num objects: {zoneFile.NumObjects}");
            Console.WriteLine($"    Num handles: {zoneFile.NumHandles}");
            Console.WriteLine($"    District name: {zoneFile.DistrictName}");
        }
    }
}
