using System;
using IO = System.IO;
using System.Windows.Media;

namespace XLauncher.UI.DataAdapters
{

  using Common;
  using Entities.Environments;

  public class GlobalAddin {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Brush XLLColor =>
      Configuration.Instance.LocalSettings.ExcelArch == ArchType.x86
      ? Configuration.Instance.X86Color
      : Configuration.Instance.X64Color
    ;

    public string DisplayName => Key.IfNull(FileName);
    public string FileName => IO.Path.GetFileName(Path).Trim();
    public bool IsXLL => IO.Path.GetExtension(Path).Equals(".XLL", StringComparison.OrdinalIgnoreCase);

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
