using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace BlazorCacheBuster.Tasks
{
    public class UpdateAssets : Task
    {
        [Required]
        public string PublishDir { get; set; } = string.Empty;

        [Required]
        public string BrotliCompressToolPath { get; set; } = string.Empty;

        public bool DisableCacheBusting { get; set; }
        public bool BustIndexHtml { get; set; }
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

                var bootJson = File.ReadAllText(bootJsonPath);
                var scripts = Tools.GetInitializersInBoot(bootJson);
                foreach (var (script, hash) in scripts)
                {
                    Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Updating \"{script}.lib.module.js\" with hash \"{hash}\" as query string");
                    bootJson = bootJson.Replace($"{script}.lib.module.js", $"{script}.lib.module.js?q={hash}");
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
                    if (!Tools.BrotliCompress(bootJsonPath, bootJsonBrPath, BrotliCompressToolPath, CompressionLevel!, Log))
                    {
                        return false;
                    }
                }
            }

            if (BustIndexHtml)
            {
                Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Trying to cache bust index.html");
                var wwwrootDirs = Directory.GetDirectories(PublishDir, "wwwroot", SearchOption.AllDirectories);
                foreach (var wwwrootDir in wwwrootDirs)
                {
                    var indexHtmlPath = Path.Combine(wwwrootDir, "index.html");
                    Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Busting Index");
                    var indexHtml = File.ReadAllText(indexHtmlPath);
                    var itemsToBust = Tools.FindScriptsInHtml(indexHtml, onlyCached: true);
                    foreach (var urlToBust in itemsToBust)
                    {
                        var cleanRelativePath = Tools.CleanUrl(urlToBust);
                        Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Trying to bust: {urlToBust}, relative {cleanRelativePath != null}");
                        if (cleanRelativePath == null)
                        {
                            indexHtml = indexHtml.Replace(urlToBust, urlToBust.Replace("q=cache", $"q={CacheId}"));
                            continue;
                        }
                        var cleanPaths = cleanRelativePath.Split('\\', '/');
                        var pathToBust = Path.Combine(cleanPaths.Prepend(wwwrootDir).ToArray());
                        var md5Hash = Tools.GetContentHashForFile(pathToBust);
                        Log.LogMessage(MessageImportance.High, $"BlazorCacheBuster: Trying as relative: {cleanRelativePath}, path: {pathToBust}, hash: {md5Hash}");
                        if (md5Hash == null)
                        {
                            indexHtml = indexHtml.Replace(urlToBust, Tools.AppendOrReplaceQuery(urlToBust, "q=cache", $"q={CacheId}", onlyReplace: true));
                            continue;
                        }
                        indexHtml = indexHtml.Replace(urlToBust, Tools.AppendOrReplaceQuery(urlToBust, "q=cache", $"q={md5Hash}", onlyReplace: true));
                    }
                    File.WriteAllText(indexHtmlPath, indexHtml);
                }
            }

            return true;
        }
    }
}
