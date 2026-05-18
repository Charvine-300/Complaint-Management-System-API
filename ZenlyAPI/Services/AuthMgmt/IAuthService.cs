using ZenlyAPI.Domain.Utilities;

namespace ZenlyAPI.Services.AuthMgmt;

public interface IAuthService
{
    Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> StudentSignupAsync(StudentSignupRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse> LecturerSignupAsync(LecturerSignupRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<ServiceResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
    //Task<ServiceResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
