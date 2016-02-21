﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LobsterModel.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LobsterSettings")]
        public string SettingsDirectory {
            get {
                return ((string)(this["SettingsDirectory"]));
            }
            set {
                this["SettingsDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("lobster.log")]
        public string LogFilename {
            get {
                return ((string)(this["LogFilename"]));
            }
            set {
                this["LogFilename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool BackupEnabled {
            get {
                return ((bool)(this["BackupEnabled"]));
            }
            set {
                this["BackupEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LobsterSettings/DatabaseConfig.xsd")]
        public string DatabaseConfigSchemaFilename {
            get {
                return ((string)(this["DatabaseConfigSchemaFilename"]));
            }
            set {
                this["DatabaseConfigSchemaFilename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("temp")]
        public string TempFileDirectory {
            get {
                return ((string)(this["TempFileDirectory"]));
            }
            set {
                this["TempFileDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LobsterSettings/ClobType.xsd")]
        public string ClobTypeSchemaFilename {
            get {
                return ((string)(this["ClobTypeSchemaFilename"]));
            }
            set {
                this["ClobTypeSchemaFilename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("backup")]
        public string BackupDirectory {
            get {
                return ((string)(this["BackupDirectory"]));
            }
            set {
                this["BackupDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>.html</string>
  <string>.txt</string>
  <string>.xml</string>
  <string>.xmls</string>
  <string>.xmlp</string>
  <string>.js</string>
  <string>.css</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection DiffableExtensions {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["DiffableExtensions"]));
            }
            set {
                this["DiffableExtensions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LogSensitiveMessages {
            get {
                return ((bool)(this["LogSensitiveMessages"]));
            }
            set {
                this["LogSensitiveMessages"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int FileUpdateTimeoutMilliseconds {
            get {
                return ((int)(this["FileUpdateTimeoutMilliseconds"]));
            }
            set {
                this["FileUpdateTimeoutMilliseconds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AppendFooterToDatabaseFiles {
            get {
                return ((bool)(this["AppendFooterToDatabaseFiles"]));
            }
            set {
                this["AppendFooterToDatabaseFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LogFileEvents {
            get {
                return ((bool)(this["LogFileEvents"]));
            }
            set {
                this["LogFileEvents"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LobsterTypes")]
        public string ClobTypeDirectoryName {
            get {
                return ((string)(this["ClobTypeDirectoryName"]));
            }
            set {
                this["ClobTypeDirectoryName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LobsterConnection.xml")]
        public string DatabaseConfigFileName {
            get {
                return ((string)(this["DatabaseConfigFileName"]));
            }
            set {
                this["DatabaseConfigFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection CodeSourceDirectories {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["CodeSourceDirectories"]));
            }
            set {
                this["CodeSourceDirectories"] = value;
            }
        }
    }
}
