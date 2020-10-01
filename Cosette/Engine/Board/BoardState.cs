﻿using System;
using Cosette.Engine.Ai.Score;
using Cosette.Engine.Ai.Score.PieceSquareTables;
using Cosette.Engine.Board.Operators;
using Cosette.Engine.Common;
using Cosette.Engine.Fen;
using Cosette.Engine.Moves;

namespace Cosette.Engine.Board
{
    public class BoardState
    {
        public ulong[][] Pieces { get; set; }
        public ulong[] Occupancy { get; set; }
        public ulong OccupancySummary { get; set; }
        public ulong EnPassant { get; set; }
        public Castling Castling { get; set; }
        public int ColorToMove { get; set; }
        public int MovesCount { get; set; }
        public int IrreversibleMovesCount { get; set; }
        public int NullMoves { get; set; }

        public bool[] CastlingDone { get; set; }
        public int[] Material { get; set; }
        public int[][] Position { get; set; }

        public int[] PieceTable { get; set; }

        public ulong Hash { get; set; }
        public ulong PawnHash { get; set; }

        private readonly FastStack<int> _killedPieces;
        private readonly FastStack<ulong> _enPassants;
        private readonly FastStack<Castling> _castlings;
        private readonly FastStack<int> _promotedPieces;
        private readonly FastStack<ulong> _hashes;
        private readonly FastStack<ulong> _pawnHashes;
        private readonly FastStack<int> _irreversibleMovesCounts;

        private readonly int _materialAtOpening;

        public BoardState()
        {
            Pieces = new ulong[2][];
            Pieces[Color.White] = new ulong[6];
            Pieces[Color.Black] = new ulong[6];

            Occupancy = new ulong[2];
            CastlingDone = new bool[2];
            Material = new int[2];

            Position = new int[2][];
            Position[Color.White] = new int[2];
            Position[Color.Black] = new int[2];

            PieceTable = new int[64];

            _killedPieces = new FastStack<int>(512);
            _enPassants = new FastStack<ulong>(512);
            _castlings = new FastStack<Castling>(512);
            _promotedPieces = new FastStack<int>(512);
            _hashes = new FastStack<ulong>(512);
            _pawnHashes = new FastStack<ulong>(512);
            _irreversibleMovesCounts = new FastStack<int>(512);

            _materialAtOpening =
                EvaluationConstants.Pieces[Piece.King] +
                EvaluationConstants.Pieces[Piece.Queen] +
                EvaluationConstants.Pieces[Piece.Rook] * 2 +
                EvaluationConstants.Pieces[Piece.Bishop] * 2 +
                EvaluationConstants.Pieces[Piece.Knight] * 2 +
                EvaluationConstants.Pieces[Piece.Pawn] * 8;
        }

