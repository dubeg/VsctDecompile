using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Microsoft.VisualStudio.CommandTable;

public static class CommandLine {
    private static List<CommandLine.CommandLineParameter> ParseCommandLine(string[] args) {
        List<CommandLine.CommandLineParameter> list = new List<CommandLine.CommandLineParameter>();
        if (args == null) {
            return list;
        }
        foreach (string text in args) {
            bool flag = false;
            CommandLine.CommandLineParameter[] knownParameters = CommandLine.KnownParameters;
            int j = 0;
            while (j < knownParameters.Length) {
                CommandLine.CommandLineParameter commandLineParameter = knownParameters[j];
                if (commandLineParameter.Parse(text)) {
                    flag = true;
                    if (!list.Contains(commandLineParameter)) {
                        list.Add(commandLineParameter);
                        break;
                    }
                    if (CommandLine.CommandLineParameter.ValueKind.Single == commandLineParameter.Kind) {
                        string message = string.Format(Resources.Culture, Resources.CmdLineError_DuplicatedParam, commandLineParameter.Name);
                        throw new CommandLineException(message);
                    }
                    break;
                }
                else {
                    j++;
                }
            }
            if (!flag) {
                string message2 = string.Format(Resources.Culture, Resources.CmdLineError_UnknownParam, text);
                throw new CommandLineException(message2);
            }
        }
        return list;
    }

    private static void ValidateParameters(List<CommandLine.CommandLineParameter> parameters) {
        if (!parameters.Contains(CommandLine.NamelessParameters)) {
            throw new CommandLineException(Resources.CmdLineError_NoSource);
        }
        if (CommandLine.NamelessParameters.Values.Count > 2) {
            string message = string.Format(Resources.Culture, Resources.CmdLineError_TooManyInput, CommandLine.NamelessParameters.Values[2], CommandLine.NamelessParameters.Values[0], CommandLine.NamelessParameters.Values[1]);
            throw new CommandLineException(message);
        }
        if (parameters.Contains(CommandLine.EmitLanguageParameter)) {
            if (!parameters.Contains(CommandLine.EmitNamespaceParameter)) {
                throw new CommandLineException(Resources.CmdLineError_NoNamespace);
            }
            if (!parameters.Contains(CommandLine.EmitFileParameter)) {
                throw new CommandLineException(Resources.CmdLineError_NoCodeFile);
            }
        }
    }

    private static void PrintHelp() {
        Console.Out.WriteLine(CommandLine.CopyRightMessage);
        Console.Out.WriteLine();
        Console.Out.WriteLine(Resources.CmdLineHelp_Syntax);
        Console.Out.WriteLine();
        foreach (CommandLine.CommandLineParameter commandLineParameter in CommandLine.KnownParameters) {
            string helpString = commandLineParameter.HelpString;
            if (!string.IsNullOrEmpty(helpString)) {
                Console.Out.WriteLine(helpString);
            }
        }
        Console.Out.WriteLine();
    }

    //public static int Main(string[] args) {
    //    int result = 0;
    //    try {
    //        if (args.Length == 1 && args[0].Length > 0 && args[0][0] == '@') {
    //            args = File.ReadAllLines(args[0].Substring(1));
    //        }
    //        List<CommandLine.CommandLineParameter> list = CommandLine.ParseCommandLine(args);
    //        if (list.Contains(CommandLine.HelpParameter)) {
    //            CommandLine.PrintHelp();
    //            return 0;
    //        }
    //        CommandLine.ValidateParameters(list);
    //        Compiler compiler = new Compiler();
    //        compiler.SourceFile = CommandLine.NamelessParameters.Values[0];
    //        if (CommandLine.NamelessParameters.Values.Count > 1) {
    //            compiler.OutputFile = CommandLine.NamelessParameters.Values[1];
    //        }
    //        foreach (CommandLine.CommandLineParameter commandLineParameter in list) {
    //            commandLineParameter.Apply(compiler);
    //        }
    //        if (!list.Contains(CommandLine.NoLogoParameter)) {
    //            Console.Out.WriteLine(CommandLine.CopyRightMessage);
    //        }
    //        if (!compiler.Compile()) {
    //            result = 1;
    //        }
    //    }
    //    catch (Exception ex) {
    //        result = 1;
    //        Console.WriteLine(ex.Message);
    //    }
    //    return result;
    //}

