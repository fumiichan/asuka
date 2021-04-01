# Usage

## Basic Usage

Asuka has 4 main Commands:

-   `get` is used to retrieve and download doujinshi either by code or a collection of links in a text file.
-   `recommend` is used to get recommendations on specific doujinshi.
-   `search` is used to search a doujinshi.
-   `random` is used to get really a random doujinshi.

### `get`

Usage:

```
asuka get <-i|--input <file|url>> [-r|--readonly] [-p|--pack] [-o|--output <path>]
```

| Option                      | Required? | Description                                        |
|-----------------------------|-----------|----------------------------------------------------|
| `-i \| --input <code>`      | Yes       | Specify a gallery code to download.                |
| `-r \| --readonly`          | No        | View the information only.                         |
| `-p \| --pack`              | No        | Pack the downloaded doujinshi as CBZ archive       |
| `-o \| --output <path>`     | No        | Path to save the downloaded doujinshi.             |

**Usage Examples**

```
asuka get -i 177013
```

You need to add the links seperated by a new line.

#### Get the information only

```
asuka get -i https://nhentai.net/g/177013 -r
```

#### Pack the downloaded archive

```
asuka get -i https://nhentai.net/g/177013 -p
```

**Remarks**

-   `--readonly` option supports on single link only.

### `recommend`

Usage:

```
asuka recommend <-i|--input <url>> [-p|--pack] [-o|--output <path>]
```

| Option                  | Required? | Description                                        |
|-------------------------|-----------|----------------------------------------------------|
| `-i \| --input <code>`  | Yes       | Specify a gallery code to download.                |
| `-p \| --pack`          | No        | Pack the downloaded doujinshi as CBZ archive       |
| `-o \| --output <path>` | No        | Path to save the downloaded doujinshi.             |

**Usage Examples**

Usage is the same as on the `get` command except that it doesn't accept file paths as arguments.

### `search`

Usage:

```
asuka search [options]
```

| Option                       | Required? | Description                                          |
|------------------------------|-----------|------------------------------------------------------|
| `-q \| --query <query>`      | No        | Your search query                                    |
| `-e \| --exclude <query>`    | No        | Exclude tags in your search                          |
| `--pageRange <range>`        | No        | Filter for specifying minimum page count in gallery. |
| `--dateRange <timeframe>`    | No        | Filter for specifying exact timeframe                |
| `-p \| --page <num>`         | Yes       | Page Number.                                         |
| `--pack`                     | No        | Pack the downloaded doujinshi as CBZ archive         |
| `-o \| --output <path>`      | No        | Path to save the downloaded doujinshi.               |

**Usage Examples**

```
asuka search -q maid "\"big breasts\"" --page 1
```

Searching with finer queries:

Finer queries are

| Filter            | Type          | Description                                 | Examples                                     |
|-------------------|---------------|---------------------------------------------|----------------------------------------------|
| `category`        | `string`      | Specify the doujin category                 | `category:manga`                             |
| `artist`          | `string`      | Specify artist to filter                    | `artist:shindol`                             |
| `parody`          | `string`      | Specify parody of the doujin                | `parody:"to love ru"`                        |
| `tag`             | `string`      | Specify a tag                               | `tag:"big breasts"`                          |
| `character`       | `string`      | Specify a character in the doujin           | `character:astolfo`                          |
| `language`        | `string`      | Specify language of the doujin              | `language:english`                           |
| `group`           | `string`      | Specify the group (or circle) of the doujin | `group:poyopoyosky`                          |

**Remarks**

-   Use `--exclude` option to exclude tags instead of adding dashes (`-`) before queries.
-   `--dateRange` and `--pageRange` supports operators such as `>` `>=` `<` and `<=`.
-   `--dateRange` supports following date units (Ex: `--dateRange ">2d" "<=5d"`:
    -   `h` for hours
    -   `d` for days
    -   `w` for weeks
    -   `m` for months
    -   `y` for years

### `random`

Usage:

```
asuka random [-p|--pack] [-o|--output <path>]
```

| Option                     | Required? | Description                                  |
|----------------------------|-----------|----------------------------------------------|
| `--pack`                   | No        | Pack the downloaded doujinshi as CBZ archive |
| `-o \| --output <path>`    | No        | Path to save the downloaded doujinshi.       |
