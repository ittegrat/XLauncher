using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace XLauncher.UI.DataAdapters
{

  using EE = Entities.Environments;

  public class Addin
  {

    public const string ANY = "any";
    public const string GroupPropertyName = nameof(Framework);

    public static IEnumerable<string> ArchTypes => GetArchTypes();
    public static Brush X86Color => Configuration.Instance.X86Color;
    public static Brush X64Color => Configuration.Instance.X64Color;

    readonly EE.Addin addin;

    public string FileName => addin.FileName;
    public string Id => (addin is EE.XLL ? String.Empty : $"{Arch}::") + addin.Id;
    public string QFileName => (addin is EE.XLL ? String.Empty : $"{Arch}::") + addin.QFileName;

    public bool ReadOnly { get => addin.ReadOnly; set => addin.ReadOnly = value; }
    public string Arch { get; set; }
    public string Framework { get; set; }
    public string Key { get => addin.Key; set => addin.Key = value; }
    public string Path { get => addin.Path; set => addin.Path = value; }

    public Addin() : this(new EE.Addin(), null) { }
    Addin(EE.Addin ai, string framework) {
      addin = ai;
      Arch = ai is EE.XLL xll ? xll.Arch.ToString() : ANY;
      Framework = framework;
    }

    public Addin Clone() {
      return new Addin {
        Arch = Arch,
        Framework = Framework,
        Key = Key,
        Path = Path,
        ReadOnly = ReadOnly
      };
    }
    public void SetValue(Addin other) {
      Arch = other.Arch;
      Framework = other.Framework;
      Key = other.Key;
      Path = other.Path;
      ReadOnly = other.ReadOnly;
    }

    public static void Fill(ICollection<Addin> addins, EE.Environment env) {
      addins.Clear();
      foreach (var ai in env.Frameworks.SelectMany(f => f.Addins.Select(a => new Addin(a, f.Name)))) {
        addins.Add(ai);
      }
    }

    public static explicit operator EE.Addin(Addin ai) {

      if (ai == null)
        return null;

      if (ai.Arch == ANY) {

        if (ai.addin is EE.XLL xll)
          return new EE.Addin { Key = xll.Key, Path = xll.Path, ReadOnly = xll.ReadOnly };
        else
          return ai.addin;

      } else {

        var xll = ai.addin as EE.XLL;
        if (xll == null)
          xll = new EE.XLL { Key = ai.addin.Key, Path = ai.addin.Path, ReadOnly = ai.addin.ReadOnly };

        xll.Arch = (EE.ArchType)Enum.Parse(typeof(EE.ArchType), ai.Arch);

        return xll;

      }

    }

    static IEnumerable<string> GetArchTypes() {
      var list = new List<string>(3);
      list.Add(ANY);
      list.AddRange(((EE.ArchType[])Enum.GetValues(typeof(EE.ArchType))).Select(t => t.ToString()));
      return list;
    }

  }

}
