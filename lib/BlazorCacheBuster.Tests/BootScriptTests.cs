using BlazorCacheBuster.Tasks;

namespace BlazorCacheBuster.Tests;

public class BootScriptTests
{
    string bootScriptFile;
    public BootScriptTests()
    {
        bootScriptFile = File.ReadAllText("Assets/blazor.boot.json");
    }
    
    [Fact]
    public void CanFindScriptsInBootFile()
    {
        var bootFiles = Tools.GetInitializersInBoot(bootScriptFile);
        bootFiles.Should().Contain(("_content/BlazorWasmAntivirusProtection/BlazorWasmAntivirusProtection", "sha256-OR6+lSoCe5oK8IgysXULNFu3+eC370PZSnllZAzA5TY="));
    }
}