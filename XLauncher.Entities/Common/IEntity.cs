using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace XLauncher.Entities.Common
{

  internal interface IEntity<T> : IValidable
  {
  }

  internal interface IMergeable<T>
  {
    void Merge(T other);
  }

  // Reference types (arrays and strings) shall be validated. Value
  // types and enums are assigned to their default values; the .NET
  // serializer throws if can't parse an enum. The default value for
  // an enum is the first value.
  internal interface IValidable
  {
    void Validate();
  }

  internal static class IEntityExtensions
  {

    static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    static readonly XmlReaderSettings settings = GetXmlReaderSettings();

    public static T DeepClone<T>(this T t) where T : IEntity<T> {
      using (var ms = new MemoryStream()) {
        var bf = new BinaryFormatter();
        bf.Serialize(ms, t);
        ms.Position = 0;
        return (T)bf.Deserialize(ms);
      }
    }
    public static T Deserialize<T>(string path) where T : IEntity<T>, new() {

      if (!File.Exists(path))
        throw new Exception($"{typeof(T).Name} '{path}' does not exist.");

      var xs = new XmlSerializer(typeof(T));
      xs.UnknownNode += (s, e) => {
        throw new ValidationException($"Node '{e.Name}' at ({e.LineNumber},{e.LinePosition})", new Exception($"Unknown '{e.NodeType}'"));
      };

      using (var xr = XmlReader.Create(path, settings)) {
        T t;
        try {
          t = (T)xs.Deserialize(xr);
        }
        catch (InvalidOperationException iex) {
          var ex = iex.InnerException;
          logger.Debug($"Error deserializing '{path}'.");
          logger.Debug(ex.InnerException, ex.Message);
          throw new ValidationException($"{ex.Message} => {ex.InnerException.Message}");
        }
        t.Validate();
        return t;
      }
    }
    public static void Serialize<T>(this T t, string path) where T : IEntity<T> {

      t.Validate();

      var root = Path.GetDirectoryName(path);
      if (!Directory.Exists(root))
        Directory.CreateDirectory(root);

      var xs = new XmlSerializer(typeof(T));

      var xws = new XmlWriterSettings {
        //Encoding = new UnicodeEncoding(false, false),
        Indent = true,
        //OmitXmlDeclaration = false
      };
      using (var xw = XmlWriter.Create(path, xws)) {
        try { xs.Serialize(xw, t); }
        catch (InvalidOperationException iex) {
          throw new Exception($"Error serializing '{path}': " + iex.InnerException.Message);
        }
        xw.Close();
      }

    }

    static XmlReaderSettings GetXmlReaderSettings() {

      var asm = typeof(IEntityExtensions).Assembly;

      var schemas = new string[] {
        "Common.xsd",
        "Authorization.xsd",
        "Environments.xsd",
        "Session.xsd"
      };

      var resNames = schemas.Select(s => asm.GetManifestResourceNames().Single(rn => rn.EndsWith(s, StringComparison.OrdinalIgnoreCase)));

      var xrs = new XmlReaderSettings();
      xrs.ValidationType = ValidationType.Schema;
      xrs.ValidationEventHandler += (s, e) => {

        var xr = (XmlReader)s;

        var message = $"{xr.NodeType} '{xr.Name}' at ({e.Exception.LineNumber},{e.Exception.LinePosition})";

        if (e.Severity == XmlSeverityType.Warning) {
          logger.Warn(e.Exception, message);
          return;
        }

        throw new ValidationException(message, e.Exception);

      };

      foreach (var rn in resNames) {
        using (var rs = asm.GetManifestResourceStream(rn)) {
          var xs = XmlSchema.Read(rs, null);
          xrs.Schemas.Add(xs);
        }
      }

      return xrs;

    }

  }

}
