START /B /wait cmd /c "npm install"
START /B /wait cmd /c "tsc --build tsconfig.json"
vsce publish