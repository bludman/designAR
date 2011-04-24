using System;

namespace designAR
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (designAR game = new designAR())
            {
                game.Run();
            }
        }
    }
}

