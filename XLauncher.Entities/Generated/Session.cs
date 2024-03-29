//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 
namespace XLauncher.Entities.Session {
    using System.Xml.Serialization;
    using XLauncher.Entities.Common;
    
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Addin))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:schemas-bimi-com:XLauncher:Session")]
    public partial class Addin : PathInfo {
        
        private bool readOnlyField;
        
        public Addin() {
            this.readOnlyField = true;
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(true)]
        [System.Xml.Serialization.XmlAttributeAttribute("readOnly")]
        public bool ReadOnly {
            get {
                return this.readOnlyField;
            }
            set {
                this.readOnlyField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:schemas-bimi-com:XLauncher:Session")]
    [System.Xml.Serialization.XmlRootAttribute("session", Namespace="urn:schemas-bimi-com:XLauncher:Session", IsNullable=false)]
    public partial class Session {
        
        private Addin[] addinField;
        
        private Context[] contextField;
        
        private string titleField;
        
        private bool loadGlobalsFirstField;
        
        public Session() {
            this.loadGlobalsFirstField = true;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("addin")]
        public Addin[] Addins {
            get {
                return this.addinField;
            }
            set {
                this.addinField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("context")]
        public Context[] Contexts {
            get {
                return this.contextField;
            }
            set {
                this.contextField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("title")]
        public string Title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <remarks/>
        [System.ComponentModel.DefaultValueAttribute(true)]
        [System.Xml.Serialization.XmlAttributeAttribute("loadGlobalsFirst")]
        public bool LoadGlobalsFirst {
            get {
                return this.loadGlobalsFirstField;
            }
            set {
                this.loadGlobalsFirstField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:schemas-bimi-com:XLauncher:Session")]
    public partial class Context {
        
        private Addin[] addinField;
        
        private Param[] paramField;
        
        private string nameField;
        
        private string versionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("addin")]
        public Addin[] Addins {
            get {
                return this.addinField;
            }
            set {
                this.addinField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("param")]
        public Param[] Params {
            get {
                return this.paramField;
            }
            set {
                this.paramField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("version")]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:schemas-bimi-com:XLauncher:Session")]
    public partial class Param {
        
        private string nameField;
        
        private ParamType typeField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("type")]
        public ParamType Type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute("value")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:schemas-bimi-com:XLauncher:Session")]
    public enum ParamType {
        
        /// <remarks/>
        Boolean,
        
        /// <remarks/>
        DateTime,
        
        /// <remarks/>
        String,
    }
}
