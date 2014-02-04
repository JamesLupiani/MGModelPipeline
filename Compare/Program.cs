using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
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

    class Program
    {
        static void Main(string[] args)
        {
            const string asset = "dude.fbx";
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

            // XNA Serialize
            var xnaSerializer = new DataContractSerializer(typeof(XnaNodeContent), new[]
            {
                typeof(TimeSpan),
                typeof(xna::Microsoft.Xna.Framework.Vector3),
                typeof(xna::Microsoft.Xna.Framework.Matrix),
                typeof(xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.MeshContent),
                typeof(xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BasicMaterialContent),
                typeof(xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneContent),
                typeof(xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationKeyframe),
            }, int.MaxValue, false, true, null);
            using (var xnaOut = XmlWriter.Create(Path.Combine(outDir, "xna.xml"), xmlSettings))
                xnaSerializer.WriteObject(xnaOut, xnaOutput);

            // MonoGame Serialize
            var mgSerializer = new DataContractSerializer(typeof(MonoGameNodeContent), new[]
            {
                typeof(TimeSpan),
                typeof(monogame::Microsoft.Xna.Framework.Vector3),
                typeof(monogame::Microsoft.Xna.Framework.Matrix),
                typeof(monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.MeshContent),
                typeof(monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BasicMaterialContent),
                typeof(monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneContent),
                typeof(monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationKeyframe),
            }, int.MaxValue, false, true, null);
            using (var mgOut = XmlWriter.Create(Path.Combine(outDir, "mg.xml"), xmlSettings))
                mgSerializer.WriteObject(mgOut, mgOutput);

            Console.Write("Press any key to continue . . .");
            Console.ReadKey();
        }
    }
}
