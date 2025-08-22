using System;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.CommandTable;

[Serializable]
internal class CommandLineException : Exception {
    public CommandLineException(string message) : base(message) {
    }

    protected CommandLineException(SerializationInfo info, StreamingContext context) : base(info, context) {
    }
}
