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
    using MonoGameFbxImporter = monogame::Microsoft.Xna.Framework.Content.Pipeline.FbxImporter;

    class Program
    {
        static void Main(string[] args)
        {
            const string asset = "dude.fbx";
            var contentDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Content"));
            var intDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\obj"));
            var outDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\bin"));
            var absAssetPath = Path.Combine(contentDir, asset);

            var xnaContext = new XnaImporterContext(Path.Combine(intDir, "xna"), Path.Combine(outDir, "xna"));
            var xnaImporter = new XnaFbxImporter();
            var xnaOutput = xnaImporter.Import(absAssetPath, xnaContext);

            var mgContext = new MonoGameImporterContext(Path.Combine(intDir, "mg"), Path.Combine(outDir, "mg"));
            var mgImporter = new MonoGameFbxImporter();
            var mgOutput = mgImporter.Import(absAssetPath, mgContext);

            // Compare objects here

            Console.Write("Press any key to continue . . .");
            Console.ReadKey();
        }
    }
}
