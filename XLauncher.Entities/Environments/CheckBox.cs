using System;

namespace XLauncher.Entities.Environments
{

  using Session;

  public partial class CheckBox
  {

    public override void Apply(Param param) {
      Validate(param, TypeCode.Boolean);
      Value = Boolean.Parse(param.Value);
    }
    public override Param ToParam() {
      return new Param { Name = Name, Value = Value.ToString(), Type = ParamType.Boolean };
    }

  }

}
