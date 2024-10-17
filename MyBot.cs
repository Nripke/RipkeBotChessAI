using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    /*
        RIPKEBOT v 1.4

        POSSIBILITIES:

        Started: Create an opening repetoire for first 5 moves
        DONE: Simple recursive search algorithm
        DONE: Alpha-Beta Pruning
        DONE: Optimize move order for pruning

        EVALUATION STRATEGIES:
        DONE: Material
        DONE: Checkmate
        DONE: King safety
        Done: Piece preference squares
        TO DO:
        - Weighted piece mobility
        - Open Files
        - Isolated Pawns
        - Passed Pawns
        - Doubled Pawns
        - Weighted King Control
    */
    /*class Store 
    {
        public int depth;
        public double eval;

        public Store(int d, double e)
        {
            depth = d;
            eval = e;
        }
    }*/
    int searchDepth = 1;
    int maxDepth = 3;
    int inf = int.MaxValue;
    int negInf = int.MinValue;

    int searches = 0;
    int cuts = 0;
    //Dictionary<ulong, string> openingRep = new Dictionary<ulong, string>();
    //Dictionary<ulong, Store> storedEvals = new Dictionary<ulong, Store>(200000);
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    int[] pieceVals = {0, 100, 300, 325, 500, 900, 99999};
    
    Boolean endgame;
    Move[] bestMove;
    Board board;
    //List<Move>[] killerMoves;

    int[] pawnOpening = {0,	0,	0,	0,	0,	0,	0,	0,
                        30,	30,	30,	30, 30, 30, 30,	30,
                        15,	0,	0,	0,	0,	0,	0,	15,
                        0,	0,	0,	15,	15,	0,	0,	0,
                        0, -10, -10, 20, 25, -10, -10, 0,
                        12,	12,	20,	0, 20, 0, 12, 12,
                        16,	16,	16,	-20, -20, 16, 16, 16,
                        0,	0,	0,	0,	0,	0,	0,	0};

    int[] pawnEndgame = {0,	0,	0,	0,	0,	0,	0,	0,
                        50,	46,	46,	43,	43,	46,	46,	50,
                        40,	36,	36,	33,	33,	33,	36,	40,
                        30,	30,	30,	28,	28,	30,	30,	30,
                        20,	20,	20,	20,	20,	20,	20,	20,
                        10,	10,	10,	10,	10,	10,	10,	10,
                        10,	10,	10,	10,	10,	10,	10,	10,
                        0,	0,	0,	0,	0,	0,	0,	0};

    int[] kingOpening = {-60, -60, -60,	-60, -60, -60, -60,	-60,
                        -60, -55, -50, -50,	-50, -50, -55, -60,
                        -60, -50, -50, -50,	-50, -50, -50, -60,
                        -60, -50, -50, -50,	-50, -50, -50, -60,
                        -60, -55, -50, -50,	-50, -50, -55, -60,
                        -50, -50, -50, -50,	-50, -50, -50, -50,
                        -40, -40, -40, -50,	-50, -40, -40, -40,
                        10,	25,	30,	-25, -20, -35,	35,	30};

    int[] kingEndgame = {-40, -25, -25,	-25, -25,-25, -25, -40,
                        -20, 30, 30, 30, 30, 30, 30, -20,
                        -20, 33, 40, 40, 40, 40, 33, -20,
                        -20, 30, 40, 50, 50, 40, 30, -20,
                        -20, 30, 40, 50, 50, 40, 30, -20,
                        -20, 27, 33, 40, 40, 33, 27, -20,
                        -20, 7,	15,	33,	33,	15,	7, -20,
                        -40, -30,-30, -30, -30,	-30, -30, -40};

    int[] knightOpening = {-40,	-25, -25, -25, -25,	-25, -25, -40,
                        -25, -20, 20, 20, 20, 20, -20, -25,
                        -20, 10, 20, 20, 20, 20, 10, -20,
                        -10, 15, 18, 22, 22, 18, 15, -10,
                        -10, 15, 18, 22, 22, 18, 15, -10,
                        -20, 15, 23, 15, 15, 23, 15, -20,
                        -20, -30, 0, 10, 10, 0,	-30, -20,
                        -40, -10, -5, -5, -5, -5, -10, -40};

    int[] knightEndgame = {-15,	-10, -5, 0,	0, -5, -10,	-15,
                        -10, -5, 7,	10,	10,	7, -5, -10,
                        -5,	7, 15, 20, 20, 15, 7, -5,
                        -10, 10, 20, 30, 30, 20, 10, -10,
                        -10, 10, 20, 30, 30, 20, 10, -10,
                        -5,	7, 15, 20, 20, 15, 7, -5,
                        -10, -5, 7,	10,	10,	7, -5, -10,
                        -15, -10, -5, 0, 0, -5,	-10, -15};

    int[] bishopOpening = {-15,	-10, -10, -10, -10,	-10, -10, -15,
                        -5,	5, 3, -8, -8, 3, 5,	-5,
                        5, -3, 8, 0, 0,	8, -3, 5,
                        -5,	15,	-3,	10,	10,	-3,	15,	-5,
                        -5,	-3,	25,	12,	12,	25,	-3,	-5,
                        -5,	20,	15,	15,	15,	15,	20,	-5,
                        5, 20, 8, 6, 6,	8, 20, 5,
                        -10, -3, -5, -10, -10, -5, -15, -10};

    int[] bishopEndgame = {15, -5, -8, -8, -8, -8, -5, 15,
                        -5,	20,	5, 3, 3, 5,	20,	-5,
                        -8,	5, 25, 20, 20, 25, 5, -8,
                        -8,	3, 20, 30, 30, 20, 3, -8,
                        -8,	3, 20, 30, 30, 20, 3, -8,
                        -8,	5, 25, 20, 20, 25, 5, -8,
                        -5,	20,	5, 3, 3, 5,	20,	-5,
                        15,	-5,	-8,	-8,	-8,	-8,	-5,	15};

    int[] queenOpening = {-30, -10,	3, 15, 15, 3, -10, 10,
                        -10, 5,	-5,	-10, -10, 8, 5,	10,
                        -10, -5, -7, 2,	2, -7, -5, 10,
                        -5,	8, 2, 10, 10, 2, -7, -5,
                        -5,	5, 2, 10, 10, 2, 5,	-5,
                        -10, 15, 5,	5, 5, 5, 10, -10,
                        -10, 5,	4, 3, 3, 4,	5, -10,
                        -30, -20, -5, 10, -5, -10, -20,	-30};

    int[] queenEndgame = {15, -5, -8, -8, -8, -8, -5, 15,
                        -5,	20,	5, 3, 3, 5,	20,	-5,
                        -8,	5, 25, 20, 20, 25, 5, -8,
                        -8,	3, 20, 30, 30, 20, 3, -8,
                        -8,	3, 20, 30, 30, 20, 3, -8,
                        -8,	5, 25, 20, 20, 25, 5, -8,
                        -5,	20,	5, 3, 3, 5,	20,	-5,
                        15,	-5,	-8,	-8,	-8,	-8,	-5,	15};

    int[] rookOpening = {12, 12, 12, 12, 12, 12, 12, 12,
                        25,	25,	25,	25,	25,	25,	25,	25,
                        0,	0,	0,	0,	0,	0,	0,	0,
                        0,	0,	0,	0,	0,	0,	0,	0,
                        0,	0,	0,	0,	0,	0,	0,	0,
                        0,	0,	0,	0,	0,	0,	0,	0,
                        -12, -12, -12, -12,	-12, -12, -12, -12,
                        15,	-10, -10, 10, 12, 10, -10, 15};

    int[] rookEndgame = {20, 20, 20, 20, 20, 20, 20, 20,
                        25,	25,	25,	25,	25,	25,	25,	25,
                        22,	19,	0,	0,	0,	0,	19,	22,
                        19,	0,	0, -10,	-10, 0,	0, 19,
                        16,	0,	0, -10,	-10, 0,	0, 16,
                        13,	13,	0,	0,	0,	0,	13,	13,
                        15,	15,	15,	15,	15,	15,	15,	15,
                        10,	10,	10,	10,	10,	10,	10,	10};

    int[][] positionalValsOpening = new int[6][];
    int[][] positionalValsEndgame = new int[6][];

    int[] mobilityValsOpening = {0, 10, 10, 10, 10, 10, 10};
    int[] mobilityValsEndgame = {0, 10, 10, 10, 10, 10, 10};

    int[] kingAttackValsOpening = {0, 10, 10, 15, 30, 20, 10};
    int[] kingAttackValsEndgame = {0, 10, 10, 15, 30, 20, 10};

    public MyBot()
    {
        positionalValsOpening[0] = pawnOpening;
        positionalValsOpening[1] = knightOpening;
        positionalValsOpening[2] = bishopOpening;
        positionalValsOpening[3] = rookOpening;
        positionalValsOpening[4] = queenOpening;
        positionalValsOpening[5] = kingOpening;

        positionalValsEndgame[0] = pawnEndgame;
        positionalValsEndgame[1] = knightEndgame;
        positionalValsEndgame[2] = bishopEndgame;
        positionalValsEndgame[3] = rookEndgame;
        positionalValsEndgame[4] = queenEndgame;
        positionalValsEndgame[5] = kingEndgame;
        //killerMoves = new List<Move>[totalIterations];
        //for (int i = 0; i<totalIterations; i++) {killerMoves[i] = new List<Move>();}
    }


    /*
    depth = 2 Horizon Search --> +320 Elo against EvilBot
    */
    public Move Think(Board board, Timer timer)
    {
        //System.Console.WriteLine("-----------------------Initial Prune Ordering-----------------------");
        
        this.board = board;

        // Incremental changes to depth based on pieces left
        if (BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) <= 8) //Adds +50 elo !!
        {
            maxDepth = 5;
        }

        if (BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) <= 5)
        {
            maxDepth = 6;
        }

        bestMove = new Move[maxDepth];
        //pruneOrderingTest(false, 1);
        
        endgame = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) < 16;

        stopwatch.Restart();
        for (int i = 0; i<maxDepth; i++)
        {
            search(searchDepth, negInf, inf);
            searchDepth++;
            //System.Console.WriteLine("Depth: " + (i+1) + " Has best moves: ");
            foreach (Move m in bestMove)
            {
                //System.Console.Write(m.ToString() + " ");
            }
            //System.Console.WriteLine();
        }

        stopwatch.Stop();

        searchDepth = 1;

        //System.Console.WriteLine("NPS: " + 1000*(searches)/(stopwatch.ElapsedMilliseconds+1));
        //System.Console.WriteLine("Searches: " + searches);
        //System.Console.WriteLine("Time: " + (stopwatch.ElapsedMilliseconds+1) + " milliseconds");
        //Roughly 350 kNPS
        //System.Console.WriteLine(searches);
        searches = 0;
        
        //System.Console.WriteLine("Eval: " + evaluation());

        return bestMove[0];
    }
    
    
    int horizonSearch(int depth, int alpha, int beta)
    {
        if (board.IsInCheckmate()) {return board.IsWhiteToMove ? negInf : inf;}
        if (board.IsDraw()) {return 0;}
        
        searches++;
        Move[] captures = pruneOrdering(true, 1);
        int bestEval = evaluation();

        if (depth == 0) {return bestEval;}

        if (board.IsWhiteToMove)
        {
            if (bestEval >= beta)
            {
                return beta;
            }
            alpha = max(alpha, bestEval);
        }
        else
        {
            if (bestEval <= alpha)
            {
                return alpha;
            }
            beta = min(beta, bestEval);
        }

        /*foreach (Move m in captures)
        {
            bestEval = max(bestEval, -move(m, depth, alpha, beta, false));
            alpha = max(alpha, bestEval);
            if (alpha >= beta) {return alpha;}
        }*/

        if (board.IsWhiteToMove)
        {
            foreach (Move m in captures)
            {
                bestEval = max(bestEval, move(m, depth, alpha, beta, false));

                if (bestEval >= beta) {return beta;} 
                alpha = max(alpha, bestEval);
            }
        }else {
            foreach (Move m in captures)
            {
                bestEval = min(bestEval, move(m, depth, alpha, beta, false));
                
                if (bestEval < alpha) {return alpha;}
                beta = min(beta, bestEval);
            }
        }

        return bestEval;
    }

    int search(int depth, int alpha, int beta)
    {
        searches++;
        if (board.IsInCheckmate()) {return board.IsWhiteToMove ? negInf : inf;}
        if (board.IsDraw()) {return 0;}
        if (depth == 0) {return horizonSearch(2, negInf, inf);}

        Move[] moves = pruneOrdering(false, depth);

        int bestEval;

        //Store st;
        //if (storedEvals.TryGetValue(board.ZobristKey, out st) && depth != maxDepth) {if (depth<=st.depth) {return st.eval;}} //Figure out how many times this is called

        if (board.IsWhiteToMove)
        {
            bestEval = negInf;
            foreach (Move m in moves)
            {
                int s = move(m, depth, alpha, beta, true);
                if (s > bestEval) {bestEval = s; bestMove[searchDepth-depth] = m;} //killerMoves[maxDepth-depth].Add(m);
                if (bestEval >= beta) {return beta;}
                alpha = max(alpha, bestEval);
            }
        }else {
            bestEval = inf;
            foreach (Move m in moves)
            {
                int s = move(m, depth, alpha, beta, true);
                if (s < bestEval) {bestEval = s; bestMove[searchDepth-depth] = m;} 
                if (bestEval < alpha) {return alpha;}
                beta = min(beta, bestEval);
            }
        }

        //storedEvals[board.ZobristKey] = new Store(depth, bestEval); //Neglibile Time
        return bestEval;
    }

    //Change the board eval as we go
    int move(Move m, int depth, int alpha, int beta, Boolean isSearch)
    {
        board.MakeMove(m);
        int s = isSearch ? search(depth-1, alpha, beta) : horizonSearch(depth-1, alpha, beta);
        board.UndoMove(m);
        return s;
    }

    Move[] pruneOrdering (Boolean captures, int depth)
    {
        Move[] moves = (Move[]) board.GetLegalMoves(captures).Clone();
        int[] vals = new int[moves.Length];
        for (int i=0; i<moves.Length; i++)
        {
            Move m = moves[i];
            int mpt = (int) m.MovePieceType;
            if (m.IsCapture) {vals[i] += (pieceVals[(int) m.CapturePieceType] - pieceVals[mpt]/5);}
            if (m.Equals(bestMove[searchDepth-depth])) {vals[i] += 100000;}
            //if (killerMoves[maxDepth-depth].Contains(m)) {vals[i] += 1000;}
            if (board.SquareIsAttackedByOpponent(m.TargetSquare)) {vals[i] -= pieceVals[mpt]/5;}
            if(m.IsCastles) {vals[i] += 150;}
            if(m.IsPromotion) {vals[i] += 3*pieceVals[(int) m.PromotionPieceType];}
            
            /* ADD MOVE ORDERING BASED ON POSITIONAL VALUE **COULD BE VERY COOL** */
            vals[i] += getPositionalValue(mpt, board.IsWhiteToMove, m.TargetSquare.File, m.TargetSquare.Rank) - getPositionalValue(mpt, board.IsWhiteToMove, m.StartSquare.File, m.StartSquare.Rank);
        }

        Array.Sort(vals, moves);
        Array.Reverse(moves);
        return moves;
    }

    void pruneOrderingTest(Boolean captures, int depth)
    {
        Move[] moves = (Move[]) board.GetLegalMoves(captures).Clone();
        int[] vals = new int[moves.Length];
        for (int i=0; i<moves.Length; i++)
        {
            Move m = moves[i];
            int mpt = (int) m.MovePieceType;
            if (m.IsCapture) {vals[i] += (pieceVals[(int) m.CapturePieceType] - pieceVals[mpt]/5);}
            if (m.Equals(bestMove[searchDepth-depth])) {vals[i] += 100000;}
            //if (killerMoves[maxDepth-depth].Contains(m)) {vals[i] += 1000;}
            if (board.SquareIsAttackedByOpponent(m.TargetSquare)) {vals[i] -= pieceVals[mpt]/5;}

            /* ADD MOVE ORDERING BASED ON POSITIONAL VALUE **COULD BE VERY COOL** */
            vals[i] += getPositionalValue(mpt, board.IsWhiteToMove, m.TargetSquare.File, m.TargetSquare.Rank) - getPositionalValue(mpt, board.IsWhiteToMove, m.StartSquare.File, m.StartSquare.Rank);
        }

        Array.Sort(vals, moves);
        Array.Reverse(moves);
        Array.Reverse(vals);
        for (int i = 0; i<moves.Length; i++)
        {
            System.Console.WriteLine("Move: " + moves[i] + ", Value: " + vals[i]);
        }
    }
    int max(int a, int b)
    {
        return a > b ? a : b;
    }

    int min(int a, int b)
    {
        return a < b ? a : b;
    }

    int getPositionalValue(int p, bool isWhite, int file, int rank)
    {
        int index = file + 8*(isWhite ? 7-rank : rank);
        //System.Console.WriteLine("Piece type: " + p + ", Index: " + index);
        return  endgame ? positionalValsEndgame[p-1][index] : positionalValsOpening[p-1][index];
    }

    int evaluation()
    {
        PieceList[] pl = board.GetAllPieceLists();
        if (board.IsInCheckmate()) {if (board.IsWhiteToMove) {return negInf;}else {return inf;}}
        if (board.IsDraw()) {return 0;}
        int eval = 0;

        for (int i=0; i<12; i++)
        {
            foreach (Piece p in pl[i])
            {
                int color = (p.IsWhite ? 1 : -1);
                int pType = (int) p.PieceType;
                
                var mobility = BitboardHelper.GetPieceAttacks(p.PieceType, p.Square, board, p.IsWhite);
                eval += (pieceVals[pType] + getPositionalValue(pType, p.IsWhite, p.Square.File, p.Square.Rank) + getMobilityValue(pType)*BitboardHelper.GetNumberOfSetBits(mobility))*color;

                eval += getKingAttackValue(pType)*BitboardHelper.GetNumberOfSetBits(mobility & BitboardHelper.GetKingAttacks(board.GetKingSquare(!p.IsWhite)))*color;
            }
        }

        //Incentivize distance between kings
        int w = board.GetKingSquare(true).Index;
        int b = board.GetKingSquare(false).Index;
        int moveColor = board.IsWhiteToMove ? 1 : -1;
        if (endgame)
        {
            eval += (60-12*(Math.Abs(w/8 - b/8) + Math.Abs(w%8 - b%8)))*moveColor;
        }

        //Want protected pawns

        //Isolated Pawns = Bad

        //Doubled Pawns = Bad

        //Attack the King + King Safety: Decentivize opponent controlling squares near the king
        /*
        ulong wu = BitboardHelper.GetKingAttacks(new Square(w));
        ulong bu = BitboardHelper.GetKingAttacks(new Square(b));

        ulong personalBoard = board.IsWhiteToMove ? wu : bu;
        for (int i=0;i<BitboardHelper.GetNumberOfSetBits(personalBoard);i++)
        {
            eval += moveColor*(board.SquareIsAttackedByOpponent(new Square(BitboardHelper.ClearAndGetIndexOfLSB(ref personalBoard))) ? -20 : 0);
        }*/

        return eval;
    }

    int getMobilityValue(int pieceType)
    {
        return endgame ? mobilityValsEndgame[pieceType] : mobilityValsOpening[pieceType];
    }

    int getKingAttackValue(int pieceType)
    {
        return endgame ? kingAttackValsEndgame[pieceType] : kingAttackValsOpening[pieceType];
    }

    ulong PassedPawnMask(int squareIndex)
    {
        ulong fileA = 0x0101010101010101;
        int fileIndex = ChessChallenge.Chess.BoardHelper.FileIndex(squareIndex);
        ulong fileMask = fileA << fileIndex;
        ulong fileMaskLeft = fileA << max(0, fileIndex-1);
        ulong fileMaskRight = fileA << min(7, fileIndex+1);

        ulong tripleFileMask = fileMask | fileMaskLeft | fileMaskRight;

        int rankIndex = ChessChallenge.Chess.BoardHelper.RankIndex(squareIndex);
        ulong forwardMask = ulong.MaxValue << 8*(rankIndex + 1);
        return 0;
    }
}