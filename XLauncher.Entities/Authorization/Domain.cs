using System;
using System.Collections.Generic;
using System.Linq;

namespace XLauncher.Entities.Authorization
{

  using Common;
  using XLauncher.Common;

  public partial class Domain : IValidable, IMergeable<Domain>
  {

    public bool All => Users.Length == 0;
    public string Key => Name + "|" + AuthType.ToString();

    public void Merge(Domain other) {

      if (other.AuthType != AuthType)
        throw new Exception("Invalid merge of different authTypes.");

      if (All)
        return;

      if (other.All)
        return;

      var ud = Users.ToDictionary(u => u.Name, StringComparer.OrdinalIgnoreCase);
      foreach (var u in other.Users) {
        if (ud.ContainsKey(u.Name))
          ud[u.Name].Merge(u);
        else
          ud.Add(u.Name, u);
      }
      Users = ud.Values.ToArray();

    }
    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Domain attribute 'name' must be non-empty.");
      Name = Name.Standardize();

      if (Users == null)
        Users = Array.Empty<User>();

      try {
        var hs = new HashSet<string>();
        foreach (var u in Users) {
          u.Validate();
          if (!hs.Add(u.Name))
            throw new ValidationException($"User '{u.Name}' is duplicated.");
        }
      }
      catch (ValidationException vex) {
        vex.Parents.Add($"Domain => {Key}");
        throw;
      }

    }

  }
}
