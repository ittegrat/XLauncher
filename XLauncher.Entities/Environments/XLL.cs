
namespace XLauncher.Entities.Environments
{
  public partial class XLL
  {

    public override string Id => $"{Arch}::{base.Id}";
    public override string QFileName => $"{Arch}::{base.QFileName}";

  }
}
