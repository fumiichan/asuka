using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Model
{
  public class SearchResponse
  {
    [JsonProperty("result")]
    public List<Response> Result { get; set; }

    [JsonProperty("num_pages")]
    public int TotalPages { get; set; }

    [JsonProperty("per_page")]
    public int ItemsPerPage { get; set; }
  }
}
