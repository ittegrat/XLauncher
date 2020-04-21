using System;
using System.Collections.Generic;
using System.Linq;

namespace XLauncher.Entities.Environments
{

  using Common;
  using ES = Session;
  using XLauncher.Common;

  public partial class Framework : IEntity<Framework>, IMergeable<Framework>
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Framework Load(string path) {
      try {

        logger.Trace($"Loading Framework '{path}'.");

        var fw = IEntityExtensions.Deserialize<Framework>(path);
        return fw;

      }
      catch (Exception ex) {
        if (!(ex is ValidationException vex)) {
          logger.Debug(ex, $"Error loading Framework '{path}'");
          vex = new ValidationException($"{ex.Message}", ex);
        }
        vex.Parents.Add(path);
        throw vex;
      }
    }

    public void Apply(ES.Context context) {

      if (!String.Equals(context.Version, Version, StringComparison.OrdinalIgnoreCase))
        return;

      var controls = Boxes
        .SelectMany(b => b.Controls)
        .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase)
      ;

      foreach (var param in context.Params) {
        if (controls.TryGetValue(param.Name, out var control))
          control.Apply(param);
      }

      var dps = controls.Values
        .OfType<DatePicker>()
        .Where(d => d.Active == true)
        .Where(d => Array.FindIndex(context.Params, p => p.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase)) < 0)
      ;
      foreach (var dp in dps)
        dp.Active = false;

      var cbs = controls.Values
        .OfType<ComboBoxEx>()
        .Where(c => c.Active == true)
        .Where(c => Array.FindIndex(context.Params, p => p.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase)) < 0)
      ;
      foreach (var cb in cbs)
        cb.Active = false;

    }
    public void Merge(Framework other) {

      Version = (Version ?? "") + "|" + (other.Version ?? "");

      if (other.EVars.Length > 0) {
        var evs = new List<EVar>(EVars);
        foreach (var oev in other.EVars) {
          var i = evs.FindIndex(x => x.Name.Equals(oev.Name, StringComparison.OrdinalIgnoreCase));
          if (i >= 0)
            evs[i] = oev;
          else
            evs.Add(oev);
        }
        EVars = evs.ToArray();
      }

      if (other.Addins.Length > 0) {
        var ais = new List<Addin>(Addins);
        foreach (var oai in other.Addins) {
          var i = ais.FindIndex(x => x.Id.Equals(oai.Id, StringComparison.OrdinalIgnoreCase));
          if (i >= 0)
            ais[i] = oai;
          else
            ais.Add(oai);
        }
        Addins = ais.ToArray();
      }

      if (other.Boxes.Length > 0) {

        var boxes = new List<Box>(Boxes);
        var controls = Boxes.SelectMany((b, i) => b.Controls.Select((c, j) => new { c.Name, i, j })).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var ob in other.Boxes) {

          var b = Array.Find(Boxes, x => x.Text.Equals(ob.Text, StringComparison.OrdinalIgnoreCase));
          var bc = b != null ? new List<Control>(b.Controls) : null;

          var oc = new List<Control>(ob.Controls);

          foreach (var c in ob.Controls) {

            if (controls.TryGetValue(c.Name, out var x)) {
              if (String.Equals(Boxes[x.i].Text, b?.Text, StringComparison.OrdinalIgnoreCase))
                bc[x.j] = c;
              else
                Boxes[x.i].Controls[x.j] = c;
              oc.Remove(c);
            } else if (b != null) {
              bc.Add(c);
              oc.Remove(c);
            }

          }

          if (b != null)
            b.Controls = bc.ToArray();

          if (oc.Count > 0) {
            ob.Controls = oc.ToArray();
            boxes.Add(ob);
          }

        }

        Boxes = boxes.ToArray();

        //@  foreach box
        //@  
        //@    foreach ctrl
        //@  
        //@      if ctrl exist --> replace + remove
        //@  
        //@      if box exist --> add to it + remove
        //@  
        //@    if box.controls.count > 0 --> add box

      }

    }
    public ES.Context ToContext(ArchType arch) {

      var ctx = new ES.Context {
        Name = Name,
        Version = Version
      };

      ctx.Addins = Addins
        .Where(ai => !(ai is XLL xll && xll.Arch != arch))
        .Select(ai => new ES.Addin { Path = ai.Path, ReadOnly = ai.ReadOnly })
        .ToArray()
      ;

      ctx.Params = Boxes
        .SelectMany(b => b.Controls)
        .Select(c => c.ToParam())
        .Where(p => p != null)
        .ToArray()
      ;

      return ctx;

    }
    public void Validate() {

      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Framework attribute 'name' must be non-empty.");
      Name = Name.Trim().ValidateChars();

      Version = Version?.Trim();
      After = After?.Trim();

      if (EVars == null)
        EVars = Array.Empty<EVar>();

      if (Addins == null)
        Addins = Array.Empty<Addin>();

      if (Boxes == null)
        Boxes = Array.Empty<Box>();

      if (EVars.Length + Addins.Length + Boxes.Length == 0)
        throw new ValidationException($"Invalid Framework '{Name}'. At least one of 'evar', 'addin', 'xll' or 'box' is required.");

      try {

        var hs1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hs2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ev in EVars) {
          ev.Validate();
          if (!hs1.Add(ev.Name))
            throw new ValidationException($"Environment variable '{ev.Name}' is duplicated.");
        }

        hs1.Clear();
        foreach (var ai in Addins) {
          ai.Validate();
          if (!hs1.Add(ai.QFileName))
            throw new ValidationException($"Addin '{ai.QFileName}' is duplicated.");
          if (!hs2.Add(ai.Id))
            throw new ValidationException($"Addin '{ai.Id}' is duplicated.");
        }

        hs1.Clear();
        foreach (var b in Boxes) {
          b.Validate();
          foreach (var c in b.Controls) {
            if (!hs1.Add(c.Name))
              throw new ValidationException($"Control '{c.Name}' is duplicated.");
          }
        }

      }
      catch (ValidationException vex) {
        vex.Parents.Add($"Framework => {Name}");
        throw;
      }

    }

  }
}
