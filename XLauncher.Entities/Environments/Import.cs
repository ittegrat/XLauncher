
namespace XLauncher.Entities.Environments
{
  public partial class Import
  {

    public override void Validate() {
      base.Validate();
      After = After?.Trim();
    }

  }
}

