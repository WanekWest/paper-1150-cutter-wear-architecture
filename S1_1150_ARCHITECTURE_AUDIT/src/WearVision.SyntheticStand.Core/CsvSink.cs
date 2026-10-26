using System.Globalization;
using System.Text;

namespace WearVision.SyntheticStand.Core;

public sealed class CsvSink : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly string[] _headers;
    private bool _disposed;

    public CsvSink(string path, params string[] headers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _headers = headers.Length > 0 ? headers : throw new ArgumentException("CSV headers are required.", nameof(headers));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _writer = new StreamWriter(path, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        _writer.WriteLine(string.Join(",", _headers.Select(Escape)));
    }

    public long RowCount { get; private set; }

    public void Write(params object?[] values)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(CsvSink));
        }

        if (values.Length != _headers.Length)
        {
            throw new ArgumentException($"Expected {_headers.Length} values, received {values.Length}.");
        }

        _writer.WriteLine(string.Join(",", values.Select(Format)));
        RowCount++;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Dispose();
        _disposed = true;
    }

    private static string Format(object? value)
    {
        return value switch
        {
            null => string.Empty,
            double d => Escape(d.ToString("G17", CultureInfo.InvariantCulture)),
            float f => Escape(f.ToString("G9", CultureInfo.InvariantCulture)),
            decimal d => Escape(d.ToString(CultureInfo.InvariantCulture)),
            bool b => b ? "true" : "false",
            IFormattable formattable => Escape(formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty),
            _ => Escape(value.ToString() ?? string.Empty)
        };
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
