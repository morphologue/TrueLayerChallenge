using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WireMock.Handlers;

namespace Morphologue.Challenges.TrueLayer.Tests.Integration.TestHelpers;

internal class EmbeddedResourceFileSystemHandler : IFileSystemHandler
{
    private static readonly Assembly _assembly = typeof(EmbeddedResourceFileSystemHandler).Assembly;
    private static readonly string[] _allResourceNames = _assembly.GetManifestResourceNames();

    private readonly string _resourceNamePrefix;

    public EmbeddedResourceFileSystemHandler(string resourceSubdirectory)
    {
        _resourceNamePrefix = $"Morphologue.Challenges.TrueLayer.Tests.Integration.Resources.{resourceSubdirectory}.";
    }

    #region Unsupported operations
    public void CreateFolder(string path) => ThrowForWrite();
    public void DeleteFile(string filename) => ThrowForWrite();
    public void WriteFile(string filename, byte[] bytes) => ThrowForWrite();
    public void WriteMappingFile(string path, string text) => ThrowForWrite();
    public byte[] ReadResponseBodyAsFile(string path) => throw new NotImplementedException();
    public string ReadResponseBodyAsString(string path) => throw new NotImplementedException();
    private void ThrowForWrite() => throw new NotSupportedException("Embedded resources are read-only");
    #endregion

    public IEnumerable<string> EnumerateFiles(string path, bool includeSubdirectories) => _allResourceNames.Where(n => n.StartsWith(path));

    public bool FileExists(string fileName) => _allResourceNames.Contains(_resourceNamePrefix + fileName);

    public bool FolderExists(string path) => _allResourceNames.Any(n => n.StartsWith(path));

    public string GetMappingFolder() => _resourceNamePrefix;

    public byte[] ReadFile(string fileName)
    {
        using var memory = new MemoryStream();
        _assembly.GetManifestResourceStream(_resourceNamePrefix + fileName)?.CopyTo(memory);
        return memory.ToArray();
    }

    public string ReadFileAsString(string fileName)
    {
        var stream = _assembly.GetManifestResourceStream(fileName);
        if (stream == null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public string ReadMappingFile(string path) => ReadFileAsString(path);
}
