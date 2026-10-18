using DAC70Setup.BL;

namespace DAC70Setup;

public partial class MainWindow : Window
{
    public MainWindow(AppShell shell)
    {
        InitializeComponent();

        Page = shell;
    }
}
