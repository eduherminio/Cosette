﻿using Cosette.Engine.Board;
using Cosette.Engine.Board.Operators;
using Cosette.Engine.Common;

namespace Cosette.Engine.Ai.Score.Evaluators
{
    public static class MobilityEvaluator
    {
#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Evaluate(BoardState board, float openingPhase, float endingPhase)
        {
            return Evaluate(board, Color.White, openingPhase, endingPhase) - Evaluate(board, Color.Black, openingPhase, endingPhase);
        }

#if INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int Evaluate(BoardState board, Color color, float openingPhase, float endingPhase)
        {
            var mobility = KnightOperator.GetMobility(board, color) + BishopOperator.GetMobility(board, color) +
                           RookOperator.GetMobility(board, color) + QueenOperator.GetMobility(board, color);
            
            return (int)(mobility * EvaluationConstants.Mobility[(int)GamePhase.Opening] * openingPhase +
                         mobility * EvaluationConstants.Mobility[(int)GamePhase.Ending] * endingPhase);
        }
    }
}
