using Microsoft.VisualStudio.CommandTable;
using System;
using System.IO;

namespace VsctDecompile; 

internal class Program {
    static void Main(string[] args) {
        var sourceFile = args[0];
        if (string.IsNullOrWhiteSpace(sourceFile)) {
            Console.WriteLine("Source file is required.");
            return;
        }
        if (!File.Exists(sourceFile)) {
            Console.WriteLine($"Source file '{sourceFile}' does not exist.");
            return;
        }
        if (Path.GetExtension(sourceFile).ToLowerInvariant() != ".dll") {
            Console.WriteLine("Source file must be a dll.");
            return;
        }

        var ct = new CommandTable();
        var messageProcessor = new SimpleMessageProcessor();
        if (ct.Read(sourceFile, messageProcessor) && messageProcessor.Errors.Count == 0) {
            var sourceFileAsCtSym = Path.ChangeExtension(sourceFile, ".ctsym");
            ct.ImportSymbols(sourceFileAsCtSym, null, null);
            if (!ct.ContainsSymbols) {
                var message = $"No symbols found in '{sourceFileAsCtSym}'.";
                messageProcessor.Error(0, null, 0, 0, message);
            }
            // Save to disk:
            // var outputName = Path.ChangeExtension(sourceFile, ".vsct");
            // ct.Save(outputName, new SaveOptions(SaveOptions.SaveFormat.XML), messageProcessor);
        }
    }
}
