namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class KdfParamsRequest
{
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
    public int Parallelism { get; set; }
}
