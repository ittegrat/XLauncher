using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace System.Runtime.InteropServices.ComTypes
{

  [ComImport]
  [ClassInterface(ClassInterfaceType.None)]
  [Guid("00021401-0000-0000-C000-000000000046")]
  internal class ShellLink { }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("000214F9-0000-0000-C000-000000000046")]
  internal interface IShellLink
  {

    // https://docs.microsoft.com/en-us/windows/win32/shell/links
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ishelllinkw
    // https://docs.microsoft.com/en-us/windows/win32/api/objidl/nn-objidl-ipersistfile

    void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
    void GetIDList(out IntPtr ppidl);
    void SetIDList(IntPtr pidl);
    void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
    void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
    void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
    void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
    void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
    void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
    void GetHotkey(out ushort pwHotkey);
    void SetHotkey(ushort wHotkey);
    void GetShowCmd(out int piShowCmd);
    void SetShowCmd(int iShowCmd);
    void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
    void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
    void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
    void Resolve(IntPtr hwnd, uint fFlags);
    void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);

  }

}

namespace XLauncher.Setup
{

  internal class ShellLinkObject : IDisposable
  {

    // https://docs.microsoft.com/en-us/windows/win32/shell/shelllinkobject-object
    // https://web.archive.org/web/20150115033855/http://www.vbaccelerator.com:80/home/NET/Code/Libraries/Shell_Projects/Creating_and_Modifying_Shortcuts/ShellLink_Code_zip_ShellLink/ShellLink_cs.asp

    internal enum WindowStyle
    {
      // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
      Normal = 1,     // SW_SHOWNORMAL
      Minimized = 7,  // SW_SHOWMINNOACTIVE
      Maximized = 3   // SW_SHOWMAXIMIZED
    }

    const int maxCapacity = 16 * 1024;

    readonly IShellLink shellLink;
    bool disposed = false;

    public string Arguments {
      get {
        var buffer = new StringBuilder(maxCapacity, maxCapacity);
        shellLink.GetArguments(buffer, buffer.Capacity);
        return buffer.ToString();
      }
      set {
        shellLink.SetArguments(value);
      }
    }
    public string Description {
      get {
        var buffer = new StringBuilder(maxCapacity, maxCapacity);
        shellLink.GetDescription(buffer, buffer.Capacity);
        return buffer.ToString();
      }
      set {
        shellLink.SetDescription(value);
      }
    }
    public WindowStyle DisplayMode {
      get {
        shellLink.GetShowCmd(out var cmd);
        return (WindowStyle)cmd;
      }
      set {
        shellLink.SetShowCmd((int)value);
      }
    }
    public (string IconPath, int IconIndex) IconLocation {
      get {
        var buffer = new StringBuilder(maxCapacity, maxCapacity);
        shellLink.GetIconLocation(buffer, buffer.Capacity, out var idx);
        return (buffer.ToString(), idx);
      }
      set {
        shellLink.SetIconLocation(value.IconPath, value.IconIndex);
      }
    }
    public string LinkPath { get; private set; }
    public string TargetPath {
      get {
        var buffer = new StringBuilder(maxCapacity, maxCapacity);
        shellLink.GetPath(buffer, buffer.Capacity, IntPtr.Zero, 0);
        return buffer.ToString();
      }
      set {
        shellLink.SetPath(value);
      }
    }
    public string WorkingDirectory {
      get {
        var buffer = new StringBuilder(maxCapacity, maxCapacity);
        shellLink.GetWorkingDirectory(buffer, buffer.Capacity);
        return buffer.ToString();
      }
      set {
        shellLink.SetWorkingDirectory(value);
      }
    }

    // Hotkey (R/W) Gets or sets the keyboard shortcut for the link.
    // RelativePath Property
    //
    // Resolve 	Looks for the target of a Shell link, even if the target has been moved or renamed.
    // Save 	Saves all changes to the link.

    public static void CreateShortcut(string target, string link) {

      if (!File.Exists(target))
        throw new ArgumentException($"The target file '{target}' does not exist.");

      if (File.Exists(link))
        throw new ArgumentException($"The shortcut file '{link}' already exists.");

      using (var sl = new ShellLinkObject()) {
        sl.TargetPath = target;
        sl.SaveAs(link);
      }

    }

    public ShellLinkObject(string link) : this() {

      LinkPath = link;
      Load();

    }

    public void Dispose() {
      if (!disposed && (shellLink != null))
        Marshal.ReleaseComObject(shellLink);
      disposed = true;
    }
    public void Save() {
      Save(String.Empty, true, false);
    }
    public void SaveAs(string link, bool overwrite = false) {
      Save(link, overwrite, false);
    }
    public void SaveCopyAs(string link, bool overwrite = false) {
      Save(link, overwrite, true);
    }

    ShellLinkObject() {
      shellLink = (IShellLink)new ShellLink();
    }
    ~ShellLinkObject() {
      Dispose();
    }

    void Load() {

      if (!File.Exists(LinkPath))
        return;

      var file = (IPersistFile)shellLink;
      file.Load(LinkPath, 0);

      // ushort msTimeOut = 1000;
      // uint resolveFlags = 0x1;
      // uint flags = (uint)((int)resolveFlags | (msTimeOut << 16));
      // shellLink.Resolve(IntPtr.Zero, resolveFlags);

    }
    void Save(string link, bool overwrite, bool saveCopyAs) {

      if (link == String.Empty)
        link = LinkPath;
      else if ((!overwrite) && File.Exists(link))
        throw new ArgumentException($"The shortcut file '{link}' already exists.");

      var file = (IPersistFile)shellLink;
      file.Save(link, !saveCopyAs);

      if (!saveCopyAs)
        LinkPath = link;

    }

  }

}
