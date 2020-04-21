using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace XLauncher.UI.DataAdapters
{

  using Common;
  using EE = Entities.Environments;
  using ES = Entities.Session;

  public class Environment : INotifyPropertyChanged
  {

    public const string GroupPropertyName = nameof(Group);

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static readonly string PublicGroupName = Configuration.Instance.GroupNamePublic;
    static readonly string LocalGroupName = Configuration.Instance.GroupNameLocal;

    readonly EE.Environment environment;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool HasPreferences => ResetPreferences(false);
    public bool IsLocal { get; }
    public string Group => IsLocal ? LocalGroupName : environment.Group.IfNull(PublicGroupName);
    public string Id => $"{Group}::{Name}";
    public string Name => environment.Name;
    public IEnumerable<string> FNames => environment.Frameworks.Select(f => f.Name);
    public IEnumerable<(string Name, string Value)> EVars => environment.Frameworks.SelectMany(f => f.EVars).Select(ev => (ev.Name, ev.Value));

    public bool Dirty { get; set; } = false;

    public static void Fill(ICollection<Environment> environments) {
      try {

        logger.Trace($@"User is: {App.Domain}\{App.User}@{App.Machine}");

        var envs = Configuration.Instance.PublicRoots
          .SelectMany(root => EE.Environment.LoadMany(root))
          .Where(e => e.IsAuthorized(App.Domain, App.User, App.Machine, Configuration.Instance.DefaultAuth))
          .Select(e => new Environment(e, false))
          .Concat(
            EE.Environment.LoadMany(Configuration.Instance.UserRoot)
              .Select(e => new Environment(e, true))
          )
        ;

        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var env in envs) {
          if (hs.Add(env.Id))
            environments.Add(env);
          else
            logger.Error($"Environment '{env.Id}' is duplicated.");
        }

        var prefs = Directory.EnumerateFiles(Configuration.Instance.LocalSettingsFolder, "*.xml")
          .Where(f => !f.Equals(Configuration.Instance.LocalSettingsFilename, StringComparison.OrdinalIgnoreCase))
          .Select(f => (f, ES.Session.Load(f)))
        ;

        foreach (var (file, session) in prefs) {

          var env = environments
            .Where(e => e.Id.Equals(session.Title, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault()
          ;

          if (env != null) {
            env.environment.Apply(session);
          } else {
            File.Delete(file);
          }

        }

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't load environments");
      }
    }

    Environment(EE.Environment env, bool local) { this.environment = env; IsLocal = local; }

    //++++++++++     API     ++++++++++
    public void Add(EVar ev, EVar after) {
      var fw = environment.Frameworks.First(f => f.Name == ev.Framework);
      var evs = new List<EE.EVar>(fw.EVars);
      var i = evs.IndexOf((EE.EVar)after);
      evs.Insert(i >= 0 ? i + 1 : evs.Count, (EE.EVar)ev);
      fw.EVars = evs.ToArray();
    }
    public void Remove(EVar ev) {
      var fw = environment.Frameworks.First(f => f.Name == ev.Framework);
      var evs = new List<EE.EVar>(fw.EVars);
      evs.Remove((EE.EVar)ev);
      fw.EVars = evs.ToArray();
    }
    public void MoveUp(EVar ev) {
      var fw = environment.Frameworks.First(f => f.Name == ev.Framework);
      var evs = new List<EE.EVar>(fw.EVars);
      var i = evs.IndexOf((EE.EVar)ev);
      if (i > 0) {
        var x = evs[i];
        evs[i] = evs[i - 1];
        evs[i - 1] = x;
        fw.EVars = evs.ToArray();
      }
    }
    public void MoveDown(EVar ev) {
      var fw = environment.Frameworks.First(f => f.Name == ev.Framework);
      var evs = new List<EE.EVar>(fw.EVars);
      var i = evs.IndexOf((EE.EVar)ev);
      if (i < evs.Count - 1) {
        var x = evs[i];
        evs[i] = evs[i + 1];
        evs[i + 1] = x;
        fw.EVars = evs.ToArray();
      }
    }

    public void Add(Addin ai, Addin after) {
      var fw = environment.Frameworks.First(f => f.Name == ai.Framework);
      var ais = new List<EE.Addin>(fw.Addins);
      var i = ais.IndexOf((EE.Addin)after);
      ais.Insert(i >= 0 ? i + 1 : ais.Count, (EE.Addin)ai);
      fw.Addins = ais.ToArray();
    }
    public void Remove(Addin ai) {
      var fw = environment.Frameworks.First(f => f.Name == ai.Framework);
      var ais = new List<EE.Addin>(fw.Addins);
      ais.Remove((EE.Addin)ai);
      fw.Addins = ais.ToArray();
    }
    public void MoveUp(Addin ai) {
      var fw = environment.Frameworks.First(f => f.Name == ai.Framework);
      var ais = new List<EE.Addin>(fw.Addins);
      var i = ais.IndexOf((EE.Addin)ai);
      if (i > 0) {
        var x = ais[i];
        ais[i] = ais[i - 1];
        ais[i - 1] = x;
        fw.Addins = ais.ToArray();
      }
    }
    public void MoveDown(Addin ai) {
      var fw = environment.Frameworks.First(f => f.Name == ai.Framework);
      var ais = new List<EE.Addin>(fw.Addins);
      var i = ais.IndexOf((EE.Addin)ai);
      if (i < ais.Count - 1) {
        var x = ais[i];
        ais[i] = ais[i + 1];
        ais[i + 1] = x;
        fw.Addins = ais.ToArray();
      }
    }

    public void Render(UIElementCollection Controls, ICollection<Addin> addins, ICollection<EVar> evars) {
      Render(Controls);
      Render(addins);
      Render(evars);
    }
    public void Render(ICollection<Addin> addins) {
      Addin.Fill(addins, environment);
    }
    public void Render(ICollection<EVar> evars) {
      EVar.Fill(evars, environment);
    }

    public Environment Clone(string newName) {
      var env = environment.Clone();
      env.Name = newName;
      env.Auths = null;
      env.Imports = null;
      env.Validate();
      return new Environment(env, true);
    }
    public void Delete() {

      if (!IsLocal)
        return;

      var path = GetPersistenceDirname();
      try {
        Directory.Delete(path, true);
      }
      catch (Exception ex) {
        logger.Error(ex, $"Can't delete environment '{Name}' (aka: {Path.GetFileName(path)})");
      }

    }
    public void Rename(string name) {
      Delete();
      environment.Name = name;
      Dirty = true;
      Save();
      RaisePropertyChanged("Name");
    }
    public void Reset() {
      ResetPreferences(true);
    }
    public void Save() {

      if (!Dirty)
        return;

      if (IsLocal) {
        environment.SaveAs(GetPersistenceDirname(), true);
      } else {
        var session = environment.ToSession(Configuration.Instance.LocalSettings.ExcelArch);
        session.Title = Id;
        session.Save(GetPreferencesFilename());
      }

      Dirty = false;

    }
    public void SaveSession(string path) {

      var session = environment.ToSession(Configuration.Instance.LocalSettings.ExcelArch);
      session.Addins = Configuration.Instance.LocalSettings.GlobalAddins
        .Where(a => a.Active)
        .Select(a => new ES.Addin { Path = a.Path, ReadOnly = a.ReadOnly })
        .ToArray()
      ;
      session.LoadGlobalsFirst = Configuration.Instance.LocalSettings.LoadGlobalsFirst;
      session.Title = Id;

      session.Save(path);

    }
    //+++++++++++++++++++++++++++++++++

    string GetPersistenceDirname() {
      return Path.Combine(Configuration.Instance.UserRoot, $"{Name.ToPascalCase("_")}");
    }
    string GetPreferencesFilename() {
      return Path.Combine(Configuration.Instance.LocalSettingsFolder, $"{Id.ToPascalCase(".")}.xml");
    }
    void RaisePropertyChanged([CallerMemberName] string propName = null) {
      var handler = PropertyChanged;
      handler?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
    void Render(UIElementCollection Controls) {

      Controls.Clear();

      foreach (var f in environment.Frameworks) {
        foreach (var b in f.Boxes) {
          if (!b.Controls.All(c => c is EE.NameValuePair))
            Controls.Add(Renderers.Render(b, this));
        }
      }

      if (Controls.Count == 0) {
        Controls.Add(Renderers.NoControls());
      }

    }
    bool ResetPreferences(bool reset) {
      var prefs = GetPreferencesFilename();
      var ans = File.Exists(prefs);
      if (reset && ans)
        File.Delete(prefs);
      return ans;
    }

  }

}
