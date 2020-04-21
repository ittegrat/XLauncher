using System;

namespace XLauncher.Entities.Environments
{

  using Common;
  using Session;

  public partial class NameValuePair
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
      if (String.IsNullOrWhiteSpace(Value))
        throw new ValidationException("NameValuePair attribute 'value' must be non-empty.");
      Value = Value.Trim();
    }

  }
}
