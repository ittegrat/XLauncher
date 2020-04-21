using System;
using System.Collections.Generic;

namespace XLauncher.Entities.Environments
{

  using Common;
  using Session;

  public partial class ComboBox
  {

    public override void Apply(Param param) {
      Validate(param, TypeCode.String);
      Value =
        Array.Find(Items, item => item.Equals(param.Value, StringComparison.OrdinalIgnoreCase))
        ?? Value;
    }
    public override Param ToParam() {
      return new Param { Name = Name, Value = Value, Type = ParamType.String };
    }
    public override void Validate() {

      base.Validate();

      Text = Text?.Trim();
      Value = Value?.Trim();

      if (Items == null || Items.Length == 0)
        throw new ValidationException($"Invalid ComboBox '{Name}'. At least one 'item' is required.");

      try {
        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < Items.Length; ++i) {
          if (String.IsNullOrWhiteSpace(Items[i]))
            throw new ValidationException("ComboBox elements 'item' must be non-empty.");
          Items[i] = Items[i].Trim();
          if (!hs.Add(Items[i]))
            throw new ValidationException($"Item '{Items[i]}' is duplicated.");
        }
      }
      catch (ValidationException vex) {
        vex.Parents.Add($"ComboBox => {Name}");
        throw;
      }

    }

  }

}
