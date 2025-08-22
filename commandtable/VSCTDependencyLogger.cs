using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.VisualStudio.CommandTable;

internal class VSCTDependencyLogger {
    public string IntermediateDirectory { get; }

    public string ReadLogPath {
        get {
            return Path.Combine(this.IntermediateDirectory, "VSCT.read.1.tlog");
        }
    }

    public string WriteLogPath {
        get {
            return Path.Combine(this.IntermediateDirectory, "VSCT.write.1.tlog");
        }
    }

    public VSCTDependencyLogger(string intermediateDirectory, IMessageWriter messageWriter) {
        this.IntermediateDirectory = intermediateDirectory;
        this.messageWriter = messageWriter;
    }

    public bool IsCurrent(string sourceFile, string targetFile) {
        this.ReadDependencyLogs();
        sourceFile = VSCTDependencyLogger.Normalize(sourceFile);
        targetFile = VSCTDependencyLogger.Normalize(targetFile);
        if (!this.readLogs.ContainsKey(sourceFile) || !this.writeLogs.ContainsKey(sourceFile)) {
            return false;
        }
        if (!File.Exists(targetFile)) {
            this.FormatAndLogMessage(CommandTableSharedResources.TargetFileNotFound, new object[]
            {
                    targetFile
            });
            return false;
        }
        DateTime lastWriteTime = new FileInfo(targetFile).LastWriteTime;
        StringCollection stringCollection = this.writeLogs[sourceFile];
        StringCollection stringCollection2 = this.readLogs[sourceFile];
        if (stringCollection == null || stringCollection2 == null) {
            return false;
        }
        foreach (string text in stringCollection) {
            if (!File.Exists(text)) {
                this.FormatAndLogMessage(CommandTableSharedResources.TargetFileNotFound, new object[]
                {
                        text
                });
                return false;
            }
            FileInfo fileInfo = new FileInfo(text);
            if (fileInfo.LastWriteTime.CompareTo(lastWriteTime) < 0) {
                lastWriteTime = fileInfo.LastWriteTime;
            }
        }
        foreach (string text2 in stringCollection2) {
            if (!File.Exists(text2)) {
                this.FormatAndLogMessage(CommandTableSharedResources.SourceFileOutOfDate, new object[]
                {
                        text2
                });
                return false;
            }
            FileInfo fileInfo2 = new FileInfo(text2);
            if (fileInfo2.LastWriteTime.CompareTo(lastWriteTime) > 0) {
                this.FormatAndLogMessage(CommandTableSharedResources.TargetFileOutOfDate, new object[]
                {
                        text2
                });
                return false;
            }
        }
        this.FormatAndLogMessage(CommandTableSharedResources.SkipTaskExecution, Array.Empty<object>());
        return true;
    }

    internal void ReadDependencyLogs() {
        this.readLogs.Clear();
        this.writeLogs.Clear();
        if (File.Exists(this.ReadLogPath) && File.Exists(this.WriteLogPath)) {
            VSCTDependencyLogger.ReadFromFile(this.readLogs, this.ReadLogPath);
            VSCTDependencyLogger.ReadFromFile(this.writeLogs, this.WriteLogPath);
        }
    }

    private static void ReadFromFile(Dictionary<string, StringCollection> log, string file) {
        StringCollection stringCollection = new StringCollection();
        using (StreamReader streamReader = new StreamReader(file, Encoding.Unicode, false)) {
            while (!streamReader.EndOfStream) {
                string text = streamReader.ReadLine();
                if (text.StartsWith("^", StringComparison.Ordinal)) {
                    string key = text.Substring(1);
                    stringCollection = new StringCollection();
                    log[key] = stringCollection;
                }
                else {
                    stringCollection.Add(text);
                }
            }
        }
    }

    public void Save() {
        VSCTDependencyLogger.WriteLogToFile(this.readLogs, this.ReadLogPath);
        VSCTDependencyLogger.WriteLogToFile(this.writeLogs, this.WriteLogPath);
    }

    private static void WriteLogToFile(Dictionary<string, StringCollection> logs, string fileName) {
        if (logs != null) {
            using (StreamWriter streamWriter = new StreamWriter(fileName, false, Encoding.Unicode)) {
                foreach (string text in logs.Keys) {
                    streamWriter.Write("^");
                    streamWriter.WriteLine(text);
                    StringCollection stringCollection = logs[text];
                    foreach (string value in stringCollection) {
                        streamWriter.WriteLine(value);
                    }
                }
            }
        }
    }

    private void FormatAndLogMessage(string unformattedMessage, params object[] args) {
        IMessageWriter messageWriter = this.messageWriter;
        if (messageWriter == null) {
            return;
        }
        messageWriter.WriteLine(string.Format(CultureInfo.CurrentCulture, unformattedMessage, args));
    }

    public void AddReadDependency(string file, string dependency) {
        VSCTDependencyLogger.AddDependency(this.readLogs, file, dependency);
    }

    public void AddWriteDependency(string file, string dependency) {
        VSCTDependencyLogger.AddDependency(this.writeLogs, file, dependency);
    }

    private static void AddDependency(Dictionary<string, StringCollection> logs, string file, string dependency) {
        file = VSCTDependencyLogger.Normalize(file);
        dependency = VSCTDependencyLogger.Normalize(dependency);
        if (!logs.ContainsKey(file)) {
            logs[file] = new StringCollection();
        }
        logs[file].Add(dependency);
    }

    private static string Normalize(string fileName) {
        return fileName.ToUpper();
    }

    private const string ReadLogFileName = "VSCT.read.1.tlog";

    private const string WriteLogFileName = "VSCT.write.1.tlog";

    private Dictionary<string, StringCollection> readLogs = new Dictionary<string, StringCollection>(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, StringCollection> writeLogs = new Dictionary<string, StringCollection>(StringComparer.OrdinalIgnoreCase);

    private IMessageWriter messageWriter;
}
