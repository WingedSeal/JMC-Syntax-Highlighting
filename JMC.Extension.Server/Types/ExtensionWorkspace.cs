using JMC.Parser.JMC;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Types
{
    internal class ExtensionWorkspace
    {
        public List<JMCFile> JMCFiles { get; set; } = [];
        public DocumentUri Root { get; set; }
        public ExtensionWorkspace(DocumentUri root)
        {
            Root = root;
            JMCFiles = InitJMCFiles();
        }

        public List<JMCFile> InitJMCFiles()
        {
            var files = new List<JMCFile>();

            //get path
            var path = DocumentUri.GetFileSystemPath(Root);
            if (path == null) return files;

            //parse files
            var jmcFiles = Directory.GetFiles(path, "*.jmc", SearchOption.AllDirectories);
            var arr = jmcFiles.AsSpan();
            for (var i = 0; i < arr.Length; i++)
            {
                ref var file = ref arr[i];
                var text = File.ReadAllText(file);
                var tree = new JMCSyntaxTree().InitializeAsync(text).Result;
                var jmcFile = new JMCFile(tree, DocumentUri.File(file));
                files.Add(jmcFile);
            }

            return files;
        }

        public async Task<List<JMCFile>> InitJMCFilesAsync()
        {
            var files = new List<JMCFile>();

            //get path
            var path = DocumentUri.GetFileSystemPath(Root);
            if (path == null) return files;

            //parse files
            var jmcFiles = Directory.GetFiles(path, "*.jmc", SearchOption.AllDirectories);
            for (var i = 0; i < jmcFiles.Length; i++)
            {
                var file = jmcFiles[i];
                var text = File.ReadAllText(file);
                var tree = await new JMCSyntaxTree().InitializeAsync(text);
                var jmcFile = new JMCFile(tree, DocumentUri.File(file));
                files.Add(jmcFile);
            }

            return files;
        }

        public JMCFile? GetJMCFile(DocumentUri documentUri) => JMCFiles.Find(v => v.DocumentUri == documentUri);
    }
}
