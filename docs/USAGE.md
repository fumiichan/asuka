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
| `-i \| --input <url\|file>` | Yes       | Specify a URL or a path to a text file to download |
| `-r \| --readonly`          | No        | View the information only.                         |
| `-p \| --pack`              | No        | Pack the downloaded doujinshi as CBZ archive       |
| `-o \| --output <path>`     | No        | Path to save the downloaded doujinshi.             |

#### Usage Examples

##### Get a single doujinshi:

```
asuka get -i https://nhentai.net/g/177013
```

##### Download multiple doujinshi:

```
asuka get -i ~/Downloads/file.txt
```

where the text file contains:

```
https://nhentai.net/g/177013
https://nhentai.net/g/177014
https://nhentai.net/g/177015
```

You need to add the links seperated by a new line.

##### Get the information only

```
asuka get -i https://nhentai.net/g/177013 -r
```

__Please take note that readonly option works only on single code.__

##### Pack the downloaded archive

```
asuka get -i https://nhentai.net/g/177013 -p
```

### `recommend`

Usage:

```
asuka recommend <-i|--input <url>> [-p|--pack] [-o|--output <path>]
```

| Option                  | Required? | Description                                        |
|-------------------------|-----------|----------------------------------------------------|
| `-i \| --input <url>`   | Yes       | Specify a URL or a path to a text file to download |
| `-p \| --pack`          | No        | Pack the downloaded doujinshi as CBZ archive       |
| `-o \| --output <path>` | No        | Path to save the downloaded doujinshi.             |


#### Usage Examples

Usage is the same as on the `get` command except that it doesn't accept file paths as arguments.

### `search`

```
asuka search [--pack] <-q|--query <query>> [-e|--exlude <query>] <-p|--page <num>> [-o|--output <path>]
```

| Option                     | Required? | Description                                  |
|----------------------------|-----------|----------------------------------------------|
| `-q \| --query <query>`    | Yes       | Your search query                            |
| `-e \| --exclude <query>`  | No        | Exclude tags in your search                  |
| `-p \| --page <num>`       | Yes       | Page Number.                                 |
| `--pack`                   | No        | Pack the downloaded doujinshi as CBZ archive |
| `-o \| --output <path>`    | No        | Path to save the downloaded doujinshi.       |

#### Usage Examples

```
asuka search -q maid "\"big breasts\"" -p 1
```

Searching with finer queries:

Finer queries are

| Filter            | Type          | Description                                 | Examples                                     |
|-------------------|---------------|---------------------------------------------|----------------------------------------------|
| `pages`           | `number`      | Specify the total pages                     | `page:3`, `page:>3`, `page:>=30 page:<=75`   |
| `category`        | `string`      | Specify the doujin category                 | `category:manga`                             |
| `artist`          | `string`      | Specify artist to filter                    | `artist:shindol`                             |
| `parody`          | `string`      | Specify parody of the doujin                | `parody:"to love ru"`                        |
| `tag`             | `string`      | Specify a tag                               | `tag:"big breasts"`                          |
| `character`       | `string`      | Specify a character in the doujin           | `character:astolfo`                          |
| `language`        | `string`      | Specify language of the doujin              | `language:english`                           |
| `group`           | `string`      | Specify the group (or circle) of the doujin | `group:poyopoyosky`                          |
| `uploaded`        | `string`      | Specify items uploaded within timeframe     | `uploaded:20d`                               |

##### Notes

-   `pages` & `uploaded` filter accepts comparison operators such as `>` (greater than), `<` (less than), `>=` (greater or equal to) & `<=` (less than or equal to)
-   Use `--exclude` option to exclude tags instead of adding dashes (`-`) before queries.

```
asuka search -q "\"big breasts\"" "page:>60" -p 1
```

```
asuka search -q "parody:\"to love ru\"" "language:japanese" -p 1
```

### `random`

```
asuka random [-p|--pack] [-o|--output <path>]
```

| Option                     | Required? | Description                                  |
|----------------------------|-----------|----------------------------------------------|
| `--pack`                   | No        | Pack the downloaded doujinshi as CBZ archive |
| `-o \| --output <path>`    | No        | Path to save the downloaded doujinshi.       |

## Configuration

Configuration are automatically created at runtime. You can modify values within `config.yaml` file.

### Available configuration items

| Key name                 | Value Type    | Default value      | Description                            |
|--------------------------|---------------|--------------------|----------------------------------------|
| ConcurrentTasks          | `uint`        | `2`                | No. of concurrent tasks to process     |
| ConcurrentImageTasks     | `uint`        | `2`                | No. of images to download in parallel  |
| PreferJapanese           | `boolean`     | `false`            | Use Japanese Titles instead of English |

__NOTE: This program will decline any changes to configuration if the values are invalid.__
