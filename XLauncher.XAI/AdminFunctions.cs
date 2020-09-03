using System;
using System.Collections.Generic;
using System.Linq;

using ExcelDna.Integration;

namespace XLauncher.XAI
{

  using Entities.Authorization;
  using Entities.Environments;

  [ExcelFunction(
    Prefix = "XLauncher.Admin."
#if !DEBUG
    , IsHidden = true
#endif
  )]
  public static class AdminFunctions
  {

    static readonly char[] dumChars = new char[] { '\\', '@' };

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static List<(string Root, Environment Env)> Environments = new List<(string, Environment)>();

    public static object AllAuthorizedUsers(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        if (Environments.Count == 0)
          return "#ERROR: no loaded environments";

        var users = new HashSet<(string D, string U, string M)>();

        foreach (var env in Environments) {

          foreach (var d in env.Env.AuthDB.Domains) {

            if (d.All) {
              users.Add((d.Key, "any", "any"));
              continue;
            }
            foreach (var u in d.Users) {
              if (u.All) {
                users.Add((d.Key, u.Name, "any"));
                continue;
              }
              foreach (var m in u.Machines)
                users.Add((d.Key, u.Name, m.Name));
            }

          }

        }

        if (users.Count == 0)
          return ExcelError.ExcelErrorNA;

        var ans = new object[users.Count, 3];
        var i = 0;

        foreach (var user in users ) {
          ans[i, 0] = user.D;
          ans[i, 1] = user.U;
          ans[i, 2] = user.M;
          ++i;
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot build table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object AuthorizedEnvs(object[] Users, string DefaultAuth, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        if (Environments.Count == 0)
          return "#ERROR: no loaded environments";

        var users = Users
          .Where(o => !(o is ExcelEmpty))
          .Cast<string>()
          .Select(s => s.Trim())
          .Distinct()
        ;

        if (!users.Any())
          return "#ERROR: empty users";

        var defAuth = (AuthType)Enum.Parse(typeof(AuthType), DefaultAuth, true);

        var envs = new List<(string Domain, string User, string Machine, Environment Env, string Root)>();

        foreach (var user in users) {
          var dum = user.Split(dumChars);
          var d = dum[0].Trim().ToUpperInvariant();
          var u = dum[1].Trim().ToUpperInvariant();
          var m = dum.Length > 3 ? dum[2].Trim().ToUpperInvariant() : "*";
          var uenvs = Environments
            .Where(x => x.Env.IsAuthorized(d, u, m, defAuth))
            .Select(x => (d, u, m, x.Env, x.Root))
          ;
          envs.AddRange(uenvs);
        }

        if (envs.Count == 0)
          return ExcelError.ExcelErrorNA;

        var ans = new object[envs.Count, 6];
        for (var i = 0; i < envs.Count; ++i) {
          ans[i, 0] = envs[i].Domain;
          ans[i, 1] = envs[i].User;
          ans[i, 2] = envs[i].Machine;
          ans[i, 3] = envs[i].Env.Name;
          ans[i, 4] = envs[i].Env.Group ?? String.Empty;
          ans[i, 5] = envs[i].Root;
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot build table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object AuthorizedUsers(string Name, string Group, string Root, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        if (Environments.Count == 0)
          return "#ERROR: no loaded environments";

        var envs = Environments.Where(x => x.Env.Name.Equals(Name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (envs.Count > 1)
          envs = envs.Where(x => {
            var grp = x.Env.Group ?? "";
            return grp.Equals(Group.Trim(), StringComparison.OrdinalIgnoreCase);
          }).ToList();

        if (envs.Count > 1)
          envs = envs.Where(x => x.Root.Equals(Root.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (envs.Count > 1)
          return "#ERROR: duplicated environments";

        if (envs.Count == 0)
          return ExcelError.ExcelErrorNA;

        var users = new List<(string D, string U, string M)>();
        foreach (var d in envs[0].Env.AuthDB.Domains) {
          if (d.All) {
            users.Add((d.Key, "any", "any"));
            continue;
          }
          foreach (var u in d.Users) {
            if (u.All) {
              users.Add((d.Key, u.Name, "any"));
              continue;
            }
            foreach (var m in u.Machines)
              users.Add((d.Key, u.Name, m.Name));
          }
        }

        if (users.Count == 0)
          return ExcelError.ExcelErrorNA;

        var ans = new object[users.Count, 3];
        for (var i = 0; i < users.Count; ++i) {
          ans[i, 0] = users[i].D;
          ans[i, 1] = users[i].U;
          ans[i, 2] = users[i].M;
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot build table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object EnvsLoaded(object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        if (Environments.Count == 0)
          return ExcelError.ExcelErrorNA;

        var ans = new object[Environments.Count, 3];
        for (var i = 0; i < Environments.Count; ++i) {
          ans[i, 0] = Environments[i].Env.Name;
          ans[i, 1] = Environments[i].Env.Group ?? String.Empty;
          ans[i, 2] = Environments[i].Root;
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot process loaded environments");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object LoadEnvironments(object[] Roots, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        var roots = Roots
          .Where(o => !(o is ExcelEmpty))
          .Cast<string>()
          .Select(s => s.Trim())
          .ToList()
        ;

        if (roots.Count == 0)
          return "#ERROR: empty roots";

        var envs = roots.SelectMany(root => Environment.LoadMany(root).Select(e => (Root: root, Env: e))).ToList();

        Environments = envs;

        if (envs.Count == 0)
          return ExcelError.ExcelErrorNA;

        var ans = new object[envs.Count, 3];
        for (var i = 0; i < envs.Count; ++i) {
          ans[i, 0] = envs[i].Env.Name;
          ans[i, 1] = envs[i].Env.Group ?? String.Empty;
          ans[i, 2] = envs[i].Root;
        }

        return ans;

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load environments");
        return $"#ERROR: {ex.Message}";
      }

    }

  }

}
