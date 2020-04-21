using System;
using System.Collections.Generic;
using System.Linq;

namespace XLauncher.Entities.Authorization
{

  using Common;
  using XLauncher.Common;

  public partial class User : IValidable, IMergeable<User>
  {

    public bool All => Machines.Length == 0;

    public void Merge(User other) {

      if (All)
        return;

      if (other.All)
        return;

      var md = Machines.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
      foreach (var m in other.Machines) {
        if (!md.ContainsKey(m.Name))
          md.Add(m.Name, m);
      }
      Machines = md.Values.ToArray();

    }
    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("User attribute 'name' must be non-empty.");
      Name = Name.Standardize();

      Email = Email?.Trim();

      if (Machines == null)
        Machines = Array.Empty<Machine>();

      try {
        var hs = new HashSet<string>();
        foreach (var m in Machines) {
          m.Validate();
          if (!hs.Add(m.Name))
            throw new ValidationException($"Machine '{m.Name}' is duplicated.");
        }
      }
      catch (ValidationException vex) {
        vex.Parents.Add($"User => {Name}");
        throw;
      }

    }

  }
}





