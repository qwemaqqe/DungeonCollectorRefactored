using DungeonCollectorGUI;

using System.Windows.Forms;


Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new GameForm());


namespace DungeonCollectorGUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }
}