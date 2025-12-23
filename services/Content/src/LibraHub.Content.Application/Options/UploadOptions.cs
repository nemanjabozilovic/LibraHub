namespace LibraHub.Content.Application.Options;

public class UploadOptions
{
    public const string SectionName = "Upload";

    public long MaxCoverSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public long MaxEditionSizeBytes { get; set; } = 100 * 1024 * 1024; // 100 MB
    public string CoversBucketName { get; set; } = "covers";
    public string EditionsBucketName { get; set; } = "editions";
}

