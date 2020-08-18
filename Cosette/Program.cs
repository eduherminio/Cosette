﻿using Cosette.Engine.Moves.Simple;
using Cosette.Interactive;

namespace Cosette
{
    public class Program
    {
        static void Main(string[] args)
        {
            KingMovesGenerator.Init();
            KnightMovesGenerator.Init();

            new InteractiveConsole().Run();
        }
    }
}
