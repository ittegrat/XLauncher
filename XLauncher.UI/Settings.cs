using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace XLauncher.UI
{

  using Entities.Environments;
  using DataAdapters;

  public class LocalSettings
  {

    static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public bool ConfirmDelete { get; set; } = true;
    public bool LoadGlobalsFirst { get; set; } = true;
    public bool RestoreTab { get; set; } = true;
    public bool ShowToolTips { get; set; } = false;
    public string ExcelPath { get; set; }
    public string LastEnvironmentId { get; set; }
    public ArchType ExcelArch { get; set; }
    public List<string> LocalRoots { get; } = new List<string>();
    public ObservableCollection<GlobalAddin> GlobalAddins { get; } = new ObservableCollection<GlobalAddin>();

    [XmlIgnore]
    public IEnumerable<string> RootedLocalRoots => LocalRoots.Select(r => App.GetRootedPath(r)).Distinct();

    public static LocalSettings Load(string fileName) {

      if (!File.Exists(fileName)) {
        logger.Info($"Settings file '{fileName}' does not exist.");
        return New();
      }

      try {

        var xs = new XmlSerializer(typeof(LocalSettings));
        using (var xr = XmlReader.Create(fileName)) {
          var settings = (LocalSettings)xs.Deserialize(xr);
          return settings;
        }

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't load settings");
        return New();
      }

    }
    public void Save(string fileName) {

      try {

        var basedir = Path.GetDirectoryName(fileName);

        if (!Directory.Exists(basedir))
          Directory.CreateDirectory(basedir);

        var xs = new XmlSerializer(typeof(LocalSettings));
        var xws = new XmlWriterSettings { Indent = true };
        using (var xw = XmlWriter.Create(fileName, xws)) {
          xs.Serialize(xw, this);
          xw.Close();
        }

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't save settings");
      }

    }
    public bool SelectExcelPath() {
      try {

        string initialFolder = null;

        if (String.IsNullOrWhiteSpace(ExcelPath) || !File.Exists(ExcelPath)) {

          var (is32bit, path) = DetectExcelVersion();

          if (path != null) {
            ExcelArch = is32bit ? ArchType.x86 : ArchType.x64;
            initialFolder = Path.GetDirectoryName(path);
          } else {
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
          }

        } else {

          initialFolder = Path.GetDirectoryName(ExcelPath);

        }

        var ofd = new OpenFileDialog {
          DereferenceLinks = true,
          Filter = "MS Excel|excel.exe|All files (*.*)|*.*",
          InitialDirectory = initialFolder,
          Title = "Select Excel Path"
        };

        if (ofd.ShowDialog(Application.Current.MainWindow) == true) {

          ExcelPath = ofd.FileName;
          if (ExcelPath.ToUpperInvariant().Contains("X86")) {
            ExcelArch = ArchType.x86;
          } else {
            ExcelArch = ArchType.x64;
          }

        }

        return true;

      }
      catch (Exception ex) {
        logger.Error(ex, "Can't select Excel path");
        return false;
      }
    }

    static LocalSettings New() {

      var settings = new LocalSettings();

      var (is32bit, path) = DetectExcelVersion();
      if (path != null) {
        settings.ExcelArch = is32bit ? ArchType.x86 : ArchType.x64;
        settings.ExcelPath = path;
      }

      return settings;

    }
    static (bool is32bit, string path) DetectExcelVersion() {

      // https://stackoverflow.com/questions/3266675/how-to-detect-installed-version-of-ms-office

      var views = new RegistryView[] {
        RegistryView.Registry64,
        RegistryView.Registry32,
      };

      var versions = new string[] {
        "16.0",
        "15.0",
        "14.0",
        "12.0",
        "11.0",
      };

      bool is32 = false;
      string root = null;

      foreach (var view in views) {

        using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view)) {

          foreach (var ver in versions) {

            using (var sk = hklm.OpenSubKey($@"SOFTWARE\Microsoft\Office\{ver}\Excel\InstallRoot")) {
              if (sk != null) {
                is32 = (view == RegistryView.Registry32);
                root = sk.GetValue("Path")?.ToString();
                sk.Close();
                break;
              }
            }

          }

          hklm?.Close();

        }

      }

      var path = Path.Combine(root, "Excel.exe");

      if (File.Exists(path))
        return (is32, path);

      return (false, null);

    }

  }

}
