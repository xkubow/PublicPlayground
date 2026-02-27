using JK.Platform.Core.Exceptions;

namespace JK.Platform.Core.Validations;

public static partial class Guard
{
   /// <summary>
   /// Ensure that the value is not null. If null, an exception is thrown.
   /// </summary>
   /// <exception cref="ArgumentNullException"></exception>
   public static void NotNull(object? value, string name, string? message = null)
   {
      if (value == null)
         throw new ValidationStatusException("ValidationError").AddValidationError(name, "InvalidNotNullValue", message ?? $"Value for '{name}' cannot be 'null'.");
   }
}