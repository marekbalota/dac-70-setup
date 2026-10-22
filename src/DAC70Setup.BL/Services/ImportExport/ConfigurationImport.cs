using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Services.ImportExport;

public record ConfigurationImport(IReadOnlyList<Dac70SettingValue> Values, IReadOnlyList<ConfigurationImportIssue> Issues);
