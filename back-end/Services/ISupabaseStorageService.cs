namespace VbMerchant.Services;

public interface ISupabaseStorageService
{
    long MaxFileSizeBytes { get; }

    Task UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<string?> CreateSignedUrlAsync(
        string objectPath,
        CancellationToken cancellationToken = default);
}
