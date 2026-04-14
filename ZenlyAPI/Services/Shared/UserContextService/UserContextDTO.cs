namespace ZenlyAPI.Services.Shared.UserContextService;

public record CurrentUser(string SecondaryIssuerId, string Id, string Email, string RoleName, string RoleId, string Username, string FirstName, string LastName);
