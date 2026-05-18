using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Domain.Entities.Shared;
using ZenlyAPI.Domain.User;
using ZenlyAPI.Domain.User.Lecturers;
using ZenlyAPI.Domain.User.Students;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Extensions;
using ZenlyAPI.Services.Shared.Security;
using ZenlyAPI.Services.Shared.UserContextService;
using Response = ZenlyAPI.Services.Shared.Response;

namespace ZenlyAPI.Services.AuthMgmt;

public class AuthService(ZenlyDbContext database, ZenlyConfig zenlyConfig, IUserContextService userContextService) : IAuthService
{
    private readonly int RefreshTokenValidity = 7;
    public async Task<ServiceResponse<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            string email = request.Email.Trim();
            string password = request.Password.Trim();

            BaseUser? user = null;

            if (request.UserType == UserType.Student)
            {
                user = await database.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Email == email && x.Type == UserType.Student,
                        cancellationToken);
            }
            else if (request.UserType == UserType.Lecturer)
            {
                user = await database.Lecturers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Email == email && x.Type == UserType.Lecturer,
                        cancellationToken);
            }

            if (user is null)
            {
                return Response.Unauthorized<LoginResponse>(
                    "Invalid login credentials",
                    null!);
            }

            bool isCorrectPassword = BCryptHash.Verify(user.Password!, password);

            if (!isCorrectPassword)
            {
                return Response.BadRequest<LoginResponse>(
                    "Incorrect email address or password",
                    null!);
            }

            LoginResponse result = await CreateAccessTokenAsync(
                user,
                true,
                cancellationToken);

            return Response.Success("Login successful.", result);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);

            return Response.SystemMalfunction<LoginResponse>(
                "Login failed. It is not you, it's us. Please try again or contact support.",
                null!);
        }
    }

    public async Task<ServiceResponse> StudentSignupAsync(StudentSignupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if first and last name pairing exists
            bool nameExists = false;
            if (request.UserType == UserType.Student)
            {
                nameExists = await database.Students.AsNoTracking().AnyAsync(s => s.FirstName.ToLower() == request.FirstName.ToLower() && s.LastName.ToLower() == request.LastName.ToLower(), cancellationToken);
            }

            if (nameExists)
            {
                return Response.Conflict("A student with this first and last name already exists");
            }

            bool emailExists = false;
            if (request.UserType == UserType.Student)
            {
                emailExists = await database.Students.AsNoTracking().AnyAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            }

            if (emailExists)
            {
                return Response.Conflict("A student with this email already exists");
            }

            bool matricNoExists = await database.Students.AsNoTracking().AnyAsync(s => s.MatricNo == request.MatricNo, cancellationToken);

            if (matricNoExists)
            {
                return Response.Conflict("A student with this matric number already exists");
            }

            bool facultyExists = await database.Faculties.AsNoTracking().AnyAsync(f => f.Id == request.FacultyId, cancellationToken);
            if (!facultyExists)
            {
                return Response.NotFound("The specified faculty does not exist");
            }

            bool departmentExists = await database.Departments.AsNoTracking().AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
            if (!departmentExists)
            {
                return Response.NotFound("The specified department does not exist");
            }

            Student student = new()
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username,
                Type = UserType.Student,
                Email = request.Email,
                MatricNo = request.MatricNo,
                FacultyId = request.FacultyId,
                DepartmentId = request.DepartmentId,
                Password = BCryptHash.Hash(request.Password)
            };

            await database.Students.AddAsync(student, cancellationToken);

            // Adding courses
            var studentCourses = request.Courses.Select(course => new StudentCourse
            {
                StudentId = student.Id,
                CourseId = course
            }).ToList();

            // TODO - Email notification to student about successful registration
            database.Students_Courses.AddRange(studentCourses);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Created("Student registered successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);
            return Response.SystemMalfunction("Login failed. It is not you, it's us. Please try again or contact support.");
        }
    }

    public async Task<ServiceResponse> LecturerSignupAsync(LecturerSignupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            bool nameExists = false;
            if (request.UserType == UserType.Lecturer)
            {
                nameExists = await database.Lecturers.AsNoTracking().AnyAsync(l => l.FirstName.ToLower() == request.FirstName.ToLower() && l.LastName.ToLower() == request.LastName.ToLower(), cancellationToken);
            }

            if (nameExists)
            {
                return Response.Conflict("A lecturer with this first and last name already exists");
            }

            bool emailExists = false;
            if (request.UserType == UserType.Lecturer)
            {
                nameExists = await database.Lecturers.AsNoTracking().AnyAsync(l => l.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            }

            if (emailExists)
            {
                return Response.Conflict("A lecturer with this email already exists");
            }

            bool facultyExists = await database.Faculties.AsNoTracking().AnyAsync(f => f.Id == request.FacultyId, cancellationToken);
            if (!facultyExists)
            {
                return Response.NotFound("The specified faculty does not exist");
            }

            bool departmentExists = await database.Departments.AsNoTracking().AnyAsync(d => d.Id == request.DepartmentId, cancellationToken);
            if (!departmentExists)
            {
                return Response.NotFound("The specified department does not exist");
            }


            Lecturer lecturer = new()
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Username,
                Email = request.Email,
                Type = UserType.Lecturer,
                FacultyId = request.FacultyId,
                DepartmentId = request.DepartmentId,
                Password = BCryptHash.Hash(request.Password)
            };

            await database.Lecturers.AddAsync(lecturer, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            return Response.Created("Lecturer registered successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);
            return Response.SystemMalfunction("Login failed. It is not you, it's us. Please try again or contact support.");
        }
    }

    Task<ServiceResponse<LoginResponse>> IAuthService.RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected async Task<LoginResponse> CreateAccessTokenAsync(BaseUser? user, bool isLogin, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            throw new AuthenticationException("Invalid login credentials");
        }

        JwtSecurityTokenHandler jwTokenHandler = new();
        string? secretKey = zenlyConfig.JwtConfig.JwtKey;
        byte[] key = Encoding.ASCII.GetBytes(secretKey);
        UserType userType = user.Type;


        Claim[] claims = [
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("id", user.Id.ToString()),
                new Claim("userType",userType.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                //new Claim(ClaimTypes.Role, roles),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
    JwtRegisteredClaimNames.Iat,
    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
    ClaimValueTypes.Integer64
)
        ];

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims = [..claims,
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                ];

        }


        //if (user is CorporateUser || user is BackOfficeUser)
        //{
        //    string[] privileges = await GetUserRolePrivilegesAsync(roles, user is CorporateUser);
        //    foreach (string privilege in privileges)
        //    {
        //        claims = [.. claims, new Claim("permission", privilege)];
        //    }
        //}

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(zenlyConfig.JwtConfig.JwtExpireMinutes),
            Audience = zenlyConfig.JwtConfig.JwtAudience,
            Issuer = zenlyConfig.JwtConfig.JwtIssuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        SecurityToken securityToken = jwTokenHandler.CreateToken(tokenDescriptor);
        string accessToken = jwTokenHandler.WriteToken(securityToken);
        string refreshToken = await GenerateRefreshTokenAsync(user.Id, user.Type);
        

        user.LastLogin = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken,
            refreshToken,
            tokenDescriptor.Expires.Value,
            user.FirstName,
            user.LastName, 
            userType.ToString()
        );
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId, UserType userType)
    {
        byte[] randomNumber = new byte[32];

        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomNumber);
            string refreshToken = Convert.ToBase64String(randomNumber);

            IQueryable<RefreshToken> query = database.RefreshTokens
                .Where(x => x.IsActive);

            // 🔥 Filter by correct user type
            if (userType == UserType.Student)
            {
                query = query.Where(x => x.StudentId == userId);
            }
            else if (userType == UserType.Lecturer)
            {
                query = query.Where(x => x.LecturerId == userId);
            }

            List<RefreshToken> refreshTokens = await query.ToListAsync();

            foreach (var token in refreshTokens)
            {
                token.IsActive = false;
                token.UpdatedAt = DateTime.UtcNow;
            }

            // 🔥 Create new token with correct FK
            RefreshToken newRefreshToken = new()
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenValidity),
                IsActive = true,
                Token = refreshToken
            };

            if (userType == UserType.Student)
            {
                newRefreshToken.StudentId = userId;
            }
            else if (userType == UserType.Lecturer)
            {
                newRefreshToken.LecturerId = userId;
            }

            await database.RefreshTokens.AddAsync(newRefreshToken);

            return refreshToken;
        }
    }

    public async Task<ServiceResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        BaseUser? userDetails = null;
        if (request.UserType == UserType.Student)
        {
            userDetails = await database.Students.FirstOrDefaultAsync(x => x.Id.ToString() == userContextService.User.Id && x.Type == UserType.Student, cancellationToken);
        }
        else if (request.UserType == UserType.Lecturer)
        {
            userDetails = await database.Lecturers.FirstOrDefaultAsync(x => x.Id.ToString() == userContextService.User.Id && x.Type == UserType.Lecturer, cancellationToken);
        }

            if (userDetails is null)
            {
                return Response.NotFound("User does not exist");
            }



        bool isValidPin = request.OldPassword.Verify(userDetails.Password!);
        if (!isValidPin)
        {
            return Response.BadRequest("Your current password does not match.");
        }

        bool isNotNewPassword = request.NewPassword.Verify(userDetails.Password);
        if (isNotNewPassword)
        {
            return Response.BadRequest("You cannot use your old password.");
        }

        userDetails.ModifiedAt = DateTimeOffset.UtcNow;
        userDetails.ModifiedBy = userContextService.User.Id;
        userDetails.Password = request.NewPassword.Hash();

        await database.SaveChangesAsync(cancellationToken);
        return Response.Success("Password updated successfully");
    }
}
