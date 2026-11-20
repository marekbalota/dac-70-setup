namespace DAC70Setup.Core.Serialization;

public record ConfigurationImportIssue(ConfigurationImportIssueKind Kind, string Panel, string Key, string Value);
