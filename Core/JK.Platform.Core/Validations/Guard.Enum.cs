using JK.Platform.Core.Exceptions;

namespace JK.Platform.Core.Validations;

public static partial class Guard
{
   public static void EnumIsDefined<T>(T value, string name, string? message = null) where T : Enum
   {
      if (!Enum.IsDefined(typeof(T), value))
         throw new ValidationStatusException("ValidationError").AddValidationError(name, "InvalidEnumValue", message ?? $"Value for '{name}' must be a valid enum value. Current value is '{value}'.");
   }
}