using IO = System.IO;
using System.Windows.Media;

namespace XLauncher.UI.DataAdapters
{

  using Common;

  public class GlobalAddin
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Brush RWColor => Configuration.Instance.RWColor;

    public string DisplayName => Key.IfNull(FileName);
    public string FileName => IO.Path.GetFileName(Path).Trim();

    public bool Active { get; set; }
    public bool ReadOnly { get; set; }
    public string Key { get; set; }
    public string Path { get; set; }

    public GlobalAddin Clone() {
      return new GlobalAddin {
        Active = Active,
        Key = Key,
        Path = Path,
        ReadOnly = ReadOnly
      };
    }

  }
}
