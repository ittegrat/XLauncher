
namespace XLauncher.Entities.Environments
{

  using Session;

  public partial class NullControl
  {

    public override void Apply(Param param) {
    }
    public override Param ToParam() {
      return null;
    }

  }
}
