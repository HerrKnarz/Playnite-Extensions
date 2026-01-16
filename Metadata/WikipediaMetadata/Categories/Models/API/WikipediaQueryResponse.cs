using System.Collections.Generic;

namespace WikipediaCategories.Models.API;

public class WikipediaQueryResponse<TQuery>
{
    public Dictionary<string, string> Continue { get; set; }
    public TQuery Query { get; set; }
}
