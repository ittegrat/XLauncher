using System;
using System.Collections.Generic;

namespace XLauncher.Entities.Session
{

  using Common;

  public partial class Session : IEntity<Session>
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Session Load(string path) {
      try {

        logger.Trace($"Loading Session '{path}'.");

        var session = IEntityExtensions.Deserialize<Session>(path);
        return session;

      }
      catch (Exception ex) {
        if (!(ex is ValidationException vex)) {
          logger.Debug(ex, $"Error loading Session '{path}'");
          vex = new ValidationException($"{ex.Message}", ex);
        }
        vex.Parents.Add(path);
        throw vex;
      }
    }

    public void FixEmpty() {
      foreach (var ct in Contexts)
        ct.FixEmpty();
    }
    public void Save(string path) {
      logger.Trace($"Saving Session '{path}'.");
      Validate();
      this.Serialize(path);
    }
    public void Validate() {

      Title = Title?.Trim();

      if (Addins == null)
        Addins = Array.Empty<Addin>();

      if (Contexts == null)
        Contexts = Array.Empty<Context>();

      if (Addins.Length + Contexts.Length == 0) {
        var title = String.IsNullOrEmpty(Title) ? String.Empty : $" '{Title}'";
        throw new ValidationException($"Invalid Session{title}. At least one of 'addin' or 'context' is required.");
      }

      try {

        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ai in Addins) {
          ai.Validate();
          if (!hs.Add(ai.FileName))
            throw new ValidationException($"Addin '{ai.FileName}' is duplicated.");
        }

        foreach (var ct in Contexts) {
          ct.Validate();
          foreach (var ai in ct.Addins) {
            if (!hs.Add(ai.FileName))
              throw new ValidationException($"Addin '{ai.FileName}' is duplicated.");
          }
        }

      }
      catch (ValidationException vex) {
        if (String.IsNullOrEmpty(Title))
          vex.Parents.Add($"Session => {Title}");
        throw;
      }

    }

  }

}
