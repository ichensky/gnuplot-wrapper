using System.Diagnostics;

namespace GnuPlotWrapper;

public class GnuPlot : IDisposable 
{
    private const string GnuPlotExecutable = "gnuplot";

    private bool disposed = false;

    private Process? process;

    /// <summary>
    /// Starts the GnuPlot process.
    /// </summary>
    public void Start()
    {
        if (process != null)
        {
            throw new InvalidOperationException("GnuPlot process is already started.");
        }

        process = StartProcess();
    }

    /// <summary>
    /// Executes the given GnuPlot script asynchronously.
    /// </summary>
    /// <param name="script">
    /// Example:
    ///     var script = " plot sin(x) ";
    ///     .. .ExecuteAsync(script.AsMemory());
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task ExecuteAsync(ReadOnlyMemory<char> script, CancellationToken cancellationToken = default)
    {
        if (process == null)
        {
            throw new InvalidOperationException("GnuPlot process is not started. Call Start() method first.");
        }

        if (process.HasExited)
        {
            throw new InvalidOperationException("GnuPlot process has already exited.");
        }

        await process.StandardInput.WriteLineAsync(script, cancellationToken).ConfigureAwait(false);
        await process.StandardInput.FlushAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Kills the GnuPlot process asynchronously.
    /// </summary>
    public async Task KillAndWaitForExitAsync(CancellationToken cancellationToken = default)
    {
        if (process == null || process.HasExited)
        {
            return;
        }
 
        await ExecuteAsync("exit".AsMemory(), cancellationToken).ConfigureAwait(false);

        process.Kill(true);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the GnuPlotWrapper.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed state (managed objects).
            if(process != null)
            {
                process.StandardInput?.Dispose();
                process?.Dispose();
                process = null;
            }
        }

        // Free unmanaged resources.

        disposed = true;
    }

    private static Process StartProcess()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GnuPlotExecutable,
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            } 
        };

        process.Start();
        process.StandardInput.AutoFlush = false;

        return process;
    }
}