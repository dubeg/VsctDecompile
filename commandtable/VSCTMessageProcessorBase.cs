using System;
using System.Globalization;

namespace Microsoft.VisualStudio.CommandTable;

public abstract class VSCTMessageProcessorBase : IMessageProcessor {
    protected VSCTMessageProcessorBase(bool verbose) {
        this.verbose = verbose;
    }

    public event DependencyAddedHandler OnNewDependency {
        add {
            this.onNewDependency = (DependencyAddedHandler)Delegate.Combine(this.onNewDependency, value);
        }
        remove {
            this.onNewDependency = (DependencyAddedHandler)Delegate.Remove(this.onNewDependency, value);
        }
    }

    public int ErrorsCount {
        get {
            return this.errorsCount;
        }
    }

    public int WarningsCount {
        get {
            return this.warningsCount;
        }
    }

    private string ErrorKindName(VSCTMessageProcessorBase.ErrorKind kind) {
        string result = string.Empty;
        if (kind != VSCTMessageProcessorBase.ErrorKind.Error) {
            if (kind == VSCTMessageProcessorBase.ErrorKind.Warning) {
                result = Resources.ErrorKindWarning;
            }
        }
        else {
            result = Resources.ErrorKindError;
        }
        return result;
    }

    protected string BuildErrorString(VSCTMessageProcessorBase.ErrorKind kind, int error, string file, int line, int pos, string message) {
        string result = string.Empty;
        if (string.IsNullOrEmpty(file)) {
            result = string.Format(Resources.Culture, Resources.VSCTShortErrorFormat, this.ErrorKindName(kind), this.ErrorCodeString(error), message);
        }
        else {
            result = string.Format(Resources.Culture, Resources.VSCTLongErrorFormat, new object[]
            {
                    file,
                    line,
                    pos,
                    this.ErrorKindName(kind),
                    this.ErrorCodeString(error),
                    message
            });
        }
        return result;
    }

    protected string ErrorCodeString(int error) {
        if (error == 0) {
            return string.Empty;
        }
        return string.Format(CultureInfo.CurrentUICulture, Resources.VSCTErrorCodeFormat, error);
    }

    protected void OnNewError() {
        if (this.errorsCount < 2147483647) {
            this.errorsCount++;
        }
    }

    protected void OnNewWarning() {
        if (this.warningsCount < 2147483647) {
            this.warningsCount++;
        }
    }

    public void ErrorFromException(Exception e) {
        this.Error(0, null, 0, 0, e.Message);
    }

    public abstract void Error(int error, string file, int line, int pos, string message);

    public abstract void Warning(int error, string file, int line, int pos, string message);

    public void Dependency(string file) {
        if (this.onNewDependency != null) {
            this.onNewDependency(file, this);
        }
    }

    public abstract void WriteLine(string format, params object[] arg);

    public bool VerboseOutput() {
        return this.verbose;
    }

    private bool verbose;

    private DependencyAddedHandler onNewDependency;

    private int errorsCount;

    private int warningsCount;

    protected enum ErrorKind {
        Error,
        Warning
    }
}
