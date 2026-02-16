using DAC70Setup.BL.Models;

namespace DAC70Setup.BL.Services;

public class ManageConfigurationsService
{
    private readonly DataModel _dataModel;

    public ManageConfigurationsService(DataModel dataModel)
    {
        _dataModel = dataModel;
    }

    public Task CreateNewConfiguration(string name)
    {
        return Task.CompletedTask;
    }
}
