
namespace XLauncher.Entities.Environments
{

  using Session;

  public partial class ComboBoxEx
  {

    public override void Apply(Param param) {
      base.Apply(param);
      Active = true;
    }
    public override Param ToParam() {
      return Active
        ? new Param { Name = Name, Value = Value, Type = ParamType.String }
        : null
      ;
    }

  }

}
