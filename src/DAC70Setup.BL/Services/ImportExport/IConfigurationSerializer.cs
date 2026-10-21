using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.ImportExport;

public interface IConfigurationSerializer
{
    const string HorizontalPosition = "Horizontal Position";
    const string VerticalPosition = "Vertical Position";
    const string HorizontalSize = "Horizontal Size";
    const string VerticalSize = "Vertical Size";

    string FileExtension { get; }

    void Write(Stream stream, IReadOnlyList<Dac70SettingValue> values);
}
