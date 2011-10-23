
using System;

namespace DPReflections
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DPReflections game = new DPReflections())
            {
                game.Run();
            }
        }
    }
}

