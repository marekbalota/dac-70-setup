using CommunityToolkit.Maui.Storage;
using DAC70Setup.BL.Models;

namespace DAC70Setup.BL.Services.ImportExport;

public class ConfigurationImportExportService
{
    private static readonly char[] InvalidFileNameCharacters = Path.GetInvalidFileNameChars();

    private readonly IFileSaver _fileSaver;
    private readonly YamlConfigurationSerializer _yamlSerializer;
    private readonly JsonConfigurationSerializer _jsonSerializer;

    public ConfigurationImportExportService(IFileSaver fileSaver,
        YamlConfigurationSerializer yamlSerializer,
        JsonConfigurationSerializer jsonSerializer)
    {
        _fileSaver = fileSaver;
        _yamlSerializer = yamlSerializer;
        _jsonSerializer = jsonSerializer;
    }

    public async Task<FileSaverResult> ExportAsync(Configuration configuration, ConfigurationExportFormat format)
    {
        IConfigurationSerializer serializer = format == ConfigurationExportFormat.Json ? _jsonSerializer : _yamlSerializer;

        using var stream = new MemoryStream();
        serializer.Write(stream, configuration.Settings);

        return await _fileSaver.SaveAsync(GetFileName(configuration.Name) + serializer.FileExtension, stream);
    }

    private static string GetFileName(string configurationName)
    {
        string fileName = string.Concat(configurationName.Select(character =>
            InvalidFileNameCharacters.Contains(character) ? '_' : character)).Trim();

        return fileName.Length > 0 ? fileName : "configuration";
    }
}
