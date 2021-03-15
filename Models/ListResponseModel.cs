using System.Collections.Generic;
using Newtonsoft.Json;

namespace asukav2.Models
{
  public class ListResponseModel
  {
    [JsonProperty("result")]
    public List<ResponseModel> Result { get; set; }
  }
}
