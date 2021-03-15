using System.Collections.Generic;
using Newtonsoft.Json;

namespace asukav2.Models
{
  public class ResponseModel
  {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("media_id")]
    public int MediaId { get; set; }

    [JsonProperty("title")]
    public Title Title { get; set; }

    [JsonProperty("images")]
    public Images Images { get; set; }

    [JsonProperty("tags")]
    public List<Tag> Tags { get; set; }

    [JsonProperty("num_pages")]
    public int TotalPages { get; set; }

    [JsonProperty("num_favorites")]
    public int FavoritesCount { get; set; }
  }

  public class Title
  {
    [JsonProperty("english")]
    public string English { get; set; }

    [JsonProperty("japanese")]
    public string Japanese { get; set; }

    [JsonProperty("pretty")]
    public string Pretty { get; set; }
  }

  public class Images
  {
    [JsonProperty("pages")]
    public List<Pages> Pages { get; set; }

    [JsonProperty("thumbnail")]
    public Pages Thumbnail { get; set; }

    [JsonProperty("cover")]
    public Pages Cover { get; set; }
  }

  public class Pages
  {
    [JsonProperty("t")]
    public string Type { get; set; }

    [JsonProperty("h")]
    public int Height { get; set; }

    [JsonProperty("w")]
    public int Width { get; set; }
  }

  public class Tag
  {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }
  }
}