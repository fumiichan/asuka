using System.Runtime.Serialization;

namespace asuka.Api.Queries
{
    public enum SearchSortOptions
    {
        [EnumMember(Value = "popular-week")]
        PopularWeek,
        
        [EnumMember(Value = "popular-today")]
        PopularToday,
        
        [EnumMember(Value = "popular")]
        Popular,
        
        [EnumMember(Value = "date")]
        Recent
    }
}