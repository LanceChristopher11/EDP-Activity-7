using ConcertEventSystemUI;

namespace EDP_Act4_GUI_FIXED;
static class Program
{
    // The main entry point for the application
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm());
    }    
}