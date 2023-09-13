using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class Workspace
    {
        public List<JMCFile> JMCFiles { get; set; } = new();
        public List<HJMCFile> HJMCFiles { get; set; } = new();
        public JMCConfig Config { get; set; }
        public string Path { get; set; } = string.Empty;
        public DocumentUri DocumentUri { get; set; }
        public Workspace(DocumentUri Uri)
        {
            var fspath = DocumentUri.GetFileSystemPath(Uri);
            if (fspath == null)
            {
                DocumentUri = DocumentUri.From(Path);
                return;
            }
            Path = fspath;
            DocumentUri = Uri;


            var jmcfiles = Directory.GetFiles(fspath, "*.jmc", SearchOption.AllDirectories);
            JMCFiles = jmcfiles.Select(v => new JMCFile(v)).ToList();

            var hjmcfiles = Directory.GetFiles(fspath, "*.hjmc", SearchOption.AllDirectories);
            HJMCFiles = hjmcfiles.Select(v => new HJMCFile(v)).ToList();

            var configs = Directory.GetFiles(fspath, "jmc_config.json");
            if (configs.Length > 1)
            {
                JMCLanguageServer.Server.ShowWarning("jmc_config.json must only contains one in each workspace!");
            }
            Config = JsonConvert.DeserializeObject<JMCConfig>(File.ReadAllText(configs.First()));
        }

        /// <summary>
        /// Search for a JMC File
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public JMCFile? FindJMCFile(DocumentUri uri) => JMCFiles.Find(v => v.DocumentUri == uri);
        public HJMCFile? FindHJMCFile(DocumentUri uri) => HJMCFiles.Find(v => v.DocumentUri == uri);
    }
}
