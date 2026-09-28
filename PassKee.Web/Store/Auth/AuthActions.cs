using PassKee.Api.Shared.Dto.RequestsAndResponses.Auth;

namespace PassKee.Web.Store.Auth;

public record struct RegisterAction(string Email, string Password);

public record struct RegisterSuccessAction(AuthResponse Response, string SecretKeyBase64, byte[] UserPrivateKey, byte[] UserPublicKey);

public record struct RegisterFailureAction(string ErrorMessage);

public record struct LoginAction(string Email, string Password, string SecretKeyBase64);

public record struct LoginSuccessAction(AuthResponse Response, byte[]? UserPrivateKey, byte[]? UserPublicKey);

public record struct LoginFailureAction(string ErrorMessage);

public record struct ResetAuthStateAction();

public record struct UnlockAction(string Password);
