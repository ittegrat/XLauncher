
namespace XLauncher.Entities.Environments
{

  using XLauncher.Common;

  public partial class Addin
  {

    public virtual string Id => Key.IfNull(FileName);
    public virtual string QFileName => FileName;

    public override void Validate() {
      base.Validate();
      Key = Key?.Trim();
    }

  }

}
