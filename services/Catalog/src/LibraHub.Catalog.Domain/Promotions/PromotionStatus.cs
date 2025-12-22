namespace LibraHub.Catalog.Domain.Promotions;

public enum PromotionStatus
{
    Draft = 0,
    Scheduled = 1,
    Active = 2,
    Paused = 3,
    Ended = 4,
    Cancelled = 5
}
