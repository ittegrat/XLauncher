using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace XLauncher.Entities.Environments
{

  using Common;
  using Authorization;
  using Session;
  using XLauncher.Common;

  public partial class Environment : IEntity<Environment>
  {

    const string ENVROOTFILE = "_Environment.xml";
    const string AUTHDBFILE = "_Users.xml";
    const string LAST = "last";

    readonly static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    List<Framework> frameworks = new List<Framework>();
    Dictionary<string, Framework> fmap = new Dictionary<string, Framework>(StringComparer.OrdinalIgnoreCase);

    public AuthDB AuthDB { get; } = AuthDB.New();

    [XmlIgnore]
    public Framework[] Frameworks {
      get {
        return frameworks.ToArray();
      }
      set {
        frameworks = new List<Framework>(value);
      }
    }

    public static bool IsEnvironment(string path) {
      var ef = Path.Combine(path, ENVROOTFILE);
      return File.Exists(ef);
    }
    public static Environment Load(string path) {
      try {

        logger.Trace($"Loading Environment '{path}'.");

        if (!IsEnvironment(path))
          throw new Exception($"Environment '{path}' does not exist.");

        var ef = Path.Combine(path, ENVROOTFILE);
        var env = IEntityExtensions.Deserialize<Environment>(ef);

        var uf = Path.Combine(path, AUTHDBFILE);
        if (File.Exists(uf)) {
          var other = AuthDB.Load(uf);
          env.AuthDB.Merge(other);
        }

        for (var i = env.Auths.Length - 1; i >= 0; --i) {
          var impPath = env.Auths[i].GetRootedPath(path);
          var other = AuthDB.Load(impPath);
          env.AuthDB.Merge(other);
        }

        var inext = env.Imports.Length;
        for (var i = 0; i < env.Imports.Length; ++i) {

          var imp = env.Imports[i];

          if (!String.IsNullOrEmpty(imp.After)) {
            inext = i;
            break;
          }

          var impPath = imp.GetRootedPath(path);
          env.Import(impPath, imp.After, imp.WithAuth);

        }

        foreach (var f in Directory.EnumerateFiles(path, "*.xml")) {

          var fn = Path.GetFileName(f);
          if (Strings.IsReserved(fn[0]))
            continue;

          var fw = Framework.Load(f);
          env.Merge(fw, fw.After);

        }

        for (var i = inext; i < env.Imports.Length; ++i) {
          var imp = env.Imports[i];
          var impPath = imp.GetRootedPath(path);
          env.Import(impPath, imp.After.IfNull(LAST), imp.WithAuth);
        }

        try {
          var hse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
          var hsa = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
          foreach (var f in env.frameworks) {
            foreach (var ev in f.EVars) {
              if (!hse.Add(ev.Name))
                throw new ValidationException($"Environment variable '{ev.Name}' is duplicated.");
            }
            foreach (var ai in f.Addins) {
              if (!hsa.Add(ai.QFileName))
                throw new ValidationException($"Addin '{ai.QFileName}' is duplicated.");
            }
            foreach (var b in f.Boxes)
              foreach (var c in b.Controls)
                c.SetParent(f);
          }
        }
        catch (ValidationException vex) {
          vex.Parents.Add($"Environment => {env.Name}");
          throw;
        }

        return env;

      }
      catch (Exception ex) {
        if (!(ex is ValidationException vex)) {
          logger.Debug(ex, $"Error loading Environment '{path}'");
          vex = new ValidationException($"{ex.Message}", ex);
        }
        vex.Parents.Add(path);
        throw vex;
      }
    }
    public static List<Environment> LoadMany(string path) {

      var envs = new List<Environment>();

      try {

        if (!Directory.Exists(path))
          throw new ArgumentException($"Environment root '{path}' does not exist.");

        foreach (var folder in Directory.GetDirectories(path)) {
          if (IsEnvironment(folder)) {
            try {
              envs.Add(Environment.Load(folder));
            }
            catch (Exception ex) {
              logger.Error(ex, $"Environment '{folder}' is unloadable");
              if (ex is ValidationException vex) {
                logger.Debug("Parent chain is{0}", vex.Parents.Count > 0 ? ":" : " empty.");
                foreach (var p in vex.Parents)
                  logger.Debug($"{p}");
              }
            }
          }
        }

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't load environments");
      }

      return envs;

    }

    public void Apply(Session session) {
      foreach (var ctx in session.Contexts) {
        if (fmap.TryGetValue(ctx.Name, out var fw))
          fw.Apply(ctx);
      }
    }
    public Environment Clone() { return this.DeepClone(); }
    public bool IsAuthorized(string domain, string username, string machine, AuthType defaultAuth) {
      try {
        var ans = AuthDB.Domains.Length > 0
          ? AuthDB.IsAuthorized(domain, username, machine)
          : defaultAuth == AuthType.allow
        ;
        logger.Debug(
          "Authorization {0} for environment '{1}'{2}.",
          ans ? "granted" : "denied",
          Name,
          String.IsNullOrWhiteSpace(Group) ? String.Empty : $", group '{Group}'"
        );
        return ans;
      }
      catch (Exception ex) {
        logger.Error(ex, "Authorization failed");
        return false;
      }
    }
    public void Import(string path, string after, bool withAuth) {
      if (File.Exists(path)) {
        var fw = Framework.Load(path);
        Merge(fw, after);
      } else if (IsEnvironment(path)) {
        var env = Environment.Load(path);
        Merge(env, after, withAuth);
      } else
        throw new Exception($"Invalid import directive '{path}'.");
    }
    public void Merge(Environment other, string after, bool withAuth) {

      if (withAuth)
        AuthDB.Merge(other.AuthDB);

      var next = after;
      foreach (var fw in other.frameworks) {
        Merge(fw, next);
        next = fw.Name;
      }

    }
    public void Merge(Framework other, string after) {
      if (fmap.TryGetValue(other.Name, out var fw)) {
        fw.Merge(other);
      } else {
        int i;
        if (String.IsNullOrEmpty(after) || after.Equals(LAST, StringComparison.OrdinalIgnoreCase)) {
          i = frameworks.Count;
        } else {
          try {
            i = frameworks.IndexOf(fmap[after]);
            ++i;
          }
          catch {
            throw new Exception($"Framework not found '{after}'.");
          }
        }
        frameworks.Insert(i, other);
        fmap.Add(other.Name, other);
      }
    }
    public void SaveAs(string path, bool overwrite) {

      logger.Trace($"Saving Environment '{Name}'.");

      if (!overwrite && Directory.Exists(path))
        throw new Exception($"Folder '{path}' already exist.");

      Directory.CreateDirectory(path);

      var ef = Path.Combine(path, ENVROOTFILE);
      logger.Trace($"Saving Environment Root '{ef}'.");
      this.Serialize(ef);

      for (var i = 0; i < frameworks.Count; ++i) {
        var fw = frameworks[i];
        var ff = Path.Combine(path, $"{i:00}_{fw.Name.ToPascalCase()}.xml");
        logger.Trace($"Saving Framework '{ff}'.");
        fw.Serialize(ff);
      }

    }
    public Session ToSession(ArchType arch) {

      var session = new Session();

      //var fws = new List<Entities.Session.Context>();
      //foreach (var f in frameworks) {
      //
      //  var fw = new Entities.Session.Context();
      //  fws.Add(fw);
      //
      //  fw.Name = f.Name;
      //
      //  var ais = new List<Entities.Session.Addin>();
      //  foreach (var ai in f.Addins) {
      //    var sai = new Entities.Session.Addin { Path = ai.Path, ReadOnly = ai.ReadOnly };
      //    if (ai is XLL xll && xll.Arch != arch)
      //      continue;
      //    ais.Add(sai);
      //  }
      //  if (ais.Count > 0)
      //    fw.Addins = ais.ToArray();
      //
      //  var pars = new List<Entities.Session.Param>();
      //  foreach (var b in f.Boxes) {
      //    foreach (var c in b.Controls) {
      //
      //      var p = c.ToParam();
      //      if (p != null)
      //        pars.Add(p);
      //
      //    }
      //  }
      //  if (pars.Count > 0)
      //    fw.Params = pars.ToArray();
      //
      //}
      //if (fws.Count > 0)
      //  session.Frameworks = fws.ToArray();

      session.Contexts = frameworks
        .Select(f => f.ToContext(arch))
        .ToArray()
      ;

      return session;

    }
    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Environment attribute 'name' must be non-empty.");
      Name = Name.Trim().ValidateChars();

      Group = Group?.Trim().ValidateChars();

      if (Auths == null)
        Auths = Array.Empty<PathInfo>();

      if (Imports == null)
        Imports = Array.Empty<Import>();

      try {

        foreach (var au in Auths)
          au.Validate();

        foreach (var imp in Imports)
          imp.Validate();
      }
      catch (ValidationException vex) {
        vex.Parents.Add($"Environment => {Name}");
        throw;
      }

    }

  }
}
