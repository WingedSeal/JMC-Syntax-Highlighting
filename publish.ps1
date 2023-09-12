$dotnet_command = Get-Command "dotnet" -errorAction SilentlyContinue 
if (-Not $dotnet_command)
{
    "dotnet is not installed"
    "install dotnet in https://dotnet.microsoft.com/en-us/download"
    exit
}
$npm_command = Get-Command "npm" -errorAction SilentlyContinue
if (-Not $npm_command)
{
    "NPM is not installed"
    exit
}
$tsc_command = Get-Command "tsc" -errorAction SilentlyContinue
if (-Not $tsc_command)
{
    "tsc is not installed"
    exit
}


Set-Location .\vscode-ext
npm install
tsc --build tsconfig.json
Set-Location ..\
dotnet publish JMC.Extension.Server -p:PublishProfile=Windows -o .\vscode-ext\out\src\server -v q
dotnet publish JMC.Extension.Server -p:PublishProfile=macOS -o .\vscode-ext\out\src\server -v q
Set-Location .\vscode-ext
vsce publish
Set-Location ..\
