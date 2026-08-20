using SQLite;

namespace DAC70Setup.Data;

[Table("Settings")]
public class SettingRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public Guid ConfigId { get; set; }

    public byte MessageType { get; set; }

    public byte SettingId { get; set; }

    public byte[] ValueBytes { get; set; } = Array.Empty<byte>();
}
