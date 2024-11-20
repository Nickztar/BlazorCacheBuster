
Push-Location $PSScriptRoot

function Write-Message {
    param([string]$message)
    Write-Host
    Write-Host $message
    Write-Host
}
function Confirm-PreviousCommand {
    param([string]$errorMessage)
    if ( $LASTEXITCODE -ne 0) { 
        if ( $errorMessage) {
            Write-Message $errorMessage
        }    
        exit $LASTEXITCODE 
    }
}

function Confirm-Process {
    param ([System.Diagnostics.Process]$process, [string]$errorMessage)
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) {
        Write-Host $process.ExitCode
        if ( $errorMessage) {
            Write-Message $errorMessage
        }    
        exit $process.ExitCode 
    }
}

Write-Host "Parameters"
Write-Host "=========="
Write-Host "Version suffix: test"

Write-Message "Cleaning ..."
Remove-Item -Path ../artifacts -Recurse -Force
Get-ChildItem ..\ -include bin, obj -Recurse | ForEach-Object ($_) { Remove-Item $_.FullName -Force -Recurse }

# Confirm-PreviousCommand

Write-Message "Building ..."
dotnet build ../lib/BlazorCacheBuster.Tasks/BlazorCacheBuster.Tasks.csproj -c Release
dotnet build ../lib/BlazorCacheBuster.BrotliCompress/BlazorCacheBuster.BrotliCompress.csproj -c Release
dotnet build ../lib/BlazorCacheBuster/BlazorCacheBuster.csproj -c Release /p:VersionSuffix="testJ"
Confirm-PreviousCommand

Write-Message "Creating nuget package ..."
dotnet pack ../lib/BlazorCacheBuster/BlazorCacheBuster.csproj -c Release /p:VersionSuffix="testJ" -o ../artifacts/nuget
Confirm-PreviousCommand

Write-Message "Building WASM Sample App ..."
dotnet publish ../Samples/WASM/SampleWASM.csproj -c Release -o ../artifacts/samples/WASM -v d
Confirm-PreviousCommand

Write-Message "Build completed successfully"