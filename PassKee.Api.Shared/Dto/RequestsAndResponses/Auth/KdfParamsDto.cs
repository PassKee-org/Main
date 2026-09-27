namespace PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

public class KdfParamsDto
{
    public int Iterations { get; set; }
    public int MemorySize { get; set; }
    public int Parallelism { get; set; }
}