        public void SetDefaultState()
        {
            Pieces[Color.White][Piece.Pawn] = 65280;
            Pieces[Color.White][Piece.Rook] = 129;
            Pieces[Color.White][Piece.Knight] = 66;
            Pieces[Color.White][Piece.Bishop] = 36;
            Pieces[Color.White][Piece.Queen] = 16;
            Pieces[Color.White][Piece.King] = 8;

            Pieces[Color.Black][Piece.Pawn] = 71776119061217280;
            Pieces[Color.Black][Piece.Rook] = 9295429630892703744;
            Pieces[Color.Black][Piece.Knight] = 4755801206503243776;
            Pieces[Color.Black][Piece.Bishop] = 2594073385365405696;
            Pieces[Color.Black][Piece.Queen] = 1152921504606846976;
            Pieces[Color.Black][Piece.King] = 576460752303423488;

            Occupancy[Color.White] = 65535;
            Occupancy[Color.Black] = 18446462598732840960;
            OccupancySummary = Occupancy[Color.White] | Occupancy[Color.Black];

            EnPassant = 0;
            Castling = Castling.Everything;
            ColorToMove = Color.White;
            MovesCount = 0;
            IrreversibleMovesCount = 0;
            NullMoves = 0;

            CastlingDone[Color.White] = false;
            CastlingDone[Color.Black] = false;

            Material[Color.White] = CalculateMaterial(Color.White);
            Material[Color.Black] = CalculateMaterial(Color.Black);

            Position[Color.White][GamePhase.Opening] = CalculatePosition(Color.White, GamePhase.Opening);
            Position[Color.White][GamePhase.Ending] = CalculatePosition(Color.White, GamePhase.Ending);
            Position[Color.Black][GamePhase.Opening] = CalculatePosition(Color.Black, GamePhase.Opening);
            Position[Color.Black][GamePhase.Ending] = CalculatePosition(Color.Black, GamePhase.Ending);

            Array.Fill(PieceTable, -1);

            PieceTable[0] = Piece.Rook;
            PieceTable[1] = Piece.Knight;
            PieceTable[2] = Piece.Bishop;
            PieceTable[3] = Piece.King;
            PieceTable[4] = Piece.Queen;
            PieceTable[5] = Piece.Bishop;
            PieceTable[6] = Piece.Knight;
            PieceTable[7] = Piece.Rook;

            PieceTable[8] = Piece.Pawn;
            PieceTable[9] = Piece.Pawn;
            PieceTable[10] = Piece.Pawn;
            PieceTable[11] = Piece.Pawn;
            PieceTable[12] = Piece.Pawn;
            PieceTable[13] = Piece.Pawn;
            PieceTable[14] = Piece.Pawn;
            PieceTable[15] = Piece.Pawn;

            PieceTable[48] = Piece.Pawn;
            PieceTable[49] = Piece.Pawn;
            PieceTable[50] = Piece.Pawn;
            PieceTable[51] = Piece.Pawn;
            PieceTable[52] = Piece.Pawn;
            PieceTable[53] = Piece.Pawn;
            PieceTable[54] = Piece.Pawn;
            PieceTable[55] = Piece.Pawn;

            PieceTable[56] = Piece.Rook;
            PieceTable[57] = Piece.Knight;
            PieceTable[58] = Piece.Bishop;
            PieceTable[59] = Piece.King;
            PieceTable[60] = Piece.Queen;
            PieceTable[61] = Piece.Bishop;
            PieceTable[62] = Piece.Knight;
            PieceTable[63] = Piece.Rook;

            Hash = ZobristHashing.CalculateHash(this);
            PawnHash = ZobristHashing.CalculatePawnHash(this);

            _killedPieces.Clear();
            _enPassants.Clear();
            _castlings.Clear();
            _promotedPieces.Clear();
            _hashes.Clear();
            _pawnHashes.Clear();
            _irreversibleMovesCounts.Clear();
        }

        public int GetAvailableMoves(Span<Move> moves)
        {
            var movesCount = PawnOperator.GetAvailableMoves(this, ColorToMove, moves, 0);
            movesCount = KnightOperator.GetAvailableMoves(this, ColorToMove, moves, movesCount);
            movesCount = BishopOperator.GetAvailableMoves(this, ColorToMove, moves, movesCount);
            movesCount = RookOperator.GetAvailableMoves(this, ColorToMove, moves, movesCount);
            movesCount = QueenOperator.GetAvailableMoves(this, ColorToMove, moves, movesCount);
            movesCount = KingOperator.GetAvailableMoves(this, ColorToMove, moves, movesCount);

            return movesCount;
        }

        public int GetAvailableQMoves(Span<Move> moves)
        {
            var movesCount = PawnOperator.GetAvailableQMoves(this, ColorToMove, moves, 0);
            movesCount = KnightOperator.GetAvailableQMoves(this, ColorToMove, moves, movesCount);
            movesCount = BishopOperator.GetAvailableQMoves(this, ColorToMove, moves, movesCount);
            movesCount = RookOperator.GetAvailableQMoves(this, ColorToMove, moves, movesCount);
            movesCount = QueenOperator.GetAvailableQMoves(this, ColorToMove, moves, movesCount);
            movesCount = KingOperator.GetAvailableQMoves(this, ColorToMove, moves, movesCount);

            return movesCount;
        }

