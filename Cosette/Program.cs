﻿using System.Diagnostics;
using System.Threading;
using Cosette.Engine.Ai;
using Cosette.Engine.Moves;
using Cosette.Engine.Moves.Magic;
using Cosette.Interactive;

namespace Cosette
{
    public class Program
    {
        static void Main(string[] args)
        {
            TranspositionTable.Init();
            MagicBitboards.InitWithInternalKeys();

            new InteractiveConsole().Run();
        }
    }
}
