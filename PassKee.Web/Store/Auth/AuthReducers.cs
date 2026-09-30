using Fluxor;

namespace PassKee.Web.Store.Auth;

public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState OnRegister(AuthState state, RegisterAction action) =>
        state with
        {
            IsLoading = true,
            ErrorMessage = null,
            ShowSecretKey = false
        };

    [ReducerMethod]
    public static AuthState OnRegisterSuccess(AuthState state, RegisterSuccessAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = null,
            ShowSecretKey = true,
            SecretKey = action.SecretKey,
            IsAuthenticated = true,
            CurrentUser = action.Response,
            UserPrivateKey = action.UserPrivateKey,
            UserPublicKey = action.UserPublicKey
        };

    [ReducerMethod]
    public static AuthState OnRegisterFailure(AuthState state, RegisterFailureAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            ShowSecretKey = false
        };

    [ReducerMethod]
    public static AuthState OnLogin(AuthState state, LoginAction action) =>
        state with
        {
            IsLoading = true,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static AuthState OnUnlock(AuthState state, UnlockAction action) =>
        state with
        {
            IsLoading = true,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static AuthState OnLoginSuccess(AuthState state, LoginSuccessAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = null,
            IsAuthenticated = true,
            CurrentUser = action.Response,
            UserPrivateKey = action.UserPrivateKey,
            UserPublicKey = action.UserPublicKey
        };

    [ReducerMethod]
    public static AuthState OnLoginFailure(AuthState state, LoginFailureAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage,
            IsAuthenticated = false,
            UserPrivateKey = null,
            UserPublicKey = null
        };

    [ReducerMethod]
    public static AuthState OnResetAuthState(AuthState state, ResetAuthStateAction action) =>
        new();
}
