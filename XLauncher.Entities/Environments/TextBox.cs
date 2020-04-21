using System;

namespace XLauncher.Entities.Environments
{

  using Session;

  public partial class TextBox
  {

    public override void Apply(Param param) {
      Validate(param, TypeCode.String);
      Value = param.Value;
    }
    public override Param ToParam() {
      return new Param { Name = Name, Value = Value, Type = ParamType.String };
    }
    public override void Validate() {
      base.Validate();
      Text = Text?.Trim();
    }

  }

}
