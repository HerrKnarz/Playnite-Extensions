using System.Collections.Generic;

namespace WikipediaMetadata.Categories.Models.API;

public class WikipediaQueryResponse<TQuery>
{
    public Dictionary<string, string> Continue { get; set; }
    public TQuery Query { get; set; }
}
