using System;
using System.Globalization;

namespace XLauncher.Entities.Environments
{

  using Session;

  public partial class DatePicker
  {

    const string format = "yyyy-MM-dd";

    public override void Apply(Param param) {
      Validate(param, TypeCode.DateTime);
      Value = DateTime.ParseExact(param.Value, format, CultureInfo.InvariantCulture);
      Active = true;
    }
    public override Param ToParam() {
      return Active
        ? new Param { Name = Name, Value = Value.ToString(format, CultureInfo.InvariantCulture), Type = ParamType.DateTime }
        : null
      ;
    }
    public override void Validate() {
      base.Validate();
      Text = Text?.Trim();
      if (Value == default)
        Value = DateTime.Today;
    }

  }

}
