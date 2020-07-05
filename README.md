![asuka logo](docs/banner.png)

[![Maintenance](https://badgen.net/badge/maintained%3F/yes/green)](https://github.com/aikoofujimotoo/asuka/graphs/commit-activity)
[![CircleCI](https://circleci.com/gh/aikoofujimotoo/asuka.svg?style=shield&circle-token=488813c48d642cdb1ff63cdb2483fdab55df8c19)](https://circleci.com/gh/aikoofujimotoo/asuka)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0059a755ff954e02ac9a10246170a3fb)](https://www.codacy.com?utm_source=github.com&utm_medium=referral&utm_content=aikoofujimotoo/asuka&utm_campaign=Badge_Grade)
[![Age Rating](https://badgen.net/badge/age%20rating/18+/red)](https://en.wikipedia.org/wiki/Age_of_majority)
[![MIT license](https://badgen.net/badge/license/MIT/green)](LICENSE)

## Features

-   Downloads your favorite doujinshi by code, search, random or by recommendation based from doujin code
-   Views your doujinshi information without going to the site
-   Simple and fast
-   Runs on Windows, Linux and mac OS.

## Requirements

-   Processor Architecture must be one of these:
    -   arm
    -   arm64
    -   x86
    -   x64

-   OS
    -   For Windows, this is compatible to Windows 7 SP1 or later.
        -   For Windows 7 SP1 and Windows 8.1, you need to install the following:
            -   [Microsoft Visual C++ 2015 Redistributable Update 3](https://www.microsoft.com/download/details.aspx?id=52685)
            -   [KB2533623](https://support.microsoft.com/en-gb/help/2533623/microsoft-security-advisory-insecure-library-loading-could-allow-remot)

    -   For Linux, these are currently supported distributions:
        -   Red Hat Enterprise Linux Version 6 or later
        -   CentOS, Oracle Linux Version 7 or later
        -   Fedora 30 or later
        -   Debian 9 or later
        -   Ubuntu 16.04, 18.04, 19.10, 20.04
        -   Linux Mint 18 or later
        -   openSUSE 15 or later
        -   SUSE Enterprise Linux (SLES) 12 SP2 or later
        -   Alpine Linux 3.10 or later.

    -   macOS 10.13 or later

-   [.NET Core Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Compiling from Source

This requires you to have .NET Core 3.1 SDK installed.

1.  Clone the repository. Run `git clone https://github.com/aikoofujimotoo/asuka`
2.  Run `dotnet restore` to restore packages
3.  Run `dotnet build`

If you are using Visual Studio, The packages are restored automatically after you open the project. Wait for it to finish and compile the project.

## Credits

-   [Asuka Fan Art by 黒輪](https://www.pixiv.net/en/artworks/60973409)

## License

This project is licensed under [MIT License](LICENSE).
