using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.Validators.Attributes;
using static ZenlyAPI.Domain.Utilities.StartsWithAttribute;

namespace ZenlyAPI.Services.AuthMgmt;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiryTimeStamp, string FirstName, string LastName, string UserType);


public class LoginRequest
{
    [Required]
    public UserType UserType { get; set; }

    [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}

public class StudentSignupRequest
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [MatricNo]
    public string MatricNo { get; set; }

    [Required]
    public Guid FacultyId { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    public List<Guid> Courses { get; set; }

    [Required]
    public UserType UserType { get; set; }

    [PasswordValidation]
    public string Password { get; set; }
}

public class LecturerSignupRequest
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public Guid FacultyId { get; set; }
    [Required]
    public Guid DepartmentId { get; set; }
    [Required]
    public UserType UserType { get; set; }
    [PasswordValidation]
    public string Password { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public UserType UserType { get; set; }

    [Required]
    public string? OldPassword { get; set; }

    [Required]
    [PasswordValidation]
    public string? NewPassword { get; set; }

    [Required]
    [PasswordValidation]
    [Compare("NewPassword")]
    public string? ConfirmPassword { get; set; }
}