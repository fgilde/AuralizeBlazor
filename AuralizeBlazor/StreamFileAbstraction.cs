using System.IO;

namespace AuralizeBlazor;

public class StreamFileAbstraction : TagLib.File.IFileAbstraction
{
    private readonly string _name;
    private readonly Stream _readStream;
    private readonly Stream _writeStream;

    public StreamFileAbstraction(string name, Stream readStream, Stream writeStream)
    {
        _name = name;
        _readStream = readStream;
        _writeStream = writeStream;
    }

    public string Name => _name;
    public Stream ReadStream => _readStream;
    public Stream WriteStream => _writeStream;

    public void CloseStream(Stream stream)
    {
        // Do not close the stream, it is managed outside
    }
}