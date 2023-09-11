using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace JMC.Extension.Server.Datas.Workspace
{
    internal class Workspace
    {
        public List<JMCFile> JMCFiles { get; set; } = new();
        public List<HJMCFile> HJMCFiles { get; set; } = new();
        public JMCConfig Config { get; set; }
        public string Path { get; set; }
        public DocumentUri DocumentUri { get; set; }
#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
        public Workspace(DocumentUri Uri)
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null ?。請考慮宣告為可為 Null。
        {
            var fspath = DocumentUri.GetFileSystemPath(Uri);
            if (fspath == null)
                return;
            Path = fspath;
            DocumentUri = Uri;

            var jmcfiles = Directory.GetFiles(fspath, "*.jmc", SearchOption.AllDirectories);
            JMCFiles = jmcfiles.Select(v => new JMCFile(v)).ToList();

            var hjmcfiles = Directory.GetFiles(fspath, "*.hjmc", SearchOption.AllDirectories);
            HJMCFiles = hjmcfiles.Select(v => new HJMCFile(v)).ToList();

            var config = Directory.GetFiles(fspath, "jmc_config.json");
        }

        /// <summary>
        /// Search for a JMC File
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public JMCFile? FindJMCFile(DocumentUri uri) => JMCFiles.Find(v => v.DocumentUri == uri);
    }
}
