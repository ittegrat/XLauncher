using System;
using IO = System.IO;
using System.Linq;

namespace XLauncher.Entities.Common
{
  public partial class PathInfo : IValidable
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public string FileName => IO.Path.GetFileName(Path).Trim();

    public string GetRootedPath(string root) {
      try {
        var path = IO.Path.IsPathRooted(Path)
          ? Path
          : IO.Path.Combine(root, Path)
        ;
        return IO.Path.GetFullPath(path);
      }
      catch (Exception ex) {
        logger.Debug(ex, $"Invalid rooted path: root='{root}', path='{Path}'");
        throw new ValidationException($"Invalid rooted path for '{Path}'", ex);
      }
    }
    public virtual void Validate() {

      if (String.IsNullOrWhiteSpace(Path))
        throw new ValidationException("PathInfo attribute 'path' must be non-empty.");
      Path = Path.Trim();

      if (Path.Any(c => Array.IndexOf(IO.Path.GetInvalidPathChars(), c) >= 0))
        throw new ValidationException($"Path '{Path}' contains illegal characters.");

    }

  }
}
