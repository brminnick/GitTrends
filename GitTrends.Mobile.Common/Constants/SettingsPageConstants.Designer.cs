﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GitTrends.Mobile.Common.Constants {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class SettingsPageConstants {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SettingsPageConstants() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("GitTrends.Mobile.Common.Constants.SettingsPageConstants", typeof(SettingsPageConstants).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string RegisterForNotifications {
            get {
                return ResourceManager.GetString("RegisterForNotifications", resourceCulture);
            }
        }
        
        public static string Theme {
            get {
                return ResourceManager.GetString("Theme", resourceCulture);
            }
        }
        
        public static string PreferredChartSettingsLabelText {
            get {
                return ResourceManager.GetString("PreferredChartSettingsLabelText", resourceCulture);
            }
        }
        
        public static string Language {
            get {
                return ResourceManager.GetString("Language", resourceCulture);
            }
        }
        
        public static string CreatedBy {
            get {
                return ResourceManager.GetString("CreatedBy", resourceCulture);
            }
        }
        
        public static string IncludeOrganizations {
            get {
                return ResourceManager.GetString("IncludeOrganizations", resourceCulture);
            }
        }
        
        public static string ManageOrganizations {
            get {
                return ResourceManager.GetString("ManageOrganizations", resourceCulture);
            }
        }
    }
}
