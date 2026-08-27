using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Mappers;

public class SettingMapper
{
    public Dac70SettingValue? Map(SettingDto dto)
    {
        if (!Dac70SettingCatalog.TryGet(dto.Key, out Dac70SettingDefinition? definition))
        {
            return null;
        }

        try
        {
            return definition!.CreateValue(dto.ValueBytes);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