        public void MakeMove(Move move)
        {
            var pieceType = PieceTable[move.From];
            var enemyColor = ColorOperations.Invert(ColorToMove);

            if (ColorToMove == Color.White)
            {
                MovesCount++;
            }

            _castlings.Push(Castling);
            _hashes.Push(Hash);
            _pawnHashes.Push(PawnHash);
            _enPassants.Push(EnPassant);
            _irreversibleMovesCounts.Push(IrreversibleMovesCount);

            if (pieceType == Piece.Pawn || ((byte)move.Flags & MoveFlagFields.Capture) != 0)
            {
                IrreversibleMovesCount = 0;
            }
            else
            {
                IrreversibleMovesCount++;
            }

            if (EnPassant != 0)
            {
                var enPassantRank = BitOperations.BitScan(EnPassant) % 8;
                Hash = ZobristHashing.ToggleEnPassant(Hash, enPassantRank);
                EnPassant = 0;
            }

            if (move.Flags == MoveFlags.Quiet)
            {
                MovePiece(ColorToMove, pieceType, move.From, move.To);
                Hash = ZobristHashing.MovePiece(Hash, ColorToMove, pieceType, move.From, move.To);

                if (pieceType == Piece.Pawn)
                {
                    PawnHash = ZobristHashing.MovePiece(PawnHash, ColorToMove, pieceType, move.From, move.To);
                }
            }
            else if (move.Flags == MoveFlags.DoublePush)
            {
                MovePiece(ColorToMove, pieceType, move.From, move.To);
                Hash = ZobristHashing.MovePiece(Hash, ColorToMove, pieceType, move.From, move.To);
                PawnHash = ZobristHashing.MovePiece(PawnHash, ColorToMove, pieceType, move.From, move.To);

                var enPassantField = ColorToMove == Color.White ? 1ul << move.To - 8 : 1ul << move.To + 8;
                var enPassantFieldIndex = BitOperations.BitScan(enPassantField);

                EnPassant |= enPassantField;
                Hash = ZobristHashing.ToggleEnPassant(Hash, enPassantFieldIndex % 8);
            }
            else if (move.Flags == MoveFlags.EnPassant)
            {
                var enemyPieceField = ColorToMove == Color.White ? (byte)(move.To - 8) : (byte)(move.To + 8);
                var killedPiece = PieceTable[enemyPieceField];

                RemovePiece(enemyColor, killedPiece, enemyPieceField);
                Hash = ZobristHashing.AddOrRemovePiece(Hash, enemyColor, killedPiece, enemyPieceField);
                PawnHash = ZobristHashing.AddOrRemovePiece(PawnHash, enemyColor, killedPiece, enemyPieceField);

                MovePiece(ColorToMove, pieceType, move.From, move.To);
                Hash = ZobristHashing.MovePiece(Hash, ColorToMove, pieceType, move.From, move.To);
                PawnHash = ZobristHashing.MovePiece(PawnHash, ColorToMove, pieceType, move.From, move.To);

                _killedPieces.Push(killedPiece);
            }
            else if (((byte)move.Flags & MoveFlagFields.Capture) != 0)
            {
                var killedPiece = PieceTable[move.To];

                RemovePiece(enemyColor, killedPiece, move.To);
                Hash = ZobristHashing.AddOrRemovePiece(Hash, enemyColor, killedPiece, move.To);

                if (killedPiece == Piece.Pawn)
                {
                    PawnHash = ZobristHashing.AddOrRemovePiece(PawnHash, enemyColor, killedPiece, move.To);
                }
                else if (killedPiece == Piece.Rook)
                {
                    switch (move.To)
                    {
                        case 0:
                        {
                            Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteShort);
                            Castling &= ~Castling.WhiteShort;
                            break;
                        }
                        case 7:
                        {
                            Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteLong);
                            Castling &= ~Castling.WhiteLong;
                            break;
                        }
                        case 56:
                        {
                            Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackShort);
                            Castling &= ~Castling.BlackShort;
                            break;
                        }
                        case 63:
                        {
                            Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackLong);
                            Castling &= ~Castling.BlackLong;
                            break;
                        }
                    }
                }

                // Promotion
                if (((byte)move.Flags & MoveFlagFields.Promotion) != 0)
                {
                    var promotionPiece = GetPromotionPiece(move.Flags);

                    RemovePiece(ColorToMove, pieceType, move.From);
                    Hash = ZobristHashing.AddOrRemovePiece(Hash, ColorToMove, pieceType, move.From);
                    PawnHash = ZobristHashing.AddOrRemovePiece(PawnHash, ColorToMove, pieceType, move.From);

                    AddPiece(ColorToMove, promotionPiece, move.To);
                    Hash = ZobristHashing.AddOrRemovePiece(Hash, ColorToMove, promotionPiece, move.To);

                    _promotedPieces.Push(promotionPiece);
                }
                else
                {
                    MovePiece(ColorToMove, pieceType, move.From, move.To);
                    Hash = ZobristHashing.MovePiece(Hash, ColorToMove, pieceType, move.From, move.To);

                    if (pieceType == Piece.Pawn)
                    {
                        PawnHash = ZobristHashing.MovePiece(PawnHash, ColorToMove, pieceType, move.From, move.To);
                    }
                }

                _killedPieces.Push(killedPiece);
            }
            else if (move.Flags == MoveFlags.KingCastle || move.Flags == MoveFlags.QueenCastle)
            {
                // Short castling
                if (move.Flags == MoveFlags.KingCastle)
                {
                    if (ColorToMove == Color.White)
                    {
                        MovePiece(Color.White, Piece.King, 3, 1);
                        MovePiece(Color.White, Piece.Rook, 0, 2);

                        Hash = ZobristHashing.MovePiece(Hash, Color.White, Piece.King, 3, 1);
                        Hash = ZobristHashing.MovePiece(Hash, Color.White, Piece.Rook, 0, 2);
                    }
                    else
                    {
                        MovePiece(Color.Black, Piece.King, 59, 57);
                        MovePiece(Color.Black, Piece.Rook, 56, 58);

                        Hash = ZobristHashing.MovePiece(Hash, Color.Black, Piece.King, 59, 57);
                        Hash = ZobristHashing.MovePiece(Hash, Color.Black, Piece.Rook, 56, 58);
                    }
                }
                // Long castling
                else
                {
                    if (ColorToMove == Color.White)
                    {
                        MovePiece(Color.White, Piece.King, 3, 5);
                        MovePiece(Color.White, Piece.Rook, 7, 4);

                        Hash = ZobristHashing.MovePiece(Hash, Color.White, Piece.King, 3, 5);
                        Hash = ZobristHashing.MovePiece(Hash, Color.White, Piece.Rook, 7, 4);
                    }
                    else
                    {
                        MovePiece(Color.Black, Piece.King, 59, 61);
                        MovePiece(Color.Black, Piece.Rook, 63, 60);

                        Hash = ZobristHashing.MovePiece(Hash, Color.Black, Piece.King, 59, 61);
                        Hash = ZobristHashing.MovePiece(Hash, Color.Black, Piece.Rook, 63, 60);
                    }
                }

                if (ColorToMove == Color.White)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteShort);
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteLong);
                    Castling &= ~Castling.WhiteCastling;
                }
                else
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackShort);
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackLong);
                    Castling &= ~Castling.BlackCastling;
                }

                CastlingDone[ColorToMove] = true;
            }
            else if (((byte)move.Flags & MoveFlagFields.Promotion) != 0)
            {
                var promotionPiece = GetPromotionPiece(move.Flags);

                RemovePiece(ColorToMove, pieceType, move.From);
                Hash = ZobristHashing.AddOrRemovePiece(Hash, ColorToMove, pieceType, move.From);
                PawnHash = ZobristHashing.AddOrRemovePiece(PawnHash, ColorToMove, pieceType, move.From);

                AddPiece(ColorToMove, promotionPiece, move.To);
                Hash = ZobristHashing.AddOrRemovePiece(Hash, ColorToMove, promotionPiece, move.To);

                _promotedPieces.Push(promotionPiece);
            }

            if (pieceType == Piece.King && move.Flags != MoveFlags.KingCastle && move.Flags != MoveFlags.QueenCastle)
            {
                if (ColorToMove == Color.White)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteShort);
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteLong);
                    Castling &= ~Castling.WhiteCastling;
                }
                else
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackShort);
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackLong);
                    Castling &= ~Castling.BlackCastling;
                }
            }
            else if (pieceType == Piece.Rook && Castling != 0)
            {
                if (move.From == 0)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteShort);
                    Castling &= ~Castling.WhiteShort;
                }
                else if (move.From == 7)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.WhiteLong);
                    Castling &= ~Castling.WhiteLong;
                }
                else if (move.From == 56)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackShort);
                    Castling &= ~Castling.BlackShort;
                }
                else if (move.From == 63)
                {
                    Hash = ZobristHashing.RemoveCastlingFlag(Hash, Castling, Castling.BlackLong);
                    Castling &= ~Castling.BlackLong;
                }
            }

            ColorToMove = enemyColor;
            Hash = ZobristHashing.ChangeSide(Hash);
        }

        public void UndoMove(Move move)
        {
            var pieceType = PieceTable[move.To];
            ColorToMove = ColorOperations.Invert(ColorToMove);

            if (move.Flags == MoveFlags.Quiet || move.Flags == MoveFlags.DoublePush)
            {
                MovePiece(ColorToMove, pieceType, move.To, move.From);
            }
            else if (move.Flags == MoveFlags.EnPassant)
            {
                var enemyColor = ColorOperations.Invert(ColorToMove);
                var enemyPieceField = ColorToMove == Color.White ? (byte)(move.To - 8) : (byte)(move.To + 8);
                var killedPiece = _killedPieces.Pop();

                MovePiece(ColorToMove, Piece.Pawn, move.To, move.From);
                AddPiece(enemyColor, killedPiece, enemyPieceField);
            }
            else if (((byte)move.Flags & MoveFlagFields.Capture) != 0)
            {
                var enemyColor = ColorOperations.Invert(ColorToMove);
                var killedPiece = _killedPieces.Pop();

                // Promotion
                if (((byte)move.Flags & MoveFlagFields.Promotion) != 0)
                {
                    var promotionPiece = _promotedPieces.Pop();
                    RemovePiece(ColorToMove, promotionPiece, move.To);
                    AddPiece(ColorToMove, Piece.Pawn, move.From);
                }
                else
                {
                    MovePiece(ColorToMove, pieceType, move.To, move.From);
                }

                AddPiece(enemyColor, killedPiece, move.To);
            }
            else if (move.Flags == MoveFlags.KingCastle || move.Flags == MoveFlags.QueenCastle)
            {
                // Short castling
                if (move.Flags == MoveFlags.KingCastle)
                {
                    if (ColorToMove == Color.White)
                    {
                        MovePiece(Color.White, Piece.King, 1, 3);
                        MovePiece(Color.White, Piece.Rook, 2, 0);
                    }
                    else
                    {
                        MovePiece(Color.Black, Piece.King, 57, 59);
                        MovePiece(Color.Black, Piece.Rook, 58, 56);
                    }
                }
                // Long castling
                else
                {
                    if (ColorToMove == Color.White)
                    {
                        MovePiece(Color.White, Piece.King, 5, 3);
                        MovePiece(Color.White, Piece.Rook, 4, 7);
                    }
                    else
                    {
                        MovePiece(Color.Black, Piece.King, 61, 59);
                        MovePiece(Color.Black, Piece.Rook, 60, 63);
                    }
                }

                CastlingDone[ColorToMove] = false;
            }
            else if (((byte)move.Flags & MoveFlagFields.Promotion) != 0)
            {
                var promotionPiece = _promotedPieces.Pop();
                RemovePiece(ColorToMove, promotionPiece, move.To);
                AddPiece(ColorToMove, Piece.Pawn, move.From);
            }

            IrreversibleMovesCount = _irreversibleMovesCounts.Pop();
            PawnHash = _pawnHashes.Pop();
            Hash = _hashes.Pop();
            Castling = _castlings.Pop();
            EnPassant = _enPassants.Pop();

            if (ColorToMove == Color.White)
            {
                MovesCount--;
            }
        }

        public void MakeNullMove()
        {
            NullMoves++;
            if (ColorToMove == Color.White)
            {
                MovesCount++;
            }

            _enPassants.Push(EnPassant);
            _hashes.Push(Hash);

            if (EnPassant != 0)
            {
                var enPassantRank = BitOperations.BitScan(EnPassant) % 8;
                Hash = ZobristHashing.ToggleEnPassant(Hash, enPassantRank);
                EnPassant = 0;
            }

            ColorToMove = ColorOperations.Invert(ColorToMove);
            Hash = ZobristHashing.ChangeSide(Hash);
        }

        public void UndoNullMove()
        {
            NullMoves--;
            ColorToMove = ColorOperations.Invert(ColorToMove);

            Hash = _hashes.Pop();
            EnPassant = _enPassants.Pop();

            if (ColorToMove == Color.White)
            {
                MovesCount--;
            }
        }

        public bool IsFieldAttacked(int color, int fieldIndex)
        {
            var enemyColor = ColorOperations.Invert(color);

            var fileRankAttacks = RookMovesGenerator.GetMoves(OccupancySummary, fieldIndex) & Occupancy[enemyColor];
            var attackingRooks = fileRankAttacks & (Pieces[enemyColor][Piece.Rook] | Pieces[enemyColor][Piece.Queen]);
            if (attackingRooks != 0)
            {
                return true;
            }

            var diagonalAttacks = BishopMovesGenerator.GetMoves(OccupancySummary, fieldIndex) & Occupancy[enemyColor];
            var attackingBishops = diagonalAttacks & (Pieces[enemyColor][Piece.Bishop] | Pieces[enemyColor][Piece.Queen]);
            if (attackingBishops != 0)
            {
                return true;
            }

            var jumpAttacks = KnightMovesGenerator.GetMoves(fieldIndex);
            var attackingKnights = jumpAttacks & Pieces[enemyColor][Piece.Knight];
            if (attackingKnights != 0)
            {
                return true;
            }

            var boxAttacks = KingMovesGenerator.GetMoves(fieldIndex);
            var attackingKings = boxAttacks & Pieces[enemyColor][Piece.King];
            if (attackingKings != 0)
            {
                return true;
            }

            var field = 1ul << fieldIndex;
            var potentialPawns = boxAttacks & Pieces[enemyColor][Piece.Pawn];
            var attackingPawns = color == Color.White ?
                field & ((potentialPawns >> 7) | (potentialPawns >> 9)) :
                field & ((potentialPawns << 7) | (potentialPawns << 9));

            if (attackingPawns != 0)
            {
                return true;
            }

            return false;
        }

        public byte GetAttackingPiecesWithColor(int color, int fieldIndex)
        {
            byte result = 0;

            var fileRankAttacks = RookMovesGenerator.GetMoves(OccupancySummary, fieldIndex) & Occupancy[color];
            var attackingRooks = fileRankAttacks & Pieces[color][Piece.Rook];
            if (attackingRooks != 0)
            {
                result |= 1 << Piece.Rook;
            }

            var diagonalAttacks = BishopMovesGenerator.GetMoves(OccupancySummary, fieldIndex) & Occupancy[color];
            var attackingBishops = diagonalAttacks & Pieces[color][Piece.Bishop];
            if (attackingBishops != 0)
            {
                result |= 1 << Piece.Bishop;
            }

            var attackingQueens = (fileRankAttacks | diagonalAttacks) & Pieces[color][Piece.Queen];
            if (attackingQueens != 0)
            {
                result |= 1 << Piece.Queen;
            }

            var jumpAttacks = KnightMovesGenerator.GetMoves(fieldIndex);
            var attackingKnights = jumpAttacks & Pieces[color][Piece.Knight];
            if (attackingKnights != 0)
            {
                result |= 1 << Piece.Knight;
            }

            var boxAttacks = KingMovesGenerator.GetMoves(fieldIndex);
            var attackingKings = boxAttacks & Pieces[color][Piece.King];
            if (attackingKings != 0)
            {
                result |= 1 << Piece.King;
            }

            var field = 1ul << fieldIndex;
            var potentialPawns = boxAttacks & Pieces[color][Piece.Pawn];
            var attackingPawns = color == Color.White ?
                field & ((potentialPawns << 7) | (potentialPawns << 9)) :
                field & ((potentialPawns >> 7) | (potentialPawns >> 9));

            if (attackingPawns != 0)
            {
                result |= 1 << Piece.Pawn;
            }

            return result;
        }

        public bool IsKingChecked(int color)
        {
            var king = Pieces[color][Piece.King];
            var kingField = BitOperations.BitScan(king);

            return IsFieldAttacked(color, (byte)kingField);
        }

        public void MovePiece(int color, int piece, int from, int to)
        {
            var move = (1ul << from) | (1ul << to);

            Pieces[color][piece] ^= move;
            Occupancy[color] ^= move;
            OccupancySummary ^= move;
            
            Position[color][GamePhase.Opening] -= PieceSquareTablesData.Values[piece][color][GamePhase.Opening][from];
            Position[color][GamePhase.Opening] += PieceSquareTablesData.Values[piece][color][GamePhase.Opening][to];

            Position[color][GamePhase.Ending] -= PieceSquareTablesData.Values[piece][color][GamePhase.Ending][from];
            Position[color][GamePhase.Ending] += PieceSquareTablesData.Values[piece][color][GamePhase.Ending][to];

            PieceTable[from] = -1;
            PieceTable[to] = piece;
        }

        public void AddPiece(int color, int piece, int fieldIndex)
        {
            var field = 1ul << fieldIndex;

            Pieces[color][piece] ^= field;
            Occupancy[color] ^= field;
            OccupancySummary ^= field;

            Material[color] += EvaluationConstants.Pieces[piece];

            Position[color][GamePhase.Opening] += PieceSquareTablesData.Values[piece][color][GamePhase.Opening][fieldIndex];
            Position[color][GamePhase.Ending] += PieceSquareTablesData.Values[piece][color][GamePhase.Ending][fieldIndex];

            PieceTable[fieldIndex] = piece;
        }

        public void RemovePiece(int color, int piece, int fieldIndex)
        {
            var field = 1ul << fieldIndex;

            Pieces[color][piece] ^= field;
            Occupancy[color] ^= field;
            OccupancySummary ^= field;

            Material[color] -= EvaluationConstants.Pieces[piece];

            Position[color][GamePhase.Opening] -= PieceSquareTablesData.Values[piece][color][GamePhase.Opening][fieldIndex];
            Position[color][GamePhase.Ending] -= PieceSquareTablesData.Values[piece][color][GamePhase.Ending][fieldIndex];

            PieceTable[fieldIndex] = -1;
        }

        public int CalculateMaterial(int color)
        {
            var material = 0;

            for (var i = 0; i < 6; i++)
            {
                material += (int)BitOperations.Count(Pieces[color][i]) * EvaluationConstants.Pieces[i];
            }

            return material;
        }

        public int CalculatePosition(int color, int phase)
        {
            var result = 0;

            for (var pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                var pieces = Pieces[color][pieceIndex];
                while (pieces != 0)
                {
                    var lsb = BitOperations.GetLsb(pieces);
                    pieces = BitOperations.PopLsb(pieces);
                    var fieldIndex = BitOperations.BitScan(lsb);

                    result += PieceSquareTablesData.Values[pieceIndex][color][phase][fieldIndex];
                }
            }

            return result;
        }

        public float GetPhaseRatio()
        {
            var materialOfWeakerSide = Math.Min(Material[Color.White], Material[Color.Black]);

            var openingDelta = _materialAtOpening - EvaluationConstants.OpeningEndgameEdge;
            var boardDelta = materialOfWeakerSide - EvaluationConstants.OpeningEndgameEdge;
            var ratio = (float) boardDelta / openingDelta;

            return Math.Max(0, ratio);
        }

        public int GetGamePhase()
        {
            var materialOfWeakerSide = Math.Min(Material[Color.White], Material[Color.Black]);
            return materialOfWeakerSide > EvaluationConstants.OpeningEndgameEdge ? GamePhase.Opening : GamePhase.Ending;
        }

        public bool IsThreefoldRepetition()
        {
            if (NullMoves == 0 && _hashes.Count() >= 8)
            {
                var first = _hashes.Peek(3);
                var second = _hashes.Peek(7);

                if (Hash == first && first == second)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsFiftyMoveRuleDraw()
        {
            if (NullMoves == 0 && IrreversibleMovesCount >= 100)
            {
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return BoardToFen.Parse(this);
        }

        private int GetPromotionPiece(MoveFlags flags)
        {
            switch (flags)
            {
                case MoveFlags.QueenPromotion:
                case MoveFlags.QueenPromotionCapture:
                {
                    return Piece.Queen;
                }

                case MoveFlags.RookPromotion:
                case MoveFlags.RookPromotionCapture:
                {
                    return Piece.Rook;
                }

                case MoveFlags.BishopPromotion:
                case MoveFlags.BishopPromotionCapture:
                {
                    return Piece.Bishop;
                }

                case MoveFlags.KnightPromotion:
                case MoveFlags.KnightPromotionCapture:
                {
                    return Piece.Knight;
                }
            }

            throw new InvalidOperationException();
        }
    }
}
