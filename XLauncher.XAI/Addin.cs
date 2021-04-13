using System;
using System.IO;
using System.Linq;

using ExcelDna.Integration;

namespace XLauncher.XAI
{

  using Common;
  using Entities.Session;

  public class Addin : IExcelAddIn
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public static Session Session { get; private set; } = new Session();

    public void AutoOpen() {
      string sf = null;
      try {

        var cd = Directory.GetCurrentDirectory();

        sf = Environment.GetEnvironmentVariable(Strings.XLSESSION_EVAR);
        if (string.IsNullOrWhiteSpace(sf)) {
          var root = Path.GetDirectoryName(ExcelDnaUtil.XllPath);
          sf = Path.Combine(root, Strings.XLSESSION_FILENAME);
        }

        Session = Session.Load(sf);

        LoadAddins();

        if (!String.IsNullOrWhiteSpace(Session.Title))
          XlCall.Excel(XlCall.xlfAppTitle, Session.Title.Trim());
        XlCall.Excel(XlCall.xlcMessage, false);

        Directory.SetCurrentDirectory(cd);

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load session {sf}");
      }
    }
    public void AutoClose() {
      logger.Info($"Closing {GetType().FullName}.");
    }

    void LoadAddins() {

      var ais = Session.Contexts.SelectMany(f => f.Addins);

      logger.Debug($"LoadGlobalsFirst is '{Session.LoadGlobalsFirst}'");
      if (Session.LoadGlobalsFirst)
        ais = Session.Addins.Concat(ais);
      else
        ais = ais.Concat(Session.Addins);

      foreach (var ai in ais) {

        if (!File.Exists(ai.Path)) {
          logger.Error($"Cannot find file '{ai.Path}'.");
          continue;
        }

        XlCall.Excel(XlCall.xlcMessage, true, $"Loading {Path.GetFileName(ai.Path)}...");

        if (Path.GetExtension(ai.Path).Trim().ToUpperInvariant() == ".XLL")
          LoadXLL(ai.Path);
        else
          ComLoadAddin(ai.Path, ai.ReadOnly);

        XlCall.Excel(XlCall.xlcMessage, false);

      }

    }
    void LoadXLL(string path) {
      try {
        logger.Debug($"RegisterXLL '{path}'");
        Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
        var ans = ExcelIntegration.RegisterXLL(path);
      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load XLL");
      }
    }
    void CApiLoadAddin(string path, bool ro) {
      try {

        logger.Debug($"XlCall.xlcOpen '{path}'");
        XlCall.Excel(XlCall.xlcOpen,
          path,          // file_text
          3,             // (int)update_links
          ro,            // read_only
          Type.Missing,  // format (delimiters in text)
          Type.Missing,  // Prot_pwd (Password)
          Type.Missing,  // write_res_pwd (WriteResPassword)
          true,          // ignore_rorec (IgnoreReadOnlyRecommended)
          2,             // File_origin (Origin) --> Windows (ANSI)
          Type.Missing,  // Custom_delimit
          false,         // Add_logical 
          Type.Missing,  // Editable 
          Type.Missing,  // File_access
          Type.Missing,  // Notify_logical 
          Type.Missing   // Converter 
        );

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load Addin");
      }
    }
    void ComLoadAddin(string path, bool ro) {
      try {

        logger.Debug($"Workbooks.Open '{path}'");
        ((dynamic)ExcelDnaUtil.Application).Workbooks.Open(
          path,          // Filename
          3,             // (int)UpdateLinks (3 --> External references will be updated)
          ro,            // ReadOnly
          Type.Missing,  // Format (Text delimiter)
          Type.Missing,  // Password (Password required to open a protected workbook)
          Type.Missing,  // WriteResPassword (Password required to write to a write-reserved workbook)
          true,          // IgnoreReadOnlyRecommended
          2,             // (XlPlatform)Origin (2 --> xlWindows)
          Type.Missing,  // Delimiter (If Format is 6 --> use first char of Delimiter as delimiter)
          false,         // Editable (If the file is an Excel template, open a new workbook based on it)
          Type.Missing,  // Notify
          Type.Missing,  // Converter
          false,         // AddToMru
          Type.Missing,  // Local
          Type.Missing   // CorruptLoad
        );

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load '{path}'");
      }
    }

  }

}
