﻿using Cosette.Engine.Board;
using Cosette.Engine.Common;

namespace Cosette.Engine.Ai.Score.Evaluators
{
    public static class PositionEvaluator
    {
        public static int Evaluate(BoardState board, float openingPhase, float endingPhase)
        {
            var whitePositionScore = board.Position[Color.White][(int)GamePhase.Opening] * openingPhase +
                                     board.Position[Color.White][(int)GamePhase.Ending] * endingPhase;

            var blackPositionScore = board.Position[Color.Black][(int)GamePhase.Opening] * openingPhase +
                                     board.Position[Color.Black][(int)GamePhase.Ending] * endingPhase;

            return (int)whitePositionScore - (int)blackPositionScore;
        }
    }
}