    private static string CopyRightMessage {
        get {
            return string.Format(CultureInfo.CurrentUICulture, Resources.Copyright, FileVersionInfo.GetVersionInfo(CommandLine.ExecutingAssemblyPath).FileVersion.ToString());
        }
    }

    private static string ExecutingAssemblyPath {
        get {
            string result;
            try {
                result = Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);
            }
            catch (InvalidOperationException) {
                result = Assembly.GetExecutingAssembly().Location;
            }
            return result;
        }
    }

    private static readonly char[] startParameterChars = new char[]
    {
        '-',
        '/'
    };

    private static CommandLine.CommandLineParameter NamelessParameters = new CommandLine.PositionalParameter("NamelessParameter", null);

    private static CommandLine.CommandLineParameter VerboseParameter = new CommandLine.SimpleParameter("verbose", delegate (Compiler comp, CommandLine.CommandLineParameter p) {
        comp.Verbose = true;
    }, "CmdLineHelp_V") {
        AllowSubstring = true
    };

    private static CommandLine.CommandLineParameter HelpParameter = new CommandLine.SimpleParameter("?", null, "CmdLineHelp_Help");

    private static CommandLine.CommandLineParameter NoLogoParameter = new CommandLine.SimpleParameter("nologo", null, "CmdLineHelp_N", false) {
        AllowSubstring = true
    };

    private static CommandLine.CommandLineParameter CleanParameter = new CommandLine.SimpleParameter("clean", null, "CmdHelp_Clean") {
        AllowSubstring = true
    };

    private static CommandLine.CommandLineParameter DefinesParameter = new CommandLine.ParameterWithData("D", CommandLine.CommandLineParameter.ValueKind.Multiple, delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.Definitions = p.Values.ToArray();
    }, "CmdLineHelp_D");

    private static CommandLine.CommandLineParameter IncludesParameter = new CommandLine.ParameterWithData("I", CommandLine.CommandLineParameter.ValueKind.Multiple, delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.AdditionalIncludeDirectories = p.Values.ToArray();
    }, "CmdLineHelp_I");

    private static CommandLine.CommandLineParameter CppCommandParameter = new CommandLine.ParameterWithData("C", CommandLine.CommandLineParameter.ValueKind.Single, delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.PreProcessor = p.Values[0];
    }, null);

    private static CommandLine.CultureParameter LanguageParameter = new CommandLine.CultureParameter("L", delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.CultureInfo = ((CommandLine.CultureParameter)p).Culture;
    }, "CmdLineHelp_L");

    private static CommandLine.CommandLineParameter SymbolsParameter = new CommandLine.ParameterWithData("S", CommandLine.CommandLineParameter.ValueKind.Single, delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.CreateSymbolTable = true;
        c.GeneratedHeaderName = p.Values[0];
    }, null);

    private static CommandLine.CommandLineParameter EmitHeaderParameter = new CommandLine.ColonDelimitedParameter("EH", delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.EmitHeader = true;
        c.GeneratedHeaderName = p.Values[0];
    }, "CmdLineHelp_EH");

    private static CommandLine.CommandLineParameter EmitLanguageParameter = new CommandLine.ColonDelimitedParameter("EL", delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.EmitCS = true;
        c.GeneratedCodeLanguage = p.Values[0];
    }, "CmdLineHelp_EL");

    private static CommandLine.CommandLineParameter EmitNamespaceParameter = new CommandLine.ColonDelimitedParameter("EN", delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.GeneratedCodeNamespace = p.Values[0];
    }, "CmdLineHelp_EN");

    private static CommandLine.CommandLineParameter EmitFileParameter = new CommandLine.ColonDelimitedParameter("EF", delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.GeneratedCodeFile = p.Values[0];
    }, "CmdLineHelp_EF");

    private static CommandLine.CommandLineParameter IntermediateDirectoryCommandParameter = new CommandLine.ParameterWithData("X", CommandLine.CommandLineParameter.ValueKind.Single, delegate (Compiler c, CommandLine.CommandLineParameter p) {
        c.IntermediateDirectory = p.Values[0];
    }, null);

    private static CommandLine.SimpleParameter LogDependenciesParameter = new CommandLine.SimpleParameter("depends", delegate (Compiler comp, CommandLine.CommandLineParameter p) {
        comp.SaveDependencyInformation = true;
    }, null) {
        AllowSubstring = false
    };

    private static readonly CommandLine.CommandLineParameter[] KnownParameters = new CommandLine.CommandLineParameter[]
    {
        CommandLine.NamelessParameters,
        CommandLine.VerboseParameter,
        CommandLine.HelpParameter,
        CommandLine.NoLogoParameter,
        CommandLine.CleanParameter,
        CommandLine.DefinesParameter,
        CommandLine.IncludesParameter,
        CommandLine.CppCommandParameter,
        CommandLine.LanguageParameter,
        CommandLine.SymbolsParameter,
        CommandLine.EmitHeaderParameter,
        CommandLine.EmitLanguageParameter,
        CommandLine.EmitNamespaceParameter,
        CommandLine.EmitFileParameter,
        CommandLine.IntermediateDirectoryCommandParameter,
        CommandLine.LogDependenciesParameter
    };

    private delegate void ParameterApplyFunction(Compiler compiler, CommandLine.CommandLineParameter param);

    private abstract class CommandLineParameter {
        protected CommandLineParameter(string name, CommandLine.CommandLineParameter.ValueKind kind, CommandLine.ParameterApplyFunction applyFunction, string helpResource) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException(name);
            }
            this.name = name;
            this.AllowSubstring = false;
            this.valueKind = kind;
            this.helpResource = helpResource;
            this.applyHandler = applyFunction;
            this.values = new List<string>();
        }

        public abstract bool Parse(string rawParameter);

        protected virtual void AddValue(string value) {
            if (this.values.Count > 0 && this.valueKind != CommandLine.CommandLineParameter.ValueKind.Multiple) {
                string message = string.Format(Resources.Culture, Resources.CmdLineError_DuplicatedParam, this.Name);
                throw new CommandLineException(message);
            }
            this.values.Add(value);
        }

        public void Apply(Compiler compiler) {
            if (this.applyHandler != null) {
                this.applyHandler(compiler, this);
            }
        }

        public string HelpString {
            get {
                if (string.IsNullOrEmpty(this.helpResource)) {
                    return string.Empty;
                }
                return Resources.ResourceManager.GetString(this.helpResource, Resources.Culture);
            }
        }

        public string Name {
            get {
                return this.name;
            }
        }

        public bool AllowSubstring { get; set; }

        public CommandLine.CommandLineParameter.ValueKind Kind {
            get {
                return this.valueKind;
            }
        }

        public List<string> Values {
            get {
                return this.values;
            }
        }

        private string name;

        private List<string> values;

        private CommandLine.CommandLineParameter.ValueKind valueKind;

        private string helpResource;

        private CommandLine.ParameterApplyFunction applyHandler;

        public enum ValueKind {
            None,
            Single,
            Multiple
        }
    }

    private class PositionalParameter : CommandLine.CommandLineParameter {
        public PositionalParameter(string name, CommandLine.ParameterApplyFunction applyFunction) : base(name, CommandLine.CommandLineParameter.ValueKind.Multiple, applyFunction, null) {
        }

        public override bool Parse(string rawParameter) {
            if (string.IsNullOrEmpty(rawParameter)) {
                return false;
            }
            foreach (char c in CommandLine.startParameterChars) {
                if (rawParameter[0] == c) {
                    return false;
                }
            }
            this.AddValue(rawParameter);
            return true;
        }
    }

    private class SimpleParameter : CommandLine.CommandLineParameter {
        public SimpleParameter(string name, CommandLine.ParameterApplyFunction applyFunction, string helpResource) : this(name, applyFunction, helpResource, true) {
        }

        public SimpleParameter(string name, CommandLine.ParameterApplyFunction applyFunction, string helpResource, bool caseSensitive) : base(name, CommandLine.CommandLineParameter.ValueKind.Single, applyFunction, helpResource) {
            this.comparison = (caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Parse(string rawParameter) {
            if (string.IsNullOrEmpty(rawParameter)) {
                return false;
            }
            bool flag = false;
            foreach (char c in CommandLine.startParameterChars) {
                if (rawParameter[0] == c) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return false;
            }
            string text = rawParameter.Substring(1);
            return string.Compare(base.Name, 0, rawParameter.Substring(1), 0, base.AllowSubstring ? text.Length : base.Name.Length, this.comparison) == 0;
        }

        private StringComparison comparison;
    }

    private class ParameterWithData : CommandLine.CommandLineParameter {
        public ParameterWithData(string name, CommandLine.CommandLineParameter.ValueKind kind, CommandLine.ParameterApplyFunction applyFunction, string helpResource) : base(name, kind, applyFunction, helpResource) {
        }

        public override bool Parse(string rawParameter) {
            if (string.IsNullOrEmpty(rawParameter)) {
                return false;
            }
            bool flag = false;
            foreach (char c in CommandLine.startParameterChars) {
                if (rawParameter[0] == c) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return false;
            }
            if (rawParameter.Length < base.Name.Length + 1) {
                return false;
            }
            string strA = rawParameter.Substring(1, base.Name.Length);
            string value = rawParameter.Substring(base.Name.Length + 1);
            if (string.Compare(strA, base.Name, StringComparison.CurrentCulture) != 0) {
                return false;
            }
            if (string.IsNullOrEmpty(value)) {
                string message = string.Format(Resources.Culture, Resources.CmdLineError_MissingParamData, base.Name);
                throw new CommandLineException(message);
            }
            this.AddValue(value);
            return true;
        }
    }

    private class CultureParameter : CommandLine.ParameterWithData {
        public CultureParameter(string name, CommandLine.ParameterApplyFunction applyFunction, string helpResource) : base(name, CommandLine.CommandLineParameter.ValueKind.Single, applyFunction, helpResource) {
        }

        protected override void AddValue(string value) {
            if (char.IsDigit(value[0])) {
                int num = 0;
                try {
                    if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
                        num = int.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture);
                    }
                    else {
                        num = int.Parse(value, CultureInfo.CurrentCulture);
                    }
                }
                catch (FormatException) {
                    string message = string.Format(Resources.Culture, Resources.CmdLineError_InvalidCodepage, value);
                    throw new CommandLineException(message);
                }
                this.culture = new CultureInfo(num);
            }
            else {
                try {
                    this.culture = new CultureInfo(value);
                }
                catch (ArgumentException) {
                    string message2 = string.Format(Resources.Culture, Resources.CmdLineError_InvalidCulture, value);
                    throw new CommandLineException(message2);
                }
            }
            base.AddValue(value);
        }

        public CultureInfo Culture {
            get {
                return this.culture;
            }
        }

        private CultureInfo culture;
    }

    private class ColonDelimitedParameter : CommandLine.CommandLineParameter {
        public ColonDelimitedParameter(string name, CommandLine.ParameterApplyFunction applyFunction, string helpResource) : base(name, CommandLine.CommandLineParameter.ValueKind.Single, applyFunction, helpResource) {
        }

        public override bool Parse(string rawParameter) {
            if (string.IsNullOrEmpty(rawParameter)) {
                return false;
            }
            bool flag = false;
            foreach (char c in CommandLine.startParameterChars) {
                if (rawParameter[0] == c) {
                    flag = true;
                    break;
                }
            }
            if (!flag) {
                return false;
            }
            int num = rawParameter.IndexOf(':');
            if (num <= 0) {
                return false;
            }
            string strA = rawParameter.Substring(1, num - 1);
            if (string.Compare(strA, base.Name, StringComparison.CurrentCulture) != 0) {
                return false;
            }
            if (num != rawParameter.Length - 1) {
                string value = rawParameter.Substring(num + 1);
                this.AddValue(value);
            }
            return true;
        }
    }
}
