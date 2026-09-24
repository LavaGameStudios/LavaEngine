dotnet build LavaEngineCompiler/LavaEngineCompiler.csproj -p:PublishSingleFile=true -o bin-debug/win-x64/ 
dotnet build LavaEngineCore/LavaEngineCore.csproj -o bin-debug/win-x64/ 
dotnet build LavaEngineScripts/LavaEngineScripts.csproj -o bin-debug/win-x64/ 