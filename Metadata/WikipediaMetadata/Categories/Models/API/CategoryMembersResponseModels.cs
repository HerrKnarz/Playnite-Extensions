namespace WikipediaMetadata.Categories.Models.API;

public class CategoryMemberQueryResult
{
    public CategoryMember[] CategoryMembers { get; set; }
}

public class CategoryMember
{
    public int Ns { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
}

