using System;
using System.Windows.Forms;

namespace TheaterTicketSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Уверете се, че тук пише точно Form1
            Application.Run(new Form1());
        }
    }
}
