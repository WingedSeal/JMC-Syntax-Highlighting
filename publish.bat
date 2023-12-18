@echo off
cd .\vscode-ext
call npm install
call tsc --build tsconfig.json
cd ..\
call dotnet publish JMC.Extension.Server -p:PublishProfile=Windows -o .\vscode-ext\out\src\server -v q
call dotnet publish JMC.Extension.Server -p:PublishProfile=macOS -o .\vscode-ext\out\src\server -v q
cd .\vscode-ext
call vsce publish
cd ..\
pause