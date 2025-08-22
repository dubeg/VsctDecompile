using Microsoft.VisualStudio.CommandTable;
using System;
using System.Collections.Generic;

namespace VsctDecompile;

internal class SimpleMessageProcessor : IMessageProcessor {
    public List<string> Errors { get; set; } = new();

    public void Dependency(string file) {
        // Handle dependency messages here
        Console.WriteLine($"Dependency found: {file}");
    }

    public void Error(int error, string file, int line, int pos, string message) {
        // Handle error messages here
        Errors.Add(message);
        Console.WriteLine($"Error {error} in file '{file}' at line {line}, position {pos}: {message}");
    }

    public bool VerboseOutput() {
        // Return true if verbose output is enabled
        return true;
    }

    public void Warning(int error, string file, int line, int pos, string message) {
        // Handle warning messages here
        Console.WriteLine($"Warning {error} in file '{file}' at line {line}, position {pos}: {message}");
    }

    public void WriteLine(string format, params object[] arg) {
        // Handle general messages here
        if (arg != null && arg.Length > 0) {
            Console.WriteLine(format, arg);
        } else {
            Console.WriteLine(format);
        }
    }
}
