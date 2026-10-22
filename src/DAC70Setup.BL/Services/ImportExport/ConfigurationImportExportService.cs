using CommunityToolkit.Maui.Storage;
using DAC70Setup.BL.Models;

namespace DAC70Setup.BL.Services.ImportExport;

public class ConfigurationImportExportService
{
    private const int MaximumFileLength = 1024 * 1024;

    private static readonly char[] InvalidFileNameCharacters = Path.GetInvalidFileNameChars();

    private readonly IFileSaver _fileSaver;
    private readonly IFilePicker _filePicker;
    private readonly YamlConfigurationSerializer _yamlSerializer;
    private readonly JsonConfigurationSerializer _jsonSerializer;

    public ConfigurationImportExportService(IFileSaver fileSaver,
        IFilePicker filePicker,
        YamlConfigurationSerializer yamlSerializer,
        JsonConfigurationSerializer jsonSerializer)
    {
        _fileSaver = fileSaver;
        _filePicker = filePicker;
        _yamlSerializer = yamlSerializer;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<FileSaverResult> ExportAsync(Configuration configuration, ConfigurationExportFormat format)
    {
        ConfigurationSerializer serializer = format == ConfigurationExportFormat.Json ? _jsonSerializer : _yamlSerializer;

        using var stream = new MemoryStream();
        serializer.Write(stream, configuration.Settings);

        return await _fileSaver.SaveAsync(GetFileName(configuration.Name) + serializer.FileExtension, stream);
    }

    public Task<FileResult?> PickFileAsync()
    {
        var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = [".yaml", ".yml", ".json"],
            [DevicePlatform.Android] = ["*/*"],
            [DevicePlatform.iOS] = ["public.yaml", "public.json"],
            [DevicePlatform.MacCatalyst] = ["public.yaml", "public.json"]
        });

        return _filePicker.PickAsync(new PickOptions { FileTypes = fileTypes });
    }

    public async Task<ConfigurationImport> ImportAsync(FileResult file)
    {
        var buffer = new byte[MaximumFileLength + 1];
        int length;

        await using (Stream stream = await file.OpenReadAsync())
        {
            length = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false);
        }

        if (length > MaximumFileLength)
        {
            throw new InvalidDataException("The file is too large to be a DAC-70 configuration.");
        }

        using var content = new MemoryStream(buffer, 0, length, writable: false);

        ConfigurationSerializer serializer = Path.GetExtension(file.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase)
            ? _jsonSerializer
            : _yamlSerializer;

        return serializer.Read(content);
    }

    private static string GetFileName(string configurationName)
    {
        string fileName = string.Concat(configurationName.Select(character =>
            InvalidFileNameCharacters.Contains(character) ? '_' : character)).Trim();

        return fileName.Length > 0 ? fileName : "configuration";
    }
}
