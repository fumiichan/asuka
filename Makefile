# This restores the dependencies in entire solution
restore:
	dotnet restore

# Builds the project
.PHONY: build
build:
	# Restore first the dependencies within the solution
	dotnet restore

	# Build main application first
	dotnet publish -p:PublishSingleFile=true --self-contained false -o ./dist/ asuka.Application/asuka.csproj

	# Build the plugins
	dotnet build -c Release -o ./dist/providers/nhentai asuka.Provider.Nhentai/asuka.Provider.Nhentai.csproj
	dotnet build -c Release -o ./dist/providers/koharu asuka.Provider.Koharu/asuka.Provider.Koharu.csproj
	dotnet build -c Release -o ./dist/providers/hitomi asuka.Provider.Hitomi/asuka.Provider.Hitomi.csproj
