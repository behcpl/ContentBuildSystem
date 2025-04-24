using System;
using System.IO;
using ContentBuildSystem;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Json;
using ContentBuildSystem.Project;
using ContentBuildSystem.Rules;
using DefaultProcessors.CopyFile;
using GenericAssets.Legacy.Atlases;
using GenericAssets.Legacy.Fonts;
using GenericAssets.Legacy.Textures;
using GenericAssets.Localization;
using GenericAssets.Shader;

namespace Sandbox;

class Program
{
    static void Main(string[] args)
    {
        string projDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Example"));
        string projPath = Path.Combine(projDir, "example.asproj");
        Console.WriteLine($"FOUND: {File.Exists(projPath)}  {Path.GetFullPath(projPath)}");
        ProjectSerializer serializer = new ProjectSerializer();
        ProjectDescription? project = serializer.Deserialize(projPath, null);

        RuleSerializer ruleSerializer = new RuleSerializer();

        ContentBuilderOptions options = ContentBuilderOptions.Build(projPath, null, null);

        string configuration = "windows-legacy";//"windows-release";
        RuleProvider ruleProvider = new RuleProvider(ruleSerializer);

        ruleProvider.AddProcessor("copy", new CopyFileProcessorFactory(), typeof(CopyFileProcessorSettings));

        ruleProvider.AddProcessor("csv2json", new LangStringsProcessorFactory(), typeof(object));
        ruleProvider.AddProcessor("shaders", new ShaderGroupProcessorFactory(), typeof(ShaderGroupProcessorSettings));

        ruleProvider.AddProcessor("bmfont", new BitmapFontProcessorFactory(), typeof(BitmapFontProcessorSettings));
        ruleProvider.AddProcessor("fontgen", new SdfFontProcessorFactory(true), typeof(SdfFontProcessorSettings));
        ruleProvider.AddProcessor("texture", new TextureProcessorFactory(), typeof(TextureProcessorSettings));
        ruleProvider.AddProcessor("atlas", new SpriteAtlasProcessorFactory(true), typeof(SpriteAtlasProcessorSettings));

        IReport report = new VerboseConsoleReport();
        // IReport report = new ConsoleReport();
        ContentBuilder builder = new ContentBuilder(options, project!, ruleProvider, new BuildItemManifestSerializer());
        
        bool success = builder.PrepareConfiguration(configuration, report) && builder.BuildGroups(report);
        report.Info($"DONE {success}");

        // Console.ReadKey();
        // Stream fs = File.OpenRead(projPath);
        // JsonSerializerOptions options = new JsonSerializerOptions();
        // options.AllowTrailingCommas = true;
        // options.IncludeFields = true; 
        //
        // ProjectJson? project = JsonSerializer.Deserialize<ProjectJson>(fs, options);
        //
        // IPropertyBag propertyBag = SimplePropertyBagBuilder.FromJson(project!.Plugins![0].Options!);

        // using var watcher = new FileSystemWatcher(@"D:\TestFolder");
        //
        // watcher.NotifyFilter = NotifyFilters.Attributes
        //                        | NotifyFilters.CreationTime
        //                        | NotifyFilters.DirectoryName
        //                        | NotifyFilters.FileName
        //                        | NotifyFilters.LastAccess
        //                        | NotifyFilters.LastWrite
        //                        | NotifyFilters.Security
        //                        | NotifyFilters.Size;
        //
        // watcher.Changed += OnChanged;
        // watcher.Created += OnCreated;
        // watcher.Deleted += OnDeleted;
        // watcher.Renamed += OnRenamed;
        // watcher.Error += OnError;
        //
        // // watcher.Filter = "*.txt";
        // watcher.IncludeSubdirectories = true;
        // watcher.EnableRaisingEvents = true;
        //
        // // Console.WriteLine("Press enter to delete.");
        // // Console.ReadLine();
        // //
        // // Directory.Delete(@"D:\TestFolder\Dupu", true);
        //
        // Console.WriteLine("Press enter to exit.");
        // Console.ReadLine();
    }

    // private static unsafe void Font()
    // {
    //     FT_LibraryRec_* lib;
    //     FT_FaceRec_* face;
    //     FT_Error error = FT_Init_FreeType(&lib);
    //
    //     int spread = 6;
    //     IntPtr moduleName = Marshal.StringToHGlobalAnsi("sdf");
    //     IntPtr propertyName = Marshal.StringToHGlobalAnsi("spread");
    //
    //     FT_Property_Set(lib, (byte*)moduleName, (byte*)propertyName, &spread);
    //     Marshal.FreeHGlobal(moduleName);
    //     Marshal.FreeHGlobal(propertyName);
    //
    //     Console.WriteLine($"1 {error}");
    //
    //     error = FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi("OpenSans-SemiBold.ttf"), 0, &face);
    //     Console.WriteLine($"2 {error}");
    //     error = FT_Set_Char_Size(face, 0, 16 * 64, 300, 300);
    //     Console.WriteLine($"3 {error}");
    //     uint glyphIndex = FT_Get_Char_Index(face, 'F');
    //     Console.WriteLine($"4 go:{glyphIndex}");
    //     error = FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
    //     Console.WriteLine($"5 {error}");
    //     error = FT_Render_Glyph(face->glyph, FT_RENDER_MODE_SDF); //FT_RENDER_MODE_NORMAL
    //     Console.WriteLine($"6 {error}");
    //
    //     FT_Done_Face(face);
    //
    //     FT_Done_FreeType(lib);
    //     Console.WriteLine($"FT DONE");
    // }
}