using System.Collections.ObjectModel;
using DAC70Setup.Core.Models;

namespace DAC70Setup.BL.Models;

public class DataModel : BindableBase
{
    public DataModel()
    {

    }

    public ObservableCollection<Configuration> Configurations { get; } = new ObservableCollection<Configuration>();
}
