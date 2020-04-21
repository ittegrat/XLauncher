using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLauncher.UI.DataAdapters
{

  using EE = Entities.Environments;

  public class EVar
  {

    public const string GroupPropertyName = nameof(Framework);

    readonly EE.EVar evar;

    public string Framework { get; set; }
    public string Name { get => evar.Name; set => evar.Name = value; }
    public string Value { get => evar.Value; set => evar.Value = value; }

    public EVar() : this(new EE.EVar(), null) { }
    EVar(EE.EVar ev, string framework) { evar = ev; Framework = framework; }

    public EVar Clone() {
      return new EVar {
        Framework = Framework,
        Name = Name,
        Value = Value
      };
    }
    public void SetValue(EVar other) {
      Framework = other.Framework;
      Name = other.Name;
      Value = other.Value;
    }

    public static void Fill(ICollection<EVar> evars, EE.Environment env) {
      evars.Clear();
      foreach (var ev in env.Frameworks.SelectMany(f => f.EVars.Select(v => new EVar(v, f.Name)))) {
        evars.Add(ev);
      }
    }

    public static explicit operator EE.EVar(EVar ev) { return ev?.evar; }

  }

}
