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
    using XnaXImporter = xna::Microsoft.Xna.Framework.Content.Pipeline.XImporter;
    using XnaNodeContent = xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;
    using XnaBoneContent = xna::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneContent;

    using MonoGameFbxImporter = monogame::Microsoft.Xna.Framework.Content.Pipeline.FbxImporter;
    using MonoGameXImporter = monogame::Microsoft.Xna.Framework.Content.Pipeline.XImporter;
    using MonoGameNodeContent = monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.NodeContent;
    using MonoGameBoneContent = monogame::Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneContent;

    using System.Runtime.Serialization;
    using System.Xml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Runtime.Serialization.Formatters;

    class Program
    {
        static readonly List<JsonConverter> _converters = new List<JsonConverter>()
            {
                new MatrixConverter(),
                new Vector2Converter(),
                new NullableVector2Converter(),
                new Vector3Converter(),
                new NullableVector3Converter(),
            };

        static void Main(string[] args)
        {
            const string asset = @"Dude\dude.fbx";
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
                    Converters = _converters,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                });

            var xnaSkel = xnaOutput.Children[1] as XnaBoneContent;
            var mgSkel = mgOutput.Children[1] as MonoGameBoneContent;

            //const string channel = "L_Ankle1";//"Head";

            Console.WriteLine("Serializing XNA version...");
            using (var xnaOut = File.CreateText(Path.Combine(outDir, "xna.json")))
            {
                //var obj = xnaSkel.Animations["Take 001"].Channels[channel].ToList();
                //foreach (var item in obj)
                //    xnaOut.WriteLine(string.Format("{0,-17}: {1}", item.Time, item.Transform.ToString()));

                serializer.Serialize(xnaOut, xnaOutput);
            }

            Console.WriteLine("Serializing MonoGame version...");
            using (var mgOut = File.CreateText(Path.Combine(outDir, "mg.json")))
            {
                //var obj = mgSkel.Animations["Take 001"].Channels[channel].ToList();
                //foreach (var item in obj)
                //    mgOut.WriteLine(string.Format("{0,-17}: {1}", item.Time, item.Transform.ToString()));


                serializer.Serialize(mgOut, mgOutput);
            }

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
            var result = base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();

            result.RemoveAll(p => p.PropertyName == "Animations");
            result.RemoveAll(p => p.PropertyName == "Indices");
            result.RemoveAll(p => p.PropertyName == "Positions");
            result.RemoveAll(p => p.PropertyName == "Vertices");

            return result;
        }
    }

    // Prints XNA matrices like the MonoGame ones. I have no idea why they print differently.
    internal class MatrixConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(xna::Microsoft.Xna.Framework.Matrix);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var matrix = (xna::Microsoft.Xna.Framework.Matrix)value;
            writer.WriteStartObject();

            writer.WritePropertyName("M11");
            writer.WriteValue(matrix.M11);
            writer.WritePropertyName("M12");
            writer.WriteValue(matrix.M12);
            writer.WritePropertyName("M13");
            writer.WriteValue(matrix.M13);
            writer.WritePropertyName("M14");
            writer.WriteValue(matrix.M14);

            writer.WritePropertyName("M21");
            writer.WriteValue(matrix.M21);
            writer.WritePropertyName("M22");
            writer.WriteValue(matrix.M22);
            writer.WritePropertyName("M23");
            writer.WriteValue(matrix.M23);
            writer.WritePropertyName("M24");
            writer.WriteValue(matrix.M24);

            writer.WritePropertyName("M31");
            writer.WriteValue(matrix.M31);
            writer.WritePropertyName("M32");
            writer.WriteValue(matrix.M32);
            writer.WritePropertyName("M33");
            writer.WriteValue(matrix.M33);
            writer.WritePropertyName("M34");
            writer.WriteValue(matrix.M34);

            writer.WritePropertyName("M41");
            writer.WriteValue(matrix.M41);
            writer.WritePropertyName("M42");
            writer.WriteValue(matrix.M42);
            writer.WritePropertyName("M43");
            writer.WriteValue(matrix.M43);
            writer.WritePropertyName("M44");
            writer.WriteValue(matrix.M44);

            writer.WriteEndObject();
        }
    }

    internal class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(xna::Microsoft.Xna.Framework.Vector3);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (xna::Microsoft.Xna.Framework.Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(vec.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(vec.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(vec.Z);
            writer.WriteEndObject();
        }
    }

    internal class NullableVector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Nullable<xna::Microsoft.Xna.Framework.Vector3>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (Nullable<xna::Microsoft.Xna.Framework.Vector3>)value;
            if (vec.HasValue)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(vec.Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(vec.Value.Y);
                writer.WritePropertyName("Z");
                writer.WriteValue(vec.Value.Z);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }
    }

    internal class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(xna::Microsoft.Xna.Framework.Vector2);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (xna::Microsoft.Xna.Framework.Vector2)value;
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(vec.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(vec.Y);
            writer.WriteEndObject();
        }
    }

    internal class NullableVector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Nullable<xna::Microsoft.Xna.Framework.Vector2>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (Nullable<xna::Microsoft.Xna.Framework.Vector2>)value;
            if (vec.HasValue)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("X");
                writer.WriteValue(vec.Value.X);
                writer.WritePropertyName("Y");
                writer.WriteValue(vec.Value.Y);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
