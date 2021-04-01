![Wao Beeg Banner](docs/banner.png)

[![Maintenance](https://badgen.net/badge/maintained%3F/yes/green)](https://github.com/aikoofujimotoo/asuka/graphs/commit-activity)
[![CircleCI](https://circleci.com/gh/aikoofujimotoo/asuka.svg?style=shield&circle-token=488813c48d642cdb1ff63cdb2483fdab55df8c19)](https://circleci.com/gh/aikoofujimotoo/asuka)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/fd7d1abe2865463c93e091fc1f205dbe)](https://www.codacy.com/gh/aikoofujimotoo/asuka/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=aikoofujimotoo/asuka&amp;utm_campaign=Badge_Grade)
[![Age Rating](https://badgen.net/badge/age%20rating/18+/red)](https://en.wikipedia.org/wiki/Age_of_majority)
[![MIT license](https://badgen.net/badge/license/MIT/green)](LICENSE)

Cross-platform nhentai downloader on Console.

## Requirements

-   [.NET 5.0 Runtime](https://dotnet.microsoft.com/download/dotnet/5.0)

-   For supported platforms check [here](https://github.com/dotnet/core/blob/main/release-notes/5.0/5.0-supported-os.md)*.
    -   *Releases supports x64 Operating Systems only. You cannot use this on x86 or ARM. Check Compiling from Source section for compiling builds for these platforms.*

## Usage

By running `asuka --help` you mostly see everything you need to know how to use the client.

If you want in-depth examplaination and examples, see [here](docs/USAGE.md).

## Compiling from Source

### What do I need

-   [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

### Compiling

To compile, simply use your Terminal of your choice and navigate towards the asuka's source root.

1.  `dotnet restore` to restore the packages.

2.  `dotnet build` to build.

    You can use `--configuration` to specify the configuration to use. Available configurations are `Debug` and `Release`. For more information, check the [`dotnet build` documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build).

You'll see the built packages on `bin\Debug\net5.0` or `bin\Release\net5.0` depending on the build configuration you used.

### License

This project is licensed under [MIT license](LICENSE). For more information about the license, read the [LICENSE](LICENSE) file. It's short I promise.