using BlazorCacheBuster.Tasks;

namespace BlazorCacheBuster.Tests;

public class ScriptCacheTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldFindScriptTags(bool onlyCached)
    {
        var html = """
            <body>
                <div id="app">
                    <svg class="loading-progress">
                        <circle r="40%" cx="50%" cy="50%" />
                        <circle r="40%" cx="50%" cy="50%" />
                    </svg>
                    <div class="loading-progress-text"></div>
                </div>

                <div id="blazor-error-ui">
                    An unhandled error has occurred.
                    <a href="" class="reload">Reload</a>
                    <a class="dismiss">🗙</a>
                </div>
                <script src="_framework/blazor.webassembly.js?q=cache"></script>
                <script src='_framework/blazor.webassembly.js?q=cache'></script>
                <link href='_framework/blazor.webassembly.css?key=rat&q=cache'></script>
                <link href="_framework/blazor.webassembly.css?key=rat&q=cache"></script>
                <script src="_framework/blazor.webassembly.js"></script>
                <script src='_framework/blazor.webassembly.js'></script>
                <link href='_framework/blazor.webassembly.css?key=rat'></script>
                <link href="_framework/blazor.webassembly.css?key=rat"></script>
            </body>
        """;
        
        var scripts = Tools.FindScriptsInHtml(html, onlyCached).ToList();
        string[] shouldContain = onlyCached ? [
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.css?key=rat&q=cache",
            "_framework/blazor.webassembly.css?key=rat&q=cache"
        ] : [
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.css?key=rat&q=cache",
            "_framework/blazor.webassembly.css?key=rat&q=cache",
            "_framework/blazor.webassembly.js", 
            "_framework/blazor.webassembly.js", 
            "_framework/blazor.webassembly.css?key=rat",
            "_framework/blazor.webassembly.css?key=rat"
        ];
        scripts.Should().HaveCount(shouldContain.Length);
        scripts.Should().BeEquivalentTo(shouldContain);
    }
    
    [Fact]
    public void CanEnsureCacheInQuery()
    {
        var hash = "94360E2";
        string[] examples = [
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.js?q=cache", 
            "_framework/blazor.webassembly.css?key=rat&q=cache",
            "_framework/blazor.webassembly.css?key=rat&q=cache",
            "_framework/blazor.webassembly.js", 
            "_framework/blazor.webassembly.js", 
            "_framework/blazor.webassembly.css?key=rat",
            "_framework/blazor.webassembly.css?key=rat"
        ];
        foreach (var sample in examples)
        {
            var newSample = Tools.AppendOrReplaceQuery(sample, "q=cache", $"q={hash}");
            newSample.Should().Contain($"q={hash}");
            newSample.Should().NotContain("q=cache");
        }
    }
    
    [Theory]
    [InlineData("_framework/blazor.webassembly.js?q=cache", "_framework/blazor.webassembly.js")]
    [InlineData("https://ratscape.com/_framework/blazor.webassembly.js?q=cache", null)]
    public void ExampleFromUrl(string url, string? cleanUrl)
    {
        Tools.CleanUrl(url).Should().Be(cleanUrl);
    }
    
    [Theory]
    [InlineData("https://ratscape.com/_framework/blazor.webassembly.js?q=cache", null)]
    [InlineData("Assets/blazor.boot.json", "94360E2")]
    [InlineData("Assets/app.css", "854DB03")]
    public void GetUniqueishHashForFile(string path, string? expectedHash)
    {
        var hash = Tools.GetContentHashForFile(path);
        hash.Should().Be(expectedHash);
    }
}