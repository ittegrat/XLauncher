using System;

namespace XLauncher.Entities.Authorization
{

  using Common;
  using XLauncher.Common;

  public partial class Machine : IValidable
  {

    public void Validate() {
      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Machine attribute 'name' must be non-empty.");
      Name = Name.Standardize();
    }

  }
}
