﻿using Cosette.Engine.Ai.Search;
using Cosette.Engine.Ai.Transposition;
using Cosette.Engine.Moves.Magic;
using Cosette.Interactive;

namespace Cosette
{
    public class Program
    {
        private static void Main()
        {
            TranspositionTable.Init(SearchConstants.DefaultHashTableSize);
            PawnHashTable.Init(SearchConstants.DefaultPawnHashTableSize);
            MagicBitboards.InitWithInternalKeys();

            new InteractiveConsole().Run();
        }
    }
}
