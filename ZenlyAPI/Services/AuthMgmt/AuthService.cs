using Castle.Core.Resource;
using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
using ZenlyAPI.Domain.User.Students;
using ZenlyAPI.Domain.Utilities;
using ZenlyAPI.Services.Shared.Security;
using Response = ZenlyAPI.Services.Shared.Response;

namespace ZenlyAPI.Services.AuthMgmt;

public class AuthService(ZenlyDbContext database, ZenlyConfig zenlyConfig) : IAuthService
{
    private readonly int RefreshTokenValidity = 7;
    public async Task<ServiceResponse<LoginResponse>> StudentLoginAsync(StudentLoginRequest request, CancellationToken cancellationToken)
    {
        try
        {

            string email = request.Email.Trim();
            string password = request.Password.Trim();

            Student? student = await database.Students.AsNoTracking().FirstOrDefaultAsync(b => b.Email == email, cancellationToken);

            if (student is null)
            {
                return Response.Unauthorized<LoginResponse>("Invalid login credentials", null!);
            }

            bool isCorrectPassword = BCryptHash.Verify(student?.Password!, password);
            if (!isCorrectPassword)
            {
                return Response.BadRequest<LoginResponse>("Incorrect email address or password", null!);
            }

            //string[] userRoles = student.Roles.Select(x => x.Role.Name).ToArray();
            //string roles = string.Join(",", userRoles);

            LoginResponse result = await CreateAccessTokenAsync(student, true, cancellationToken);

            return Response.Success("Login successful.", result);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);
            return Response.SystemMalfunction<LoginResponse>("Login failed. It is not you, it's us. Please try again or contact support.", null!);
        }
    }
    public async Task<ServiceResponse> StudentSignupAsync(StudentSignupRequest request, CancellationToken cancellationToken)
    {
        // Check if first and last name pairing exists
        bool nameExists = false;
        if (request.UserType == Domain.Entities.Shared.UserType.Student)
        {
            nameExists = await database.Students.AsNoTracking().AnyAsync(s => s.FirstName.ToLower() == request.FirstName.ToLower() && s.LastName.ToLower() == request.LastName.ToLower(), cancellationToken);
        } 
        //else if (request.UserType == Domain.Entities.Shared.UserType.Lecturer)
        //{
        //    nameExists = await database.Lecturers.AsNoTracking().AnyAsync(l => l.FirstName.ToLower() == request.FirstName.ToLower() && l.LastName.ToLower() == request.LastName.ToLower(), cancellationToken);
        //}

        if (nameExists)
        {
            return Response.Conflict("A user with this first and last name already exists");
        }

        bool emailExists = false;
        if (request.UserType == Domain.Entities.Shared.UserType.Student)
        {
            emailExists = await database.Students.AsNoTracking().AnyAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        }
        //else if (request.UserType == Domain.Entities.Shared.UserType.Lecturer)
        //{
        //    nameExists = await database.Lecturers.AsNoTracking().AnyAsync(l => l.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        //}

        if (emailExists)
        {
            return Response.Conflict("A user with this email already exists");
        }
      
            bool matricNoExists = await database.Students.AsNoTracking().AnyAsync(s => s.MatricNo == request.MatricNo, cancellationToken);

            if (matricNoExists)
            {
                return Response.Conflict("A user with this matric number already exists");
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


        database.Students_Courses.AddRange(studentCourses);
        await database.SaveChangesAsync(cancellationToken);

        return Response.Success("Student registered successfully");
    }

    Task<ServiceResponse> IAuthService.LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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



}
