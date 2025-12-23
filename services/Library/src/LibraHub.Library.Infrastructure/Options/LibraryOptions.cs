namespace LibraHub.Library.Infrastructure.Options;

public class LibraryOptions
{
    public const string SectionName = "Library";

    public string ConnectionString { get; set; } = string.Empty;
    public RabbitMqOptions RabbitMq { get; set; } = new();
}

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "librahub.events";
}

