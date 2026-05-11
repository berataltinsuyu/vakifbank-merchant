namespace VbMerchant.Services;

public sealed class SupabaseStorageOptions
{
    public const string SectionName = "Supabase";

    public string? Url { get; set; }
    public string? ServiceRoleKey { get; set; }
    public string? StorageBucket { get; set; }
    public int SignedUrlExpiresSeconds { get; set; } = 300;
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
}
