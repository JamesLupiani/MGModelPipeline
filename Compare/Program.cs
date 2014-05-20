using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Compare
{
    extern alias xna;
    extern alias monogame;

    using XnaFbxImporter = xna::Microsoft.Xna.Framework.Content.Pipeline.FbxImporter;
    using XnaNodeContent = xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

    using MonoGameFbxImporter = monogame::Microsoft.Xna.Framework.Content.Pipeline.FbxImporter;
    using MonoGameNodeContent = xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;

    using System.Runtime.Serialization;
    using System.Xml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    class Program
    {
        static void Main(string[] args)
        {
            const string asset = @"Dude\dude.fbx";//Ship\ship.fbx";
            var rootDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\"));
            var contentDir = Path.GetFullPath(Path.Combine(rootDir, "Content"));
            var intDir = Path.GetFullPath(Path.Combine(rootDir, "obj"));
            var outDir = Path.GetFullPath(Path.Combine(rootDir, "bin"));
            var absAssetPath = Path.Combine(contentDir, asset);
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
            };

            if (!Directory.Exists(intDir))
                Directory.CreateDirectory(intDir);
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            Console.WriteLine("Importing with XNA...");
            var xnaContext = new XnaImporterContext(Path.Combine(intDir, "xna"), Path.Combine(outDir, "xna"));
            var xnaImporter = new XnaFbxImporter();
            var xnaOutput = xnaImporter.Import(absAssetPath, xnaContext);

            Console.WriteLine("Importing with MonoGame...");
            var mgContext = new MonoGameImporterContext(Path.Combine(intDir, "mg"), Path.Combine(outDir, "mg"));
            var mgImporter = new MonoGameFbxImporter();
            var mgOutput = mgImporter.Import(absAssetPath, mgContext);

            // Serialize for comparison
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    ContractResolver = new OrderedContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                });

            Console.WriteLine("Serializing XNA version...");
            using (var xnaOut = File.CreateText(Path.Combine(outDir, "xna.json")))
                serializer.Serialize(xnaOut, xnaOutput);

            Console.WriteLine("Serializing MonoGame version...");
            using (var mgOut = File.CreateText(Path.Combine(outDir, "mg.json")))
                serializer.Serialize(mgOut, mgOutput);

            // Done
            Console.WriteLine("Serialization complete.");
            Console.Write("Press any key to continue . . .");
            Console.ReadKey();
        }
    }

    internal class OrderedContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
        }
    }
}
