using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compare
{
    extern alias monogame;
    using monogame::Microsoft.Xna.Framework.Content.Pipeline;

    class MonoGameImporterContext : ContentImporterContext
    {
        private readonly string _intermediateDir;
        private readonly string _outputDir;
        private readonly ContentBuildLogger _logger;
        private readonly List<string> _dependencies = new List<string>();

        public MonoGameImporterContext(string intermediateDir, string outputDir)
        {
            _intermediateDir = intermediateDir;
            _outputDir = outputDir;
            _logger = new MonoGameConsoleLogger();
        }

        public override void AddDependency(string filename)
        {
            _dependencies.Add(filename);
        }

        public override string IntermediateDirectory { get { return _intermediateDir; } }
        public override ContentBuildLogger Logger { get { return _logger; } }
        public override string OutputDirectory { get { return _outputDir; } }
    }

    public class MonoGameConsoleLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            var msg = string.Format(message, messageArgs);
            var fileName = GetCurrentFilename(contentIdentity);
            Console.WriteLine("{0}: {1}", fileName, msg);
        }
    }
}
