# Usage

## Basic Usage

Asuka has 5 main Commands:

-   `get` is used to retrieve and download manga either by code or a collection of links in a text file.
-   `recommend` is used to get recommendations on specific manga.
-   `search` is used to search a manga.
-   `file` is used to download entire list of manga in a text file.
-   `random` is used to get really a random manga.

### `get`

Usage:

```text
asuka get <-i|--input <code|url>> [-r|--readonly] [-p|--pack] [-o|--output <path>]
```

| Option                      | Required? | Description                                        |
|-----------------------------|-----------|----------------------------------------------------|
| `-i \| --input <code>`      | Yes       | Specify a gallery code to download.                |
| `-r \| --readonly`          | No        | View the information only.                         |
| `-p \| --pack`              | No        | Pack the downloaded doujinshi as CBZ archive       |
| `-o \| --output <path>`     | No        | Path to save the downloaded doujinshi.             |
| `--useTachiyomiLayout`      | No        | Use Tachiyomi Folder Structure                     |

**Usage Examples**

```text
asuka get -i 177013
```

You need to add the links seperated by a new line.

#### Get the information only

```text
asuka get -i https://nhentai.net/g/177013 -r
```

#### Pack the downloaded archive

```text
asuka get -i https://nhentai.net/g/177013 -p
```

**Remarks**

-   `--readonly` option supports on single link only.

### `recommend`

Usage:

```text
asuka recommend <-i|--input <code>> [-p|--pack] [-o|--output <path>]
```

| Option                  | Required?   | Description                                        |
|-------------------------|-------------|----------------------------------------------------|
| `-i \| --input <code>`  | Yes         | Specify a gallery code to download.                |
| `-p \| --pack`          | No          | Pack the downloaded doujinshi as CBZ archive       |
| `--useTachiyomiLayout`  | No          | Use Tachiyomi Folder Structure                     |
| `-o \| --output <path>` | No          | Path to save the downloaded doujinshi.             |

**Usage Examples**

Usage is the same as on the `get` command except that it doesn't accept file paths as arguments.

### `search`

Usage:

```text
asuka search [options]
```

| Option                       | Required? | Description                                          |
|------------------------------|-----------|------------------------------------------------------|
| `-q \| --query <query>`      | No        | Your search query                                    |
| `-e \| --exclude <query>`    | No        | Exclude tags in your search                          |
| `--pageRange <range>`        | No        | Filter for specifying minimum page count in gallery. |
| `--dateRange <timeframe>`    | No        | Filter for specifying exact timeframe                |
| `-p \| --page <num>`         | Yes       | Page Number.                                         |
| `--pack`                     | No        | Pack the downloaded manga as CBZ archive             |
| `--sort <sort>`              | No        | Sort results. (default: `date`)                      |
| `-o \| --output <path>`      | No        | Path to save the downloaded doujinshi.               |
| `--useTachiyomiLayout`       | No        | Use Tachiyomi Folder Structure                       |

**Sort Options:**

| Sort                         | Description                                             |
|------------------------------|---------------------------------------------------------|
| `date`                       | Sorts result from newest to oldest.                     |
| `popular-week`               | Sorts result from most popular to less popular (Weekly) |
| `popular-today`              | Sorts result from most popular to less popular (Today)  |
| `popular`                    | Sorts result from most popular to less popular          |

**Finer Queries:**

| Filter            | Type          | Description                                 | Examples                                     |
|-------------------|---------------|---------------------------------------------|----------------------------------------------|
| `category`        | `string`      | Specify the manga category                  | `category:manga`                             |
| `artist`          | `string`      | Specify artist to filter                    | `artist:shindol`                             |
| `parody`          | `string`      | Specify parody of the manga                 | `parody:"to love ru"`                        |
| `tag`             | `string`      | Specify a tag                               | `tag:"wholesome"`                            |
| `character`       | `string`      | Specify a character in the manga            | `character:astolfo`                          |
| `language`        | `string`      | Specify language of the manga               | `language:english`                           |
| `group`           | `string`      | Specify the group (or circle) of the manga  | `group:poyopoyosky`                          |

**Remarks**

-   Use `--exclude` option to exclude tags instead of adding dashes (`-`) before queries.
-   `--dateRange` and `--pageRange` supports operators such as `>` `>=` `<` and `<=`.
-   `--dateRange` supports following date units (Ex: `--dateRange ">2d" "<=5d"`:
    -   `h` for hours
    -   `d` for days
    -   `w` for weeks
    -   `m` for months
    -   `y` for years

### `file`

Usage:

```text
asuka file <-f|--file <filepath>> [options]
```

| Option                       | Required? | Description                                          |
|------------------------------|-----------|------------------------------------------------------|
| `-f \| --file <file>`        | Yes       | Path to the text file with list of URLs              |
| `--pack`                     | No        | Pack the downloaded manga as CBZ archive             |
| `-o \| --output <path>`      | No        | Path to save the downloaded doujinshi.               |
| `--useTachiyomiLayout`       | No        | Use Tachiyomi Folder Structure                       |

### `random`

Usage:

```text
asuka random [-p|--pack] [-o|--output <path>]
```

| Option                     | Required? | Description                                  |
|----------------------------|-----------|----------------------------------------------|
| `--pack`                   | No        | Pack the downloaded manga as CBZ archive     |
| `-o \| --output <path>`    | No        | Path to save the downloaded doujinshi.       |
