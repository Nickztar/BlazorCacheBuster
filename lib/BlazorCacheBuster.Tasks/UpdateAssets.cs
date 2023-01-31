using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BlazorCacheBuster.Tasks
{
    public class UpdateAssets : Task
    {
        [Required]
        public string? PublishDir { get; set; }

        [Required]
        public string BrotliCompressToolPath { get; set; }

        public bool DisableCacheBusting { get; set; }
        public bool BlazorEnableCompression { get; set; } = true;
        public string? CompressionLevel { get; set; }

        public string CacheId { get; set; } = Guid.NewGuid().ToString("N");

        public override bool Execute()
        {
            if (DisableCacheBusting)
            {
                return true;
            }
            var frameworkDirs = Directory.GetDirectories(PublishDir, "_framework", SearchOption.AllDirectories);
            foreach (var frameworkDir in frameworkDirs)
            {
                var bootJsonPath = Path.Combine(frameworkDir, "blazor.boot.json");
                var bootJsonGzPath = Path.Combine(frameworkDir, "blazor.boot.json.gz");
                var bootJsonBrPath = Path.Combine(frameworkDir, "blazor.boot.json.br");

                Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Updating \"{bootJsonPath}\"");
                var bootJson = File.ReadAllText(bootJsonPath);
                bootJson = bootJson.Replace(".lib.module.js", $".lib.module.js?q={CacheId}");
                File.WriteAllText(bootJsonPath, bootJson);

                if (File.Exists(bootJsonGzPath) && BlazorEnableCompression)
                {
                    Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Recompressing \"{bootJsonGzPath}\"");
                    if (!Tools.GZipCompress(bootJsonPath, bootJsonGzPath, Log))
                    {
                        return false;
                    }
                }
                if (File.Exists(bootJsonBrPath) && BlazorEnableCompression)
                {
                    Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Recompressing \"{bootJsonBrPath}\"");
                    if (!Tools.BrotliCompress(bootJsonPath, bootJsonBrPath, BrotliCompressToolPath, CompressionLevel, Log))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
