namespace LibraHub.Orders.Infrastructure.Options;

public class OrdersOptions
{
    public const string SectionName = "Orders";

    public string CatalogApiUrl { get; set; } = string.Empty;
    public string LibraryApiUrl { get; set; } = string.Empty;
    public string IdentityApiUrl { get; set; } = string.Empty;
}

