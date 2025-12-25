using LibraHub.BuildingBlocks.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LibraHub.Gateway.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly ServiceClientHelper _serviceClient;
    private readonly ServicesOptions _servicesOptions;

    public DashboardController(
        ServiceClientHelper serviceClient,
        IOptions<ServicesOptions> servicesOptions)
    {
        _serviceClient = serviceClient;
        _servicesOptions = servicesOptions.Value;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var token = GetAuthorizationToken();

        var (usersTask, booksTask, ordersTask, entitlementsTask) = FetchAllStatisticsAsync(token, cancellationToken);
        await Task.WhenAll(usersTask, booksTask, ordersTask, entitlementsTask);

        var summary = BuildDashboardSummary(usersTask, booksTask, ordersTask, entitlementsTask);
        return Ok(summary);
    }

    private string GetAuthorizationToken()
    {
        return Request.Headers.Authorization.ToString();
    }

    private (Task<UserStatisticsDto?> Users, Task<BookStatisticsDto?> Books, Task<OrderStatisticsDto?> Orders, Task<EntitlementStatisticsDto?> Entitlements)
        FetchAllStatisticsAsync(string token, CancellationToken cancellationToken)
    {
        return (
            _serviceClient.GetAsync<UserStatisticsDto>(_servicesOptions.Identity, "admin/statistics/users", token, cancellationToken),
            _serviceClient.GetAsync<BookStatisticsDto>(_servicesOptions.Catalog, "admin/statistics/books", token, cancellationToken),
            _serviceClient.GetAsync<OrderStatisticsDto>(_servicesOptions.Orders, "admin/statistics/orders", token, cancellationToken),
            _serviceClient.GetAsync<EntitlementStatisticsDto>(_servicesOptions.Library, "api/admin/statistics/entitlements", token, cancellationToken)
        );
    }

    private DashboardSummaryDto BuildDashboardSummary(
        Task<UserStatisticsDto?> usersTask,
        Task<BookStatisticsDto?> booksTask,
        Task<OrderStatisticsDto?> ordersTask,
        Task<EntitlementStatisticsDto?> entitlementsTask)
    {
        var orderStats = ordersTask.Result;

        return new DashboardSummaryDto
        {
            Users = usersTask.Result,
            Books = booksTask.Result,
            Orders = orderStats,
            Entitlements = entitlementsTask.Result,
            Revenue = BuildRevenueDto(orderStats)
        };
    }

    private RevenueDto BuildRevenueDto(OrderStatisticsDto? orderStats)
    {
        return new RevenueDto
        {
            Total = orderStats?.TotalRevenue ?? 0,
            Last30Days = orderStats?.Last30Days?.Revenue ?? 0,
            Last7Days = orderStats?.Last7Days?.Revenue ?? 0,
            Currency = orderStats?.Currency ?? "USD"
        };
    }
}

public class ServicesOptions
{
    public string Identity { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
    public string Orders { get; set; } = string.Empty;
    public string Library { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public UserStatisticsDto? Users { get; init; }
    public BookStatisticsDto? Books { get; init; }
    public OrderStatisticsDto? Orders { get; init; }
    public EntitlementStatisticsDto? Entitlements { get; init; }
    public RevenueDto Revenue { get; init; } = new();
}

public class RevenueDto
{
    public decimal Total { get; init; }
    public decimal Last30Days { get; init; }
    public decimal Last7Days { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public class UserStatisticsDto
{
    public int Total { get; init; }
    public int Active { get; init; }
    public int Disabled { get; init; }
    public int Pending { get; init; }
    public int NewLast30Days { get; init; }
    public int NewLast7Days { get; init; }
}

public class BookStatisticsDto
{
    public int Total { get; init; }
    public int Published { get; init; }
    public int Draft { get; init; }
    public int Unlisted { get; init; }
    public int NewLast30Days { get; init; }
}

public class OrderStatisticsDto
{
    public int Total { get; init; }
    public int Paid { get; init; }
    public int Pending { get; init; }
    public int Cancelled { get; init; }
    public int Refunded { get; init; }
    public PeriodStatisticsDto Last30Days { get; init; } = new();
    public PeriodStatisticsDto Last7Days { get; init; } = new();
    public decimal TotalRevenue { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public class PeriodStatisticsDto
{
    public int Count { get; init; }
    public decimal Revenue { get; init; }
}

public class EntitlementStatisticsDto
{
    public int Total { get; init; }
    public int Active { get; init; }
    public int Revoked { get; init; }
    public int GrantedLast30Days { get; init; }
}

