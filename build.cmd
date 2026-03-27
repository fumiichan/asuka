@echo off

:: Restore packages
dotnet restore asuka.slnx

:: Build main application
dotnet publish --no-self-contained -o ./dist/ asuka.Application/asuka.csproj
dotnet build -c Release -o ./dist/providers/nhentai asuka.Provider.Nhentai/asuka.Provider.Nhentai.csproj
dotnet build -c Release -o ./dist/providers/koharu asuka.Provider.Koharu/asuka.Provider.Koharu.csproj
dotnet build -c Release -o ./dist/providers/hitomi asuka.Provider.Hitomi/asuka.Provider.Hitomi.csproj

:: Build tools
dotnet publish --no-self-contained -o ./dist/tools asuka.JsonToComicInfoMigrator/asuka.JsonToComicInfoMigrator.csproj

pause
