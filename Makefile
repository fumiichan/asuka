.PHONY: build
build:
	dotnet restore asuka.Application/asuka.csproj
	dotnet publish --no-self-contained -o ./dist/ asuka.Application/asuka.csproj
	
	dotnet restore asuka.Provider.Nhentai/asuka.Provider.Nhentai.csproj
	dotnet build -c Release -o ./dist/providers/nhentai asuka.Provider.Nhentai/asuka.Provider.Nhentai.csproj
	
	dotnet restore asuka.Provider.Koharu/asuka.Provider.Koharu.csproj
	dotnet build -c Release -o ./dist/providers/koharu asuka.Provider.Koharu/asuka.Provider.Koharu.csproj
	
	dotnet restore asuka.Provider.Hitomi/asuka.Provider.Hitomi.csproj
	dotnet build -c Release -o ./dist/providers/hitomi asuka.Provider.Hitomi/asuka.Provider.Hitomi.csproj
