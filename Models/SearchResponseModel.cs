using System.Collections.Generic;
using Newtonsoft.Json;

namespace asukav2.Models
{
  public class SearchResponseModel
  {
    [JsonProperty("result")]
    public List<ResponseModel> Result { get; set; }

    [JsonProperty("num_pages")]
    public uint TotalPages { get; set; }

    [JsonProperty("per_page")]
    public uint ItemsPerPage { get; set; }
  }
}
