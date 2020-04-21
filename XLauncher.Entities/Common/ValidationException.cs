using System;
using System.Collections.Specialized;

namespace XLauncher.Entities.Common
{
  public class ValidationException : Exception
  {

    public StringCollection Parents { get; } = new StringCollection();

    public ValidationException()
      : base("A validation error occurred.") {
    }
    public ValidationException(string message)
      : base(message) {
    }
    public ValidationException(string message, Exception inner)
      : base(message, inner) {
    }

  }
}
