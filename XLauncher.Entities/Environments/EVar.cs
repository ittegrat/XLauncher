using System;

namespace XLauncher.Entities.Environments
{

  using Common;

  public partial class EVar : IValidable
  {

    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("EVar attribute 'name' must be non-empty.");
      Name = Name.Trim();

      if (String.IsNullOrWhiteSpace(Value))
        throw new ValidationException("EVar attribute 'value' must be non-empty.");
      Value = Value.Trim();

    }

  }
}
