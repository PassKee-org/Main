namespace PassKee.Business.Common.Services.Web.ReCaptcha;

public class FakeReCaptchaService: IReCaptchaService
{

    public Task<bool> ValidateAsync(string token) => Task.FromResult(true);
    public bool HasSecretKey()
    {
        return true;
    }
}
