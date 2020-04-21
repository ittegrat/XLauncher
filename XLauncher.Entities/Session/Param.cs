using System;

namespace XLauncher.Entities.Session
{

  using Common;

  public partial class Param : IValidable
  {

    public TypeCode TypeCode => (TypeCode)Enum.Parse(typeof(TypeCode), Type.ToString());

    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Param attribute 'name' must be non-empty.");
      Name = Name.Trim();

      if (String.IsNullOrWhiteSpace(Value))
        throw new ValidationException("Param attribute 'value' must be non-empty.");
      Value = Value.Trim();

    }

  }

}
