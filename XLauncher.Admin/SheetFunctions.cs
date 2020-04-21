using System;
using System.Collections.Generic;
using System.Linq;

using ExcelDna.Integration;

namespace XLauncher.Admin
{

  using Entities.Authorization;
  using Entities.Environments;

  [ExcelFunction(Prefix = "XLauncher.")]
  public static class XLauncherAdminFunctions
  {

    readonly static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static List<(string Root, Environment Env)> Environments = new List<(string, Environment)>();

    public static object LoadEnvs(object[] Roots, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        var roots = Roots.Cast<string>().Select(s => s.Trim()).ToList();

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
        logger.Error(ex, $"Cannot build parameter table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object EnvList(string Domain, string User, string Machine, string DefaultAuth, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        var defAuth = (AuthType)Enum.Parse(typeof(AuthType), DefaultAuth, true);

        Domain = Domain.Trim().ToUpperInvariant();
        User = User.Trim().ToUpperInvariant();
        Machine = Machine.Trim().ToUpperInvariant();

        var envs = Environments.Where(x => x.Env.IsAuthorized(Domain, User, Machine, defAuth)).ToList();

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
        logger.Error(ex, $"Cannot build parameter table");
        return $"#ERROR: {ex.Message}";
      }

    }
    public static object EnvUsers(string Name, string Group, string Root, object Trigger) {

      if (Trigger is ExcelError)
        return Trigger;

      try {

        var envs = Environments.Where(x => x.Env.Name.Equals(Name.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (envs.Count > 1)
          envs = envs.Where(x => x.Env.Group.Equals(Group.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

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
        logger.Error(ex, $"Cannot build parameter table");
        return $"#ERROR: {ex.Message}";
      }

    }

  }

}
