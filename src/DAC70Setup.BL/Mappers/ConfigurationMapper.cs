using DAC70Setup.BL.Models;
using DAC70Setup.Core.Models;
using DAC70Setup.Core.Models.Values;

namespace DAC70Setup.BL.Mappers;

public class ConfigurationMapper
{
    private readonly SettingMapper _settingMapper;

    public ConfigurationMapper(SettingMapper settingMapper)
    {
        _settingMapper = settingMapper;
    }

    public Configuration Map(ConfigurationDto dto)
    {
        var configuration = new Configuration(dto.Id, dto.Name, dto.Created);

        foreach (var setting in dto.Settings)
        {
            Dac70SettingValue? value = _settingMapper.Map(setting);

            if (value != null)
            {
                configuration.Settings.Add(value);
            }
        }

        return configuration;
    }
}
