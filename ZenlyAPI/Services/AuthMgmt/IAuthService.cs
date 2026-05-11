using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.AuthMgmt;

public interface IAuthService
{
    Task<ServiceResponse<LoginResponse>> StudentLoginAsync(StudentLoginRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> StudentSignupAsync(StudentSignupRequest request, CancellationToken cancellationToken);
     Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
     Task<ServiceResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
