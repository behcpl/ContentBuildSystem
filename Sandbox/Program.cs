using System;
using System.Threading;
using ContentBuildSystem;
using ContentBuildSystem.Interfaces;

namespace Sandbox;

internal class Program
{
    private static void Main(string[] args)
    {
        int top = Console.CursorTop;
        int left = Console.CursorLeft;
        Console.WriteLine($"START: ({left},{top})");
        Console.WriteLine("NETX");
        Console.WriteLine("NETX");
        Console.WriteLine("NETX");
        
        ConsoleReport rep2 = new ConsoleReport();

        rep2.Info("test info");
        Thread.Sleep(200);
        rep2.Warning("test warn");
        Thread.Sleep(200);
        rep2.Error("test error");
        Thread.Sleep(200);
        rep2.Info("test info 2");
        Thread.Sleep(200);

        IReport subReport1 = rep2.CreateGroup("First group", 30);
        Thread.Sleep(200);

        for (int i = 0; i < 30; i++)
        {
            subReport1.Advance();
            Thread.Sleep(100);

            if (i == 4 || i == 12)
                subReport1.Error("Extra error");
            if (i == 7)
                subReport1.Warning("Extra warning");
        }

        Thread.Sleep(200);
        subReport1.Finish();
        Thread.Sleep(200);

        rep2.Info("test info 3 ");
        Thread.Sleep(200);

        IReport sub2 = rep2.CreateGroup("Second group", 30);
        Thread th2 = new Thread(() =>
        {
            for (int i = 0; i < 30; i++)
            {
                sub2.Advance();
                Thread.Sleep(100);

                if (i == 4 || i == 12)
                    sub2.Warning("Extra warning from th2");
            }

            sub2.Finish();
        });
        th2.Start();

        IReport sub3 = rep2.CreateGroup("Third group", 30);
        Thread th3 = new Thread(() =>
        {
            for (int i = 0; i < 30; i++)
            {
                sub3.Advance();
                Thread.Sleep(50);

                if (i == 4 || i == 12)
                    sub3.Info("Extra info from th3");
            }

            sub3.Finish();
        });
        th3.Start();

        th2.Join();
        th3.Join();

        Console.ReadKey();

        // string projDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Example"));
        // string projPath = Path.Combine(projDir, "example.asproj");
        // Console.WriteLine($"FOUND: {File.Exists(projPath)}  {Path.GetFullPath(projPath)}");
        // ProjectSerializer serializer = new();
        // ProjectDescription? project = serializer.Deserialize(projPath, null);
        //
        // RuleSerializer ruleSerializer = new();
        //
        // ContentBuilderOptions options = ContentBuilderOptions.Build(projPath, null, null);
        //
        // string configuration = "windows-legacy"; //"windows-release";
        // RuleProvider ruleProvider = new(ruleSerializer);
        //
        // ruleProvider.AddProcessor("copy", new CopyFileProcessorFactory(), typeof(CopyFileProcessorSettings));
        //
        // ruleProvider.AddProcessor("csv2json", new LangStringsProcessorFactory(), typeof(object));
        // ruleProvider.AddProcessor("shaders", new ShaderGroupProcessorFactory(), typeof(ShaderGroupProcessorSettings));
        //
        // ruleProvider.AddProcessor("bmfont", new BitmapFontProcessorFactory(), typeof(BitmapFontProcessorSettings));
        // ruleProvider.AddProcessor("fontgen", new SdfFontProcessorFactory(true), typeof(SdfFontProcessorSettings));
        // ruleProvider.AddProcessor("texture", new TextureProcessorFactory(), typeof(TextureProcessorSettings));
        // ruleProvider.AddProcessor("atlas", new SpriteAtlasProcessorFactory(true), typeof(SpriteAtlasProcessorSettings));
        //
        // IReport report = new VerboseConsoleReport();
        // // IReport report = new ConsoleReport();
        // ContentBuilder builder = new(options, project!, ruleProvider, new BuildItemManifestSerializer());
        //
        // bool success = builder.PrepareConfiguration(configuration, report) && builder.BuildGroups(report);
        // report.Info($"DONE {success}");

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
