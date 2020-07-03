using System.Collections.Generic;
using Newtonsoft.Json;

namespace asuka.Model
{
  public class RecommendationsResponse
  {
    [JsonProperty("result")]
    public List<Response> Result { get; set; }
  }
}
