using System.Net;
using System.Runtime.CompilerServices;
using JK.Platform.Core.Validations;

namespace JK.Platform.Core.Exceptions;

public class ValidationStatusException : StatusExceptionBase<ValidationStatusException>
{
   public Dictionary<string, ValidationErrorDetail>? ValidationErrors { get; private set; }

   public ValidationStatusException(
       string errorCode = "ValidationError",
       string? message = null,
       [CallerMemberName] string memberName = "",
       [CallerFilePath] string sourceFilePath = "",
       [CallerLineNumber] int sourceLineNumber = 0) : base(errorCode, message ?? "Validation error", HttpStatusCode.BadRequest, memberName, sourceFilePath, sourceLineNumber, null)

   {
   }

   public ValidationStatusException AddValidationError(string field, string? errorCode, string? errorMessage)
   {
      Guard.NotNullAndNotEmpty(field, nameof(field));
      ValidationErrors ??= [];
      ValidationErrors.Add(field, new ValidationErrorDetail { ErrorCode = errorCode, ErrorMessage = errorMessage });
      return this;
   }

   public ValidationStatusException AddValidationError((string Field, string? ErrorCode, string? ErrorMessage) item) => AddValidationError(item.Field, item.ErrorCode, item.ErrorMessage);

   public ValidationStatusException AddValidationErrors(Dictionary<string, ValidationErrorDetail>? validationErrors)
   {
      if (validationErrors is null || validationErrors.Count == 0)
         return this;

      if (ValidationErrors is null || ValidationErrors.Count == 0)
         ValidationErrors = validationErrors;
      else
      {
         foreach (var item in validationErrors)
            AddValidationError(item.Key, item.Value.ErrorCode, item.Value.ErrorMessage);
      }
      return this;
   }

   public ValidationStatusException AddValidationErrors(params (string Field, string? ErrorCode, string? ErrorMessage)[] items)
   {
      foreach (var item in items)
         AddValidationError(item.Field, item.ErrorCode, item.ErrorMessage);
      return this;
   }
}