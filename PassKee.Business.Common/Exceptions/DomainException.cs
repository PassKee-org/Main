using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Common.Exceptions.Api.Auth;
using PassKee.Business.Common.Exceptions.Common;
using ValidationException = PassKee.Business.Common.Exceptions.Common.ValidationException;

namespace PassKee.Business.Common.Exceptions;

public static class DomainException
{
    public static DataPermissionException DataPermissionException { get; } = new();
    
    public static ItemNotFoundException ItemNotFoundException { get; } = new();
    
    public static PermissionException PermissionException { get; } = new();
    
    public static TooManyRequestsException TooManyRequestsException { get; } = new();

    public static ValidationException ValidationException { get; } = new();
    
    public static RecordIsExistsException RecordIsExistsException { get; } = new();
    
    public static RecordNotFoundException RecordNotFoundException { get; } = new();
    
    public static ServerException ServerException { get; } = new();
    
    public static TooManyRecordsException TooManyRecordsException { get; } = new();
    
    public static UserNotAuthorizedException UserNotAuthorizedException { get; } = new();
    
    public static IncorrectFileException IncorrectFileException { get; } = new();
    
    public static UserNotFoundException UserNotFoundException { get; } = new();
}
