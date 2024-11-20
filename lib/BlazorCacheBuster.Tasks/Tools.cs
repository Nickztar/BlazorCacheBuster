using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BlazorCacheBuster.Tasks
{
    public static class Tools
    {
        public static bool GZipCompress(string source, string target, TaskLoggingHelper log)
        {
            try
            {
                File.Delete(target);
                using var fileStream = File.OpenRead(source);
                using var stream = File.Create(target);
                using var destination = new GZipStream(stream, CompressionLevel.Optimal);
                fileStream.CopyTo(destination);
            }
            catch (Exception ex)
            {
                if (File.Exists(target))
                {
                    File.Delete(target);
                }
                log.LogErrorFromException(ex, true, true, null);
                return false;
            }
            return true;
        }

        public static bool BrotliCompress(string source, string target, string brotliCompressToolPath, string compressionLevel, TaskLoggingHelper log)
        {
            try
            {
                // NOTE: This MSBuild Custom Task will run not only on .NET 6 or later but also on .NET Framework 4.x.
                //       Therefore the `BrotliStream` class is not usable in this MSBuild Custom Task due to
                //       the `BrotliStream` class is not provided on.NET Framework.Instead, we can do that
                //       with execution as an out process.
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"exec \"{brotliCompressToolPath}\" \"{source}\" \"{target}\" \"{compressionLevel}\""
                };
                log.LogMessage(MessageImportance.Low, $"{startInfo.FileName} {startInfo.Arguments}");
                var process = Process.Start(startInfo);
                process?.WaitForExit();
                if (process?.ExitCode != 0)
                    throw new Exception($"The exit code of recompressing with Brotli command was not 0. (it was {process?.ExitCode})");
            }
            catch (Exception ex)
            {
                if (File.Exists(target))
                {
                    File.Delete(target);
                }
                log.LogErrorFromException(ex, true, true, null);
                return false;
            }
            return true;
        }

        private const string ModuleRegExp = "\"(.*).lib.module.js\": \"(.*)\"";

        public static List<(string Script, string Hash)> GetInitializersInBoot(string bootJson)
        {
            List<(string Script, string Hash)> scriptsFound = new List<(string Module, string Hash)>();
            var matches = Regex.Matches(bootJson, ModuleRegExp);
            foreach (Match match in matches)
            {
                //Valid matches are contain two, 
                //First one is the full group (Whole row)
                //Second one for the script module
                //And third and final one for the hash
                if (match.Groups.Count == 3)
                {
                    var script = match.Groups[1].Value;
                    var hash = match.Groups[2].Value;
                    scriptsFound.Add((script, hash));
                }
            }
            return scriptsFound;
        }

        public static string? GetContentHashForFile(string? filePath)
        {
            try
            {
                if (filePath is null) return null;
                using var md5 = MD5.Create();
                using var stream = File.OpenRead(filePath);
                var contentHash = md5.ComputeHash(stream);
                return string.Concat(contentHash.Select(x => x.ToString("X2")))[..7];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private const string CacheTagsRegExp = "<(script|link)[a-z1-9\"'\\/ =]*?(src|href)=['\"]((.*?)q=cache)['\"]";
        private const string PathTagsRegExp = "<(script|link)[a-z1-9\"'\\/ =]*?(src|href)=['\"]((.*?))['\"]";

        public static IEnumerable<string> FindScriptsInHtml(string html, bool onlyCached = true)
        {
            var matches = Regex.Matches(html, onlyCached ? CacheTagsRegExp : PathTagsRegExp);
            foreach (Match match in matches)
            {
                //Valid matches are contain two, 
                //First one is the full group (Whole row)
                //Second one for the script module
                //And third and final one for the hash
                var script = match.Groups[3].Value;
                yield return script;
            }
        }


        public static string? CleanUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Relative, out var uri))
            {
                return null;
            }
            var path = url.Split('?', '#')[0];
            return path;
        }
        
        public static string AppendOrReplaceQuery(string source, string toReplace, string replacement, bool onlyReplace = false)
        {
            if (source.Contains(toReplace))
            {
                return source.Replace(toReplace, replacement);
            }
            
            if (onlyReplace) return source;
            
            if (source.Contains('?'))
            {
                return source + replacement;
            }
            return source + "?" + replacement;
        }
    }
}
