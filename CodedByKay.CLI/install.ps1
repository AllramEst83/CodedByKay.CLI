# PowerShell script to publish, rename, and move .NET application executable from the project root

# Before running the script make shure the terminal path is at: PS <your path c:>\CodedByKay.CLI\CodedByKay.CLI>
# You shuld be in the same terminal path as the file install.ps1




# Navigate to the script's directory to ensure relative paths work
Set-Location -Path $PSScriptRoot

# Assuming the script is in the same directory as the csproj file, adjust for the solution path
$solutionPath = Join-Path -Path $PSScriptRoot -ChildPath "..\CodedByKay.CLI.sln"

# Define the target installation directory
$installDir = "C:\devtools"

# Correctly reference the csproj file relative to the script's directory
$csprojPath = ".\CodedByKay.CLI.csproj"

# Publish the project using the csproj file
dotnet publish $csprojPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

# Define the publish directory and original executable name
$publishDir = Join-Path -Path $PSScriptRoot -ChildPath "..\CodedByKay.CLI\bin\Release\net8.0\win-x64\publish"
$originalExeName = "CodedByKay.CLI.exe"
$originalPath = Join-Path -Path $publishDir -ChildPath $originalExeName

# New executable name
$newExeName = "smartd.exe"
$newPath = Join-Path -Path $publishDir -ChildPath $newExeName

# Rename the executable
if (Test-Path -Path $originalPath) {
    Rename-Item -Path $originalPath -NewName $newExeName
} else {
    Write-Host "The original executable was not found."
    exit
}

# Check if the installation directory exists, and create it if it doesn't
if (-not (Test-Path -Path $installDir)) {
    New-Item -ItemType Directory -Path $installDir
}

# Move the renamed executable to the installation directory
Move-Item -Path $newPath -Destination $installDir -Force

# Output a completion message
Write-Host "Application published, renamed, and moved to $installDir successfully."

# Function to merge two JSON objects
function Merge-JsonObjects($baseJson, $envJson) {
    foreach ($prop in $envJson.PSObject.Properties) {
        $baseJson.$($prop.Name) = $prop.Value
    }
    return $baseJson
}

# Read the base appsettings.json
$baseAppSettingsPath = Join-Path -Path $publishDir -ChildPath "appsettings.json"
$baseJson = Get-Content $baseAppSettingsPath -Raw | ConvertFrom-Json

# Read the environment-specific appsettings (e.g., appsettings.Development.json)
$envAppSettingsPath = Join-Path -Path $publishDir -ChildPath "appsettings.Development.json"
$envJson = Get-Content $envAppSettingsPath -Raw | ConvertFrom-Json

# Merge the JSON objects
$mergedJson = Merge-JsonObjects -baseJson $baseJson -envJson $envJson

# Convert merged JSON object back to JSON string
$mergedJsonString = $mergedJson | ConvertTo-Json -Depth 100

# Define the target appsettings.json path in the installation directory
$targetAppSettingsPath = Join-Path -Path $installDir -ChildPath "appsettings.json"

# Write the merged configuration to the target directory
$mergedJsonString | Out-File -FilePath $targetAppSettingsPath -Force

Write-Host "Merged appsettings.json has been copied to the target directory."