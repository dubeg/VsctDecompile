using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Microsoft.VisualStudio.CommandTable;

public class Compiler : IMessageWriter {
    protected bool ValidateParameters(IMessageProcessor messageProcessor) {
        if (string.IsNullOrEmpty(this.SourceFile)) {
            this.MessageProcessor.Error(1, null, 0, 0, Resources.ErrNoSourceFile);
            return false;
        }
        if (!File.Exists(this.SourceFile)) {
            this.MessageProcessor.Error(2, this.SourceFile, 0, 0, Resources.ErrSourceNotFound);
            return false;
        }
        if (string.IsNullOrEmpty(this.OutputFile)) {
            this.OutputFile = Path.GetFileNameWithoutExtension(this.SourceFile) + ".cto";
        }
        if (string.IsNullOrEmpty(this.IntermediateDirectory)) {
            this.IntermediateDirectory = Path.GetDirectoryName(Path.GetFullPath(this.OutputFile));
            this.OutputFile = Path.GetFileName(this.OutputFile);
        }
        else if (!Path.IsPathRooted(this.IntermediateDirectory)) {
            this.IntermediateDirectory = Path.GetFullPath(this.IntermediateDirectory);
        }
        return true;
    }

    public string PreProcessor {
        get {
            return this.cppCommand;
        }
        set {
            this.cppCommand = value;
        }
    }

    public int CodePage {
        get {
            return this.codepage;
        }
        set {
            this.codepage = value;
        }
    }

    public bool CreateSymbolTable {
        get {
            return this.bCreateSymbolTable;
        }
        set {
            this.bCreateSymbolTable = value;
        }
    }

    public string Culture {
        get {
            return this.culture.Name;
        }
        set {
            this.culture = new CultureInfo(value);
        }
    }

    public CultureInfo CultureInfo {
        get {
            return this.culture;
        }
        set {
            this.culture = value;
        }
    }

    public string GeneratedCodeFile {
        get {
            return this.csFileName;
        }
        set {
            this.csFileName = value;
        }
    }

    public string GeneratedCodeLanguage {
        get {
            return this.emitLanguage;
        }
        set {
            this.emitLanguage = value;
        }
    }

    public string GeneratedCodeNamespace {
        get {
            return this.namespaceName;
        }
        set {
            this.namespaceName = value;
        }
    }

    public string GeneratedHeaderName {
        get {
            return this.headerFileName;
        }
        set {
            this.headerFileName = value;
        }
    }

    public string OutputFile {
        get {
            return this.outputName;
        }
        set {
            this.outputName = value;
        }
    }

    public string SourceFile {
        get {
            return this.inputName;
        }
        set {
            this.inputName = value;
        }
    }

    public VSCTMessageProcessorBase MessageProcessor {
        get {
            return this.messageProcessor;
        }
        set {
            this.messageProcessor = value;
        }
    }

    public string IntermediateDirectory {
        get {
            return this.intermediatesPath;
        }
        set {
            this.intermediatesPath = value;
        }
    }

    public string SymbolsFile {
        get {
            return this.outputSymbolsName;
        }
        set {
            this.outputSymbolsName = value;
        }
    }

    public bool Verbose {
        get {
            return this.bVerbose;
        }
        set {
            this.bVerbose = value;
        }
    }

    public bool EmitCS {
        get {
            return this.bEmitCSharp;
        }
        set {
            this.bEmitCSharp = value;
        }
    }

    public bool EmitHeader {
        get {
            return this.bEmitHeader;
        }
        set {
            this.bEmitHeader = value;
        }
    }

    public string[] Definitions {
        get {
            ArrayList arrayList = new ArrayList();
            foreach (string value in this.preprocessorDefines) {
                arrayList.Add(value);
            }
            return (string[])arrayList.ToArray(typeof(string));
        }
        set {
            this.preprocessorDefines = new StringCollection();
            this.preprocessorDefines.Clear();
            for (int i = 0; i < value.Length; i++) {
                string text = value[i];
                this.preprocessorDefines.Add(text);
            }
        }
    }

    public string[] AdditionalIncludeDirectories {
        get {
            return this.includePaths.ToArray();
        }
        set {
            if (value == null || value.Length == 0) {
                this.includePaths = new List<string>();
                return;
            }
            this.includePaths = new List<string>(value.Length);
            for (int i = 0; i < value.Length; i++) {
                string item = value[i];
                if (this.includePaths.BinarySearch(item, StringComparer.OrdinalIgnoreCase) < 0) {
                    this.includePaths.Add(item);
                }
            }
        }
    }

    public bool SaveDependencyInformation { get; set; }

    public bool Compile() {
        if (this.MessageProcessor == null) {
            this.MessageProcessor = new CommandLineMessageProcessor(this.Verbose);
        }
        if (this.SaveDependencyInformation) {
            this.MessageProcessor.OnNewDependency += this.DependencyAdded;
        }
        if (!this.ValidateParameters(this.messageProcessor)) {
            return false;
        }
        try {
            bool flag = false;
            try {
                using (StreamReader streamReader = File.OpenText(this.inputName)) {
                    string text = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(text)) {
                        return true;
                    }
                    if (text.StartsWith("<?xml", StringComparison.Ordinal) || text.StartsWith("<CommandTable", StringComparison.Ordinal)) {
                        flag = true;
                    }
                }
            }
            catch (FileNotFoundException ex) {
                this.messageProcessor.Error(0, null, 0, 0, ex.Message);
                return false;
            }
            if ((this.bCreateSymbolTable || this.bEmitCSharp || this.bEmitHeader) && string.IsNullOrEmpty(this.outputSymbolsName)) {
                if (flag) {
                    this.outputSymbolsName = this.inputName;
                }
                else {
                    this.outputSymbolsName = Path.ChangeExtension(this.inputName, ".ctsym");
                }
            }
            this.ct = new CommandTable();
            if (this.includePaths != null && this.includePaths.Count > 0) {
                this.ct.AddAdditionalIncludes(this.includePaths.ToArray());
            }
            if (this.preprocessorDefines != null && this.preprocessorDefines.Count > 0) {
                this.ct.AddAdditionalPreprocessorDefines(this.preprocessorDefines);
            }
            if (!string.IsNullOrEmpty(this.cppCommand)) {
                this.ct.PreprocessorPath = Path.GetFullPath(this.cppCommand);
            }
            this.ct.CodePage = this.codepage;
            if (flag) {
                string currentDirectory = Environment.CurrentDirectory;
                string fullPath = Path.GetFullPath(this.inputName);
                string fileName = Path.Combine(this.IntermediateDirectory, this.outputName);
                Environment.CurrentDirectory = Path.GetDirectoryName(fullPath);
                if (this.ct.Read(fullPath, this.messageProcessor) && this.messageProcessor.ErrorsCount == 0) {
                    this.ct.Save(fileName, new SaveOptions(SaveOptions.SaveFormat.BIN, this.culture), this.messageProcessor);
                }
                Environment.CurrentDirectory = currentDirectory;
            }
            else if (this.inputName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || this.inputName.EndsWith(".cto", StringComparison.OrdinalIgnoreCase)) {
                if (this.ct.Read(this.inputName, this.messageProcessor) && this.messageProcessor.ErrorsCount == 0) {
                    string text2 = Path.ChangeExtension(this.inputName, ".ctsym");
                    string[] array = new string[]
                    {
                            text2,
                            "Symbols.vsct",
                            "ctc.ctsym",
                            "devenv.ctsym"
                    };
                    foreach (string text3 in array) {
                        if (File.Exists(text3)) {
                            this.ct.ImportSymbols(text3, null, null);
                        }
                    }
                    if (!this.ct.ContainsSymbols) {
                        string message = string.Format(Resources.Culture, Resources.ErrDecompileWithoutSymbols, this.inputName, text2, "ctc.ctsym");
                        this.messageProcessor.Error(0, null, 0, 0, message);
                    }
                    this.ct.Save(this.outputName, new SaveOptions(SaveOptions.SaveFormat.XML), this.messageProcessor);
                }
            }
            else {
                if (this.bCreateSymbolTable || this.bEmitCSharp || this.bEmitHeader) {
                    this.MessageProcessor.WriteLine(Resources.ProcessingSymbolsToTarget, new object[]
                    {
                            this.inputName,
                            Path.GetFullPath(this.outputSymbolsName)
                    });
                    string currentDirectory2 = Environment.CurrentDirectory;
                    SymbolTableCompiler symbolTableCompiler = new SymbolTableCompiler();
                    symbolTableCompiler.ErrorOut = this.messageProcessor;
                    StringCollection stringCollection = new StringCollection();
                    stringCollection.AddRange(this.includePaths.ToArray());
                    symbolTableCompiler.Compile(this.inputName, this.outputSymbolsName, this.cppCommand, stringCollection, this.preprocessorDefines);
                    Environment.CurrentDirectory = currentDirectory2;
                }
                string currentDirectory3 = Environment.CurrentDirectory;
                CommandTableCompiler commandTableCompiler = new CommandTableCompiler();
                commandTableCompiler.ErrorOut = this.messageProcessor;
                StringCollection stringCollection2 = new StringCollection();
                stringCollection2.AddRange(this.includePaths.ToArray());
                this.ct = commandTableCompiler.Compile(this.inputName, this.codepage, this.outputName, this.cppCommand, stringCollection2, this.preprocessorDefines, this.outputSymbolsName);
                Environment.CurrentDirectory = currentDirectory3;
            }
            if (this.bEmitCSharp && this.ct != null) {
                if (string.IsNullOrEmpty(this.emitLanguage)) {
                    this.emitLanguage = "C#";
                }
                try {
                    using (CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(this.emitLanguage)) {
                        this.ct.EmitSymbols(this.outputSymbolsName, this.csFileName, this.namespaceName, codeDomProvider);
                    }
                }
                catch (ConfigurationErrorsException) {
                    this.messageProcessor.WriteLine(Resources.CantCreateCodeDOM, new object[]
                    {
                            this.emitLanguage
                    });
                    this.messageProcessor.WriteLine(Resources.CodeFileNotGenerated, new object[]
                    {
                            this.csFileName
                    });
                }
            }
            if (this.bEmitHeader && this.ct != null) {
                this.ct.EmitHeaderSymbols(null, Path.Combine(this.intermediatesPath, this.headerFileName));
            }
            this.messageProcessor.WriteLine(string.Empty, Array.Empty<object>());
            this.messageProcessor.WriteLine(Resources.TotalErrorsSummary, new object[]
            {
                    this.messageProcessor.ErrorsCount
            });
            this.messageProcessor.WriteLine(Resources.TotalWarningsSummary, new object[]
            {
                    this.messageProcessor.WarningsCount
            });
        }
        catch (ArgumentException e) {
            this.MessageProcessor.ErrorFromException(e);
            return false;
        }
        catch (IOException e2) {
            this.MessageProcessor.ErrorFromException(e2);
            return false;
        }
        bool flag2 = this.MessageProcessor.ErrorsCount == 0;
        if (flag2 && this.SaveDependencyInformation && !string.IsNullOrEmpty(this.IntermediateDirectory)) {
            this.WriteDependencyInformation();
        }
        return flag2;
    }

    private void DependencyAdded(string fileName, IMessageProcessor messageProcessor) {
        this.reportedDependencies.Add(fileName);
    }

    private void WriteDependencyInformation() {
        VSCTDependencyLogger vsctdependencyLogger = new VSCTDependencyLogger(this.IntermediateDirectory, this);
        vsctdependencyLogger.ReadDependencyLogs();
        string fullPath = Path.GetFullPath(this.inputName);
        vsctdependencyLogger.AddReadDependency(fullPath, fullPath);
        foreach (string dependency in this.reportedDependencies) {
            vsctdependencyLogger.AddReadDependency(fullPath, dependency);
        }
        vsctdependencyLogger.AddWriteDependency(fullPath, Path.Combine(this.IntermediateDirectory, this.OutputFile));
        if (this.bEmitCSharp) {
            vsctdependencyLogger.AddWriteDependency(fullPath, Path.Combine(this.IntermediateDirectory, this.GeneratedCodeFile));
        }
        if (this.bEmitHeader) {
            vsctdependencyLogger.AddWriteDependency(fullPath, Path.Combine(this.IntermediateDirectory, this.GeneratedHeaderName));
        }
        vsctdependencyLogger.Save();
    }

    void IMessageWriter.WriteLine(string message) {
        VSCTMessageProcessorBase vsctmessageProcessorBase = this.messageProcessor;
        if (vsctmessageProcessorBase == null) {
            return;
        }
        vsctmessageProcessorBase.WriteLine(message, Array.Empty<object>());
    }

    private VSCTMessageProcessorBase messageProcessor;

    private string inputName;

    private string outputName;

    private string outputSymbolsName;

    private string csFileName;

    private string emitLanguage;

    private string headerFileName;

    private string namespaceName;

    private string intermediatesPath;

    private bool bVerbose;

    private bool bEmitCSharp;

    private bool bEmitHeader;

    private bool bCreateSymbolTable;

    private List<string> includePaths;

    private StringCollection preprocessorDefines;

    private string cppCommand;

    private int codepage;

    private CommandTable ct;

    private CultureInfo culture = CultureInfo.CurrentCulture;

    private HashSet<string> reportedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
