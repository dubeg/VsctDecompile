using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudio.CommandTable;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CommandTableSharedResources {
    internal CommandTableSharedResources() {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager {
        get {
            if (CommandTableSharedResources.resourceMan == null) {
                ResourceManager resourceManager = new ResourceManager("Microsoft.VisualStudio.CommandTable.SharedResources", typeof(CommandTableSharedResources).Assembly);
                CommandTableSharedResources.resourceMan = resourceManager;
            }
            return CommandTableSharedResources.resourceMan;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture {
        get {
            return CommandTableSharedResources.resourceCulture;
        }
        set {
            CommandTableSharedResources.resourceCulture = value;
        }
    }

    internal static string SkipTaskExecution {
        get {
            return CommandTableSharedResources.ResourceManager.GetString("SkipTaskExecution", CommandTableSharedResources.resourceCulture);
        }
    }

    internal static string SourceFileOutOfDate {
        get {
            return CommandTableSharedResources.ResourceManager.GetString("SourceFileOutOfDate", CommandTableSharedResources.resourceCulture);
        }
    }

    internal static string TargetFileNotFound {
        get {
            return CommandTableSharedResources.ResourceManager.GetString("TargetFileNotFound", CommandTableSharedResources.resourceCulture);
        }
    }

    internal static string TargetFileOutOfDate {
        get {
            return CommandTableSharedResources.ResourceManager.GetString("TargetFileOutOfDate", CommandTableSharedResources.resourceCulture);
        }
    }

    private static ResourceManager resourceMan;

    private static CultureInfo resourceCulture;
}
