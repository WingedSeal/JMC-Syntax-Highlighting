cd .\vscode-ext
npm install
tsc --build tsconfig.json
cd ..\
dotnet publish JMC.Extension.Server -p:PublishProfile=Windows -o .\vscode-ext\out\src\server -v q
dotnet publish JMC.Extension.Server -p:PublishProfile=macOS -o .\vscode-ext\out\src\server -v q
cd .\vscode-ext
vsce publish
cd ..\