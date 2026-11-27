using DAC70Setup.Core.Models.Definitions;
using DAC70Setup.Core.Models.Protocol;

namespace DAC70Setup.Tests.Core.Definitions;

public class Dac70SettingCatalogTests
{
    [Fact]
    public void Get_UnknownKey_Throws()
    {
        Assert.Throws<KeyNotFoundException>(() => Dac70SettingCatalog.Get(new SettingKey(MessageType.C0, 0xFF)));
    }

    [Fact]
    public void TryGet_UnknownKey_ReturnsFalse()
    {
        bool found = Dac70SettingCatalog.TryGet(new SettingKey(MessageType.C1, 0x01), out var definition);

        Assert.False(found);
        Assert.Null(definition);
    }
}