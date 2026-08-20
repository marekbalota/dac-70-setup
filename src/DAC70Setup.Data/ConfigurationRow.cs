using SQLite;

namespace DAC70Setup.Data;

[Table("Configurations")]
public class ConfigurationRow
{
    [PrimaryKey]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime Created { get; set; }
}
