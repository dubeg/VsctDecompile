using System;

namespace Microsoft.VisualStudio.CommandTable;

internal class CommandLineMessageProcessor : VSCTMessageProcessorBase {
    public CommandLineMessageProcessor(bool verbose) : base(verbose) {
    }

    public override void Error(int error, string file, int line, int pos, string message) {
        base.OnNewError();
        string text = base.BuildErrorString(VSCTMessageProcessorBase.ErrorKind.Error, error, file, line, pos, message);
        Console.WriteLine(text);
    }

    public override void Warning(int error, string file, int line, int pos, string message) {
        base.OnNewWarning();
        string text = base.BuildErrorString(VSCTMessageProcessorBase.ErrorKind.Warning, error, file, line, pos, message);
        Console.WriteLine(text);
    }

    public override void WriteLine(string format, params object[] arg) {
        Console.WriteLine(format, arg);
    }
}
