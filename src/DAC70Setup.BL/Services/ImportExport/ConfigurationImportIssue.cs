namespace DAC70Setup.BL.Services.ImportExport;

public record ConfigurationImportIssue(ConfigurationImportIssueKind Kind, string Panel, string Key, string Value);
