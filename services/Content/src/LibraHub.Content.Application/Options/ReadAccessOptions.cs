namespace LibraHub.Content.Application.Options;

public class ReadAccessOptions
{
    public const string SectionName = "ReadAccess";

    public string CatalogApiUrl { get; set; } = string.Empty;
    public string LibraryApiUrl { get; set; } = string.Empty;
    public int TokenExpirationMinutes { get; set; } = 60;
}

