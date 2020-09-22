﻿using Cosette.Engine.Ai.Score;
using Cosette.Engine.Common;

namespace Cosette.Engine.Ai.Ordering
{
    public static class StaticExchangeEvaluation
    {
        private static short[][][][] _table;

        static StaticExchangeEvaluation()
        {
            InitTable();
            PopulateTable();
        }

        public static short Evaluate(int attackingPiece, int capturedPiece, int attacker, int defender)
        {
            return _table[attackingPiece][capturedPiece][attacker][defender];
        }

        private static void InitTable()
        {
            _table = new short[6][][][];
            for (var attackingPieceIndex = 0; attackingPieceIndex < 6; attackingPieceIndex++)
            {
                _table[attackingPieceIndex] = new short[6][][];
                for (var capturedPieceIndex = 0; capturedPieceIndex < 6; capturedPieceIndex++)
                {
                    _table[attackingPieceIndex][capturedPieceIndex] = new short[64][];
                    for (var attackerIndex = 0; attackerIndex < 64; attackerIndex++)
                    {
                        _table[attackingPieceIndex][capturedPieceIndex][attackerIndex] = new short[64];
                    }
                }
            }
        }

        private static void PopulateTable()
        {
            for (var attackingPieceIndex = 0; attackingPieceIndex < 6; attackingPieceIndex++)
            {
                for (var capturedPieceIndex = 0; capturedPieceIndex < 6; capturedPieceIndex++)
                {
                    var attackingPiece = attackingPieceIndex;
                    var capturedPiece = capturedPieceIndex;

                    for (ulong attackerIndex = 0; attackerIndex < 64; attackerIndex++)
                    {
                        for (ulong defenderIndex = 0; defenderIndex < 64; defenderIndex++)
                        {
                            var attackers = attackerIndex & ~(1ul << attackingPieceIndex);
                            var defenders = defenderIndex;

                            var currentPieceOnField = attackingPiece;
                            var result = EvaluationConstants.Pieces[capturedPiece];

                            if (defenders != 0)
                            {
                                var leastValuableDefenderField = BitOperations.GetLsb(defenders);
                                var leastValuableDefenderPiece = BitOperations.BitScan(leastValuableDefenderField);
                                defenders = BitOperations.PopLsb(defenders);

                                result -= EvaluationConstants.Pieces[currentPieceOnField];
                                currentPieceOnField = leastValuableDefenderPiece;

                                while (attackers != 0)
                                {
                                    var updatedResult = result;

                                    var leastValuableAttackerField = BitOperations.GetLsb(attackers);
                                    var leastValuableAttackerPiece = BitOperations.BitScan(leastValuableAttackerField);
                                    attackers = BitOperations.PopLsb(attackers);

                                    updatedResult += EvaluationConstants.Pieces[currentPieceOnField];
                                    currentPieceOnField = leastValuableAttackerPiece;

                                    if (defenders != 0)
                                    {
                                        leastValuableDefenderField = BitOperations.GetLsb(defenders);
                                        leastValuableDefenderPiece = BitOperations.BitScan(leastValuableDefenderField);
                                        defenders = BitOperations.PopLsb(defenders);

                                        updatedResult -= EvaluationConstants.Pieces[currentPieceOnField];
                                        currentPieceOnField = leastValuableDefenderPiece;
                                    }
                                    else
                                    {
                                        result = updatedResult;
                                        break;
                                    }

                                    if (updatedResult < result)
                                    {
                                        break;
                                    }
                                }
                            }

                            _table[attackingPieceIndex][capturedPieceIndex][attackerIndex][defenderIndex] = (short)result;
                        }
                    }
                }
            }
        }
    }
}
