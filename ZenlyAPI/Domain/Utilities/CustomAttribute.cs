using System.ComponentModel.DataAnnotations;

namespace ZenlyAPI.Domain.Utilities;

public class StartsWithAttribute : ValidationAttribute
{
    private readonly string _prefix;

    public StartsWithAttribute(string prefix)
    {
        _prefix = prefix;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult($"{validationContext.DisplayName} is required.");
        }

        var stringValue = value.ToString();

        if (!stringValue.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult(
                $"{validationContext.DisplayName} must start with '{_prefix}'."
            );
        }

        return ValidationResult.Success;
    }
}