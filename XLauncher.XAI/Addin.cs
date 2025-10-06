using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;

namespace XLauncher.XAI
{

  using Common;
  using Entities.Session;

  public class Addin : IExcelAddIn
  {

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetDllDirectory([Optional] string lpPathName);

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

        Directory.SetCurrentDirectory(cd);

      }
      catch (Exception ex) {
        logger.Error(ex, $"Cannot load session '{sf}'");
        MessageBox.Show(
          $"Cannot load session.\nSession: {sf}\nError: {ex.Message}",
          $"{Strings.APP_NAME} - Load Session",
          MessageBoxButtons.OK, MessageBoxIcon.Error
        );
      }
    }
    public void AutoClose() {
      logger.Debug($"Closing {GetType().FullName}.");
    }

    void LoadAddins() {

      var ais = Session.Contexts.SelectMany(f => f.Addins);

      logger.Debug($"LoadGlobalsFirst is '{Session.LoadGlobalsFirst}'");
      if (Session.LoadGlobalsFirst)
        ais = Session.Addins.Concat(ais);
      else
        ais = ais.Concat(Session.Addins);

      foreach (var ai in ais) {

        XlCall.Excel(XlCall.xlcMessage, true, $"Loading {Path.GetFileName(ai.Path)}...");

        try {

          if (!File.Exists(ai.Path)) {
            logger.Error($"Cannot find the file '{ai.Path}'.");
            throw new FileNotFoundException("Unable to find the specified file.", ai.Path);
          }

          if (Path.GetExtension(ai.Path).Trim().ToUpperInvariant() == ".XLL")
            LoadXLL(ai.Path);
          else
            ComLoadAddin(ai.Path, ai.ReadOnly);

        }
        catch (Exception ex) {

          string errMsg;
          MessageBoxIcon icon;

          switch (ex) {
            case FileNotFoundException fex:
              errMsg = $"Cannot find the file:\n{fex.FileName}";
              icon = MessageBoxIcon.Warning;
              break;
            case FileLoadException fex:
              errMsg = $"Cannot load the file:\n{fex.FileName}";
              icon = MessageBoxIcon.Error;
              break;
            default:
              logger.Error($"Cannot load the file '{ai.Path}'.");
              errMsg = $"Cannot load the file:\n{ai.Path}\n\n{(ex.Message.TrimEnd('.') + ".")}";
              icon = MessageBoxIcon.Error;
              break;
          }

          var ans = MessageBox.Show(
            $"{errMsg}\n\nContinue to load the environment '{Session.Title}' ?",
            $"{Strings.APP_NAME} - Load addin",
            MessageBoxButtons.YesNo, icon, MessageBoxDefaultButton.Button2
          );

          XlCall.Excel(XlCall.xlcMessage, false);

          if (ans == DialogResult.Yes) {
            logger.Debug($"Continue loading environment '{Session.Title}'.");
            continue;
          }
          else {
            logger.Debug($"Skip loading remaining addins.");
            return;
          }

        }

        XlCall.Excel(XlCall.xlcMessage, false);

      }

    }
    void LoadXLL(string path) {
      logger.Debug($"RegisterXLL '{path}'.");
      var dir = Path.GetDirectoryName(path);
      if (SetDllDirectory(dir)) {
        var loaded = ExcelIntegration.RegisterXLL(path) is string;
        logger.Debug($"{(loaded ? "Loaded" : "Cannot load")} '{path}'.");
        if (!SetDllDirectory(String.Empty))
          throw new Win32Exception(Marshal.GetLastWin32Error());
        if (loaded)
          return;
        throw new FileLoadException("Could not load the specified file.", path);
      }
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
    void CApiLoadAddin(string path, bool ro) {
      logger.Debug($"XlCall.xlcOpen '{path}'.");
      var loaded = (bool)XlCall.Excel(XlCall.xlcOpen, path, ExcelMissing.Value, ro);
      if (!loaded)
        throw new FileLoadException("Could not load the specified file.", path);
      logger.Debug($"Loaded '{path}'.");
    }
    void ComLoadAddin(string path, bool ro) {
      logger.Debug($"Workbooks.Open '{path}'.");
      var xlApp = ExcelDnaUtil.Application as Excel.Application;
      var ans = xlApp.Workbooks.Open(path, ReadOnly: ro);
      logger.Debug($"Loaded '{path}'.");
    }

  }

}
