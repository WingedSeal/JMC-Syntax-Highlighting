using JMC.Shared.Helper;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Types
{
    internal class ExtensionWorkspaces : List<ExtensionWorkspace>
    {
        public ExtensionWorkspaces() { }
        public ExtensionWorkspaces(IEnumerable<ExtensionWorkspace> extensions) : base(extensions) { }

        public ExtensionWorkspace? GetWorkspaceByUri(DocumentUri documentUri) => 
            this.FirstOrDefault(v => 
                v.JMCFiles.Any(v => v.DocumentUri == documentUri) || 
                v.HJMCFiles.Any(v => v.DocumentUri == documentUri)
            );

        public JMCFile? GetJMCFile(DocumentUri documentUri) =>
            Find(v => v.GetJMCFile(documentUri) != null)?.GetJMCFile(documentUri);
        public HJMCFile? GetHJMCFile(DocumentUri documentUri) =>
            Find(v => v.GetHJMCFile(documentUri) != null)?.GetHJMCFile(documentUri);

        public bool DeleteJMCFile(DocumentUri documentUri)
        {
            var workspace = Find(v => v.GetJMCFile(documentUri) != null);
            if (workspace == null) return false;
            var file = workspace.GetJMCFile(documentUri);
            if (file == null) return false;

            return workspace.JMCFiles.Remove(file);
        }

        public bool CreateJMCFile(DocumentUri documentUri)
        {
            var workspace = Find(v =>
            {
                var wPath = v.Root.GetFileSystemPath();
                var fPath = documentUri.GetFileSystemPath();
                return fPath.IsSubDirectoryOf(wPath);
            });
            if (workspace == null) return false;
            workspace.JMCFiles.Add(new(new(), documentUri));
            return true;
        }
    }
}
