using System;
using System.IO;
using RfgTools.Formats.Meshes;

namespace SmeshExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && args[0] == "--help"))
            {
                PrintHelpMessage();
            }
            else if (args.Contains("--mesh") && args.IndexOf("--mesh") < args.Length - 1)
            {

                string headerPath = args[args.IndexOf("--mesh") + 1];
                string dataPath = $"{Path.GetDirectoryName(headerPath)}//{Path.GetFileNameWithoutExtension(headerPath)}.gsmesh_pc";
                if (!File.Exists(headerPath))
                    throw new FileNotFoundException($"Error! Could not find file at path \"{headerPath}\". Check your path.");
                if (!File.Exists(dataPath))
                    throw new FileNotFoundException($"Error! Could not find file at path \"{dataPath}\". Check your path.");

                //Console.WriteLine(headerPath);
                //Console.WriteLine(dataPath);
                var staticMesh = new StaticMesh();
                staticMesh.Read(headerPath, dataPath);
                if (args.Contains("--info+"))
                {
                    Console.WriteLine($"\n{Path.GetFileName(headerPath)} info:");
                    Console.WriteLine($"    Version: {staticMesh.SharedHeader.Version}");
                    Console.WriteLine($"    Num submeshes: {staticMesh.NumSubmeshes}");
                    Console.WriteLine($"    Num materials: {staticMesh.SharedHeader.NumMaterials}");
                    Console.WriteLine($"    Num indices: {staticMesh.Indices.Count}");
                    Console.WriteLine($"    Num vertices: {staticMesh.Vertices.Count}");
                    Console.WriteLine($"    Index size: {staticMesh.IndexBufferConfig.IndexSize}");
                    Console.WriteLine($"    Vertex format: {staticMesh.VertexBufferConfig.VertexFormat.ToString()}, Stride: {staticMesh.VertexBufferConfig.VertexStride0}, {staticMesh.VertexBufferConfig.VertexStride1}");
                    Console.WriteLine($"    Primitive type: {staticMesh.IndexBufferConfig.PrimitiveType}");
                    Console.WriteLine($"    Num UV channels: {staticMesh.VertexBufferConfig.NumUvChannels}");
                    Console.WriteLine($"    Required textures:");
                    foreach (var textureName in staticMesh.TextureNames)
                        Console.WriteLine($"        {textureName}");
                }
                else if (args.Contains("--info"))
                {
                    Console.WriteLine($"\n{Path.GetFileName(headerPath)} info:");
                    Console.WriteLine($"    Version: {staticMesh.SharedHeader.Version}");
                    Console.WriteLine($"    Num submeshes: {staticMesh.NumSubmeshes}");
                    Console.WriteLine($"    Required textures:");
                    foreach (var textureName in staticMesh.TextureNames)
                        Console.WriteLine($"        {textureName}");
                }

                int expectedTextureCount = staticMesh.TextureNames.Count;
                if (args.Contains("--extract"))
                {
                    string outPath = $"{Path.GetDirectoryName(headerPath)}//{Path.GetFileNameWithoutExtension(headerPath)}_extract.obj";
                    string diffusePath = null;
                    string normalPath = null;
                    string specularPath = null;

                    if (args.Contains("--diffuse"))
                    {
                        int diffuseIndex = args.IndexOf("--diffuse");
                        if (diffuseIndex < args.Length - 1)
                        {
                            diffusePath = args[diffuseIndex + 1];
                        }
                        else
                        {
                            Console.WriteLine("Error, failed to parse diffuse texture path. Please type the command \"--diffuse\", followed by the diffuse texture path to provide it.");
                            Console.WriteLine("Example: SmeshExtractor.exe --mesh ./wrench.csmesh_pc --diffuse ./ambient_props_d.png");
                        }
                    }
                    if (args.Contains("--normal"))
                    {
                        int normalIndex = args.IndexOf("--normal");
                        if (normalIndex < args.Length - 1)
                        {
                            normalPath = args[normalIndex + 1];
                        }
                        else
                        {
                            Console.WriteLine("Error, failed to parse normal texture path. Please type the command \"--normal\", followed by the normal texture path to provide it.");
                            Console.WriteLine("Example: SmeshExtractor.exe --mesh ./wrench.csmesh_pc --normal ./ambient_props_n.png");
                        }
                    }
                    if (args.Contains("--specular"))
                    {
                        int specularIndex = args.IndexOf("--specular");
                        if (specularIndex < args.Length - 1)
                        {
                            specularPath = args[specularIndex + 1];
                        }
                        else
                        {
                            Console.WriteLine("Error, failed to parse specular texture path. Please type the command \"--specular\", followed by the specular texture path to provide it.");
                            Console.WriteLine("Example: SmeshExtractor.exe --mesh ./wrench.csmesh_pc --specular ./ambient_props_s.png");
                        }
                    }
                    Console.WriteLine($"Extracting {Path.GetFileName(headerPath)} to {Path.GetFileName(outPath)}...");
                    staticMesh.WriteToObjFile(outPath, diffusePath, normalPath, specularPath);
                    Console.WriteLine("Done!");
                }
            }
            else if (args.Contains("--mesh"))
            {
                Console.WriteLine("Error, failed to parse mesh path. Please type the command \"--mesh\", followed by the mesh path to provide it.");
                Console.WriteLine("Example: SmeshExtractor.exe --mesh ./wrench.csmesh_pc");
            }
            else
            {
                PrintHelpMessage();
            }
        }

        private static void PrintHelpMessage()
        {
            Console.WriteLine("To use, run the exe followed by any number of options. Most options have extra inputs they expect to immediately follow them."
                  + " The order of options doesn't matter, but their inputs must immediately follow them. Options:\n");

            Console.WriteLine("    --help      Shows this help message.\n");
            Console.WriteLine("    --mesh      This is a required command. The path of the mesh should follow this command after a space. If the "
                              + "mesh file is in the active directory of the command line, you can prefix it with ./ instead of typing the whole path.\n");
            Console.WriteLine("    --info      Prints info about the mesh such as the textures it uses and the number of submeshes.\n");
            Console.WriteLine("    --info+     Prints the same values a \"--info\" plus several more.\n");
            Console.WriteLine("    --extract   Triggers extraction of the mesh. If only this is passed and no texture arguments are used, you'll get an untextured mesh.\n");

            Console.WriteLine("The following options are only used if you passed the --extract option:\n");
            Console.WriteLine("    --diffuse   Pass if you want the model to have a diffuse (color) texture. The path of the diffuse texture should immediately follow this option."
                              + " These textures often end with _d in RFG.\n");
            Console.WriteLine("    --normal    Pass if you want the model to have a normal map/texture. The path of the normal map should immediately follow this option."
                              + " These textures often end with _n in RFG.\n");
            Console.WriteLine("    --specular  Pass if you want the model to have a specular map/texture. The path of the specular map should immediately follow this option."
                              + " These textures often end with _s in RFG.\n");
        }
    }
}
