using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BlazorCacheBuster.Tasks
{
    public class UpdateAssets : Task
    {
        [Required]
        public string PublishDir { get; set; }

        [Required]
        public string BrotliCompressToolPath { get; set; }

        public bool DisableCacheBusting { get; set; }
        public bool CacheBustWithHash { get; set; }
        public bool CacheBustIndexHtml { get; set; }
        public bool BlazorEnableCompression { get; set; } = true;
        public string CompressionLevel { get; set; }
        public string CacheReplacement { get; set; } = "cache";
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

                var bootJson = File.ReadAllText(bootJsonPath);
                if (CacheBustWithHash)
                {
                    var scripts = Tools.GetInitializersInBoot(bootJson);
                    foreach (var (script, hash) in scripts)
                    {
                        Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Updating \"{script}.lib.module.js\" with hash \"{hash}\" as query string");
                        bootJson = bootJson.Replace($"{script}.lib.module.js", $"{script}.lib.module.js?q={hash}");
                    }
                }
                else
                {
                    Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Updating \"{bootJsonPath}\" with new query string: \"{CacheId}\"");
                    bootJson = bootJson.Replace(".lib.module.js", $".lib.module.js?q={CacheId}");
                }

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

            if (CacheBustIndexHtml)
            {
                var wwwrootDirs = Directory.GetDirectories(PublishDir, "wwwroot", SearchOption.AllDirectories);
                foreach (var wwwrootDir in wwwrootDirs)
                {
                    var indexHtmlPath = Path.Combine(wwwrootDir, "index.html");
                    var indexHtml = File.ReadAllText(indexHtmlPath);
                    indexHtml = indexHtml.Replace("q=cache", $"q={CacheId}");
                    File.WriteAllText(indexHtmlPath, indexHtml);
                }
            }

            return true;
        }
    }
}
