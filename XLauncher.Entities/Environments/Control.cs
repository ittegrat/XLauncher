using System;
using System.Xml.Serialization;

namespace XLauncher.Entities.Environments
{

  using Common;
  using Session;

  public abstract partial class Control : IValidable
  {

    [XmlIgnore]
    public Framework Parent { get; private set; }

    public abstract void Apply(Param param);
    public abstract Param ToParam();
    public virtual void Validate() {
      if (String.IsNullOrWhiteSpace(Name))
        throw new ValidationException("Control attribute 'name' must be non-empty.");
      Name = Name.Trim();
    }
    public void SetParent(Framework parent) { Parent = parent; }

    protected void Validate(Param param, TypeCode typeCode) {
      if (!Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException($"Parameter name is '{param.Name}', but control name is '{Name}'.");
      if (param.TypeCode != typeCode)
        throw new ArgumentException($"Parameter type is '{param.TypeCode}', but control type is '{typeCode}'.");
    }

  }
}
