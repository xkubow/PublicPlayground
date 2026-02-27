using System.Net;
using JK.Platform.Core.Validations;

namespace JK.Platform.Core.Exceptions;

public abstract class StatusExceptionBase<TException> : Exception where TException : StatusExceptionBase<TException>
{
   public string MemberName { get; }
   public string SourceFilePath { get; }
   public int SourceLineNumber { get; }
   public HttpStatusCode StatusCode { get; }
   public string ErrorCode { get; }
   public Dictionary<string, string>? ContextData { get; private set; }

   public StatusExceptionBase(
       string? errorCode,
       string? message,
       HttpStatusCode statusCode,
       string memberName,
       string sourceFilePath,
       int sourceLineNumber,
       Exception? innerException) : base(message, innerException)

   {
      ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? $"{memberName}Failed" : errorCode;
      StatusCode = statusCode;
      MemberName = memberName;
      SourceFilePath = sourceFilePath;
      SourceLineNumber = sourceLineNumber;
   }

   public TException AddContextDataItem(string key, string value)
   {
      Guard.NotNullAndNotEmpty(key, nameof(key));
      Guard.NotNullAndNotEmpty(value, nameof(value));
      ContextData ??= [];
      var index = 0;
      var result = ContextData.TryAdd(key, value);
      while (!result)
         result = ContextData.TryAdd($"{key}_{++index}", value);
      return (TException)this;
   }

   public TException AddContextDataItems(Dictionary<string, string>? contextData)
   {
      if (contextData is null || contextData.Count == 0)
         return (TException)this;

      if (ContextData is null || ContextData.Count == 0)
         ContextData = contextData;
      else
      {
         foreach (var item in contextData)
            AddContextDataItem(item.Key, item.Value);
      }
      return (TException)this;
   }

   public TException AddContextData((string Key, string Value) item) => AddContextDataItem(item.Key, item.Value);

   public TException AddContextDataItems(params (string Key, string Value)[] items)
   {
      foreach (var item in items)
         AddContextDataItem(item.Key, item.Value);
      return (TException)this;
   }
}