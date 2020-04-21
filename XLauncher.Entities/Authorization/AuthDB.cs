using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XLauncher.Entities.Authorization
{

  using Common;

  public partial class AuthDB : IEntity<AuthDB>, IMergeable<AuthDB>
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static AuthDB Load(string path) {
      try {

        logger.Trace($"Loading AuthDB '{path}'.");

        var auth = IEntityExtensions.Deserialize<AuthDB>(path);

        for (var i = auth.Imports.Length - 1; i >= 0; --i) {
          var impPath = auth.Imports[i].GetRootedPath(Path.GetDirectoryName(path));
          var other = Load(impPath);
          auth.Merge(other);
        }

        return auth;

      }
      catch (Exception ex) {
        if (!(ex is ValidationException vex)) {
          logger.Debug(ex, $"Error loading AuthBD '{path}'");
          vex = new ValidationException($"{ex.Message}", ex);
        }
        vex.Parents.Add(path);
        throw vex;
      }
    }
    public static AuthDB New() {
      var authDB = new AuthDB {
        Imports = Array.Empty<PathInfo>(),
        Domains = Array.Empty<Domain>()
      };
      return authDB;
    }

    public bool IsAuthorized(string domain, string username, string machine) {

      if (String.IsNullOrWhiteSpace(domain))
        throw new ArgumentException("Domain must be non-empty.");

      if (String.IsNullOrWhiteSpace(username))
        throw new ArgumentException("Username must be non-empty.");

      if (String.IsNullOrWhiteSpace(machine))
        throw new ArgumentException("Machine must be non-empty.");

      var domains = Domains.Where(d => d.Name.Equals(domain, StringComparison.OrdinalIgnoreCase));

      var denyDomain = domains.SingleOrDefault(d => d.AuthType == AuthType.deny);
      if (denyDomain != null) {

        if (denyDomain.All)
          return false;

        var denyUser = denyDomain.Users.SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (denyUser != null) {

          if (denyUser.All)
            return false;

          var denyMachine = denyUser.Machines.SingleOrDefault(m => m.Name.Equals(machine, StringComparison.OrdinalIgnoreCase));
          if (denyMachine != null)
            return false;

        }

      }

      var allowDomain = domains.SingleOrDefault(d => d.AuthType == AuthType.allow);
      if (allowDomain != null) {

        if (allowDomain.All)
          return true;

        var allowUser = allowDomain.Users.SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (allowUser != null) {

          if (allowUser.All)
            return true;

          var allowMachine = allowUser.Machines.SingleOrDefault(m => m.Name.Equals(machine, StringComparison.OrdinalIgnoreCase));
          if (allowMachine != null)
            return true;

        }

      }

      return false;

    }
    public void Merge(AuthDB other) {

      var dd = Domains.ToDictionary(d => d.Key, StringComparer.OrdinalIgnoreCase);

      foreach (var d in other.Domains) {
        if (dd.ContainsKey(d.Key))
          dd[d.Key].Merge(d);
        else
          dd.Add(d.Key, d);
      }

      Domains = dd.Values.ToArray();

    }
    public void Validate() {

      if (Imports == null)
        Imports = Array.Empty<PathInfo>();

      if (Domains == null)
        Domains = Array.Empty<Domain>();

      if (Imports.Length + Domains.Length == 0)
        throw new ValidationException("Invalid AuthDB. At least one of 'import' or 'domain' is required.");

      foreach (var i in Imports)
        i.Validate();

      var hs = new HashSet<string>();
      foreach (var d in Domains) {
        d.Validate();
        if (!hs.Add(d.Key))
          throw new ValidationException($"Domain '{d.Key}' is duplicated.");
      }

    }

  }
}
