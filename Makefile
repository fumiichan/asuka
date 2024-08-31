# This restores the dependencies in entire solution
restore:
	dotnet restore

# Builds the project on *nix platforms
.PHONY: build
build:
	# Restore first the dependencies within the solution
	dotnet restore

	# Clean previous artifacts if exists
	rm -rf dist/
	mkdir -p dist/providers

	# Build main application first
	dotnet publish -p:PublishSingleFile=true --self-contained true -o ./dist/ asuka.Application/asuka.csproj

	# Build the plugins
	dotnet build -c Release -o dist/providers asuka.Provider.Nhentai/asuka.Provider.Nhentai.csproj

# Same stuff as build command, but Windows
.PHONY: build-windows
build-windows:
	# Restore first the dependencies within the solution
	dotnet restore

    # Clean previous artifacts if exists
	rd /q /s dist 2>NUL
	mkdir dist
	mkdir dist\providers
    
    # Build main application first
	dotnet publish -p:PublishSingleFile=true --self-contained false -o dist\ asuka.Application\asuka.csproj

	# Build the plugins
	dotnet build -c Release -o dist\providers asuka.Provider.Nhentai\asuka.Provider.Nhentai.csproj
