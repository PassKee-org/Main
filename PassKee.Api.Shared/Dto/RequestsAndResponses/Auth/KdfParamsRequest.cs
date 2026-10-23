using System.ComponentModel.DataAnnotations;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class KdfParamsRequest
{
    public const int MinIterations = 1;
    public const int MaxIterations = 10;
    public const int MinMemorySizeKb = 8192; // 8 MB
    public const int MaxMemorySizeKb = 262144; // 256 MB
    public const int MinParallelism = 1;
    public const int MaxParallelism = 8;

    [Range(MinIterations, MaxIterations, ErrorMessage = "KDF Iterations must be between 1 and 10.")]
    public int Iterations { get; set; }

    [Range(MinMemorySizeKb, MaxMemorySizeKb, ErrorMessage = "KDF MemorySize must be between 8192 KB and 262144 KB.")]
    public int MemorySize { get; set; }

    [Range(MinParallelism, MaxParallelism, ErrorMessage = "KDF Parallelism must be between 1 and 8.")]
    public int Parallelism { get; set; }
}
