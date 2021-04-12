using System;
using System.Collections.Generic;

namespace XLauncher.Entities.Session
{

  using Common;

  public partial class Context : IValidable
  {

    public void FixEmpty() {
      if (Addins.Length + Params.Length == 0)
        Params = new Param[] { new Param { Name = "nullKey", Value = "nullValue", Type = ParamType.String } };
    }
    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Context attribute 'name' must be non-empty.");
      Name = Name.Trim();

      Version = Version?.Trim();

      if (Addins == null)
        Addins = Array.Empty<Addin>();

      if (Params == null)
        Params = Array.Empty<Param>();

      if (Addins.Length + Params.Length == 0)
        throw new ValidationException($"Invalid Context '{Name}'. At least one of 'addin' or 'param' is required.");

      try {

        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ai in Addins) {
          ai.Validate();
          if (!hs.Add(ai.FileName))
            throw new ValidationException($"Addin '{ai.FileName}' is duplicated.");
        }

        hs.Clear();
        foreach (var p in Params) {
          p.Validate();
          if (!hs.Add(p.Name))
            throw new ValidationException($"Parameter '{p.Name}' is duplicated.");
        }

      }
      catch (ValidationException vex) {
        vex.Parents.Add($"Context => {Name}");
        throw;
      }


    }

  }
}
