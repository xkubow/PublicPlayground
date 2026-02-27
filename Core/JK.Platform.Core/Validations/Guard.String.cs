using JK.Platform.Core.Exceptions;

namespace JK.Platform.Core.Validations;

public static partial class Guard
{
    public static void NotNullAndNotEmpty(string? value, string name, string? message = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ValidationStatusException("InvalidEmptyValue").AddValidationError(name, "InvalidEmptyValue", message ?? $"Value for '{name}' cannot be empty or null.");
    }
}