using System;
using System.Collections.Generic;
using ChessChallenge.API;
public class MyBot2 : IChessBot
{
    /*
        RIPKEBOT v 1.3

        POSSIBILITIES:

        Started: Create an opening repetoire for first 5 moves
        DONE: Simple recursive search algorithm
        DONE: Alpha-Beta Pruning
        DONE: Optimize move order for pruning

        EVALUATION STRATEGIES:
        DONE: Material
        DONE: Checkmate
        King safety
        Space
        Done: Piece preference squares
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
    int maxDepth = 1;
    int totalIterations = 2;
    double inf = double.PositiveInfinity;
    double negInf = double.NegativeInfinity;

    int searches = 0;
    //Dictionary<ulong, string> openingRep = new Dictionary<ulong, string>();
    //Dictionary<ulong, Store> storedEvals = new Dictionary<ulong, Store>(200000);
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    double[] pieceVals = {0, 100, 300, 325, 500, 900, 99999};
    Boolean endgame;
    double boardEval = 0;

    int[,] positionalVals = new int[7,64];

    Move[] bestMove;
    public MyBot2() // Setup
    {
        for (int i = 0; i<64; i++)
        {
            int col = i%8-3;
            int r = i/8 - 3;

            //Statistical Function
            int plus = 25 - Math.Abs(r) - Math.Abs(col);
            positionalVals[2, i] = plus;
            positionalVals[3, i] = plus;
            positionalVals[5, i] = plus;
            if (i==27 || i==28 || i==35 || i==36) {positionalVals[1, i] = 25;}
        }

        /*for (int j = 0; j<7; j++)
        {
            for (int r = 0; r<8; r++)
            {
                for (int col = 0; col<8; col++)
                {
                    System.Console.Write(positionalVals[j, r*8 + col] + " ");
                }
                System.Console.WriteLine();
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
        }*/
    }

    public Move Think(Board board, Timer timer)
    {
        boardEval = 0;
        //Update Evaluation
        PieceList[] pl = board.GetAllPieceLists();
        for (int i=0; i<12; i++)
        {
            foreach (Piece p in pl[i])
            {
                int pt = (int) p.PieceType;
                int color = p.IsWhite ? 1 : -1;
                boardEval += pieceVals[pt]*color + positionalVals[pt, p.Square.Index]*color;
            }
        }

        bestMove = new Move[totalIterations];
        endgame = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard) < 12;
        
        /*stopwatch.Restart();
        for (int i = 0; i<1000000; i++)
        {
            evaluation(board);
        }
        stopwatch.Stop();
        System.Console.WriteLine(stopwatch.ElapsedMilliseconds);*/
        System.Console.WriteLine(evaluation(board));

        for (int i = 0; i<totalIterations; i++)
        {
            search(board, maxDepth, negInf, inf);
            maxDepth++;
        }

        maxDepth = 1;

        //System.Console.WriteLine("NPS: " + 1000*(searches+calls)/(stopwatch.ElapsedMilliseconds+1));
        //System.Console.WriteLine((100*calls)/(searches+calls));
        return bestMove[totalIterations-1];
    }
    
    
    double horizonSearch(Board board, int depth, double alpha, double beta)
    {
        searches++;
        Move[] captures = pruneOrdering(board, true, 1);
        double bestEval = evaluation(board);

        if (depth == 0) {return bestEval;}

        if (board.IsWhiteToMove)
        {
            foreach (Move m in captures)
            {
                bestEval = max(bestEval, move(m, board, depth, alpha, beta, false));

                if (bestEval > beta) {return beta;}
                alpha = max(alpha, bestEval);
            }
        }else {
            foreach (Move m in captures)
            {
                bestEval = min(bestEval, move(m, board, depth, alpha, beta, false));
                
                if (bestEval < alpha) {return alpha;}
                beta = min(beta, bestEval);
            }
        }

        return bestEval;
    }

    double search(Board board, int depth, double alpha, double beta)
    {
        if (board.IsInCheckmate()) {return board.IsWhiteToMove ? negInf : inf;}
        if (board.IsDraw()) {return 0;}
        if (depth == 0) {return horizonSearch(board, 2, negInf, inf);}

        Move[] moves = pruneOrdering(board, false, depth);
        
        double bestEval;
        //Store st;
        //if (storedEvals.TryGetValue(board.ZobristKey, out st) && depth != maxDepth) {if (depth<=st.depth) {return st.eval;}} //Figure out how many times this is called

        if (board.IsWhiteToMove)
        {
            bestEval = negInf;
            foreach (Move m in moves)
            {
                double s = move(m, board, depth, alpha, beta, true);
                if (s > bestEval) {bestEval = s; bestMove[depth-1] = m;}

                if (bestEval > beta) {return beta;}
                alpha = max(alpha, bestEval);
                
            }
        }else {
            bestEval = inf;
            foreach (Move m in moves)
            {
                double s = move(m, board, depth, alpha, beta, true);
                if (s < bestEval) {bestEval = s; bestMove[depth-1] = m;}

                if (bestEval < alpha) {return alpha;}
                beta = min(beta, bestEval);
            }
        }

        //storedEvals[board.ZobristKey] = new Store(depth, bestEval); //Neglibile Time
        return bestEval;
    }

    //Change the board eval as we go
    double move(Move m, Board board, int depth, double alpha, double beta, Boolean isSearch)
    {
        board.MakeMove(m);

        double oldEval = boardEval;
        int color = board.IsWhiteToMove ? 1 : -1;

        int p = (int) m.MovePieceType;
        int c = (int) m.CapturePieceType;
        boardEval += (positionalVals[p, m.TargetSquare.Index] - positionalVals[p, m.StartSquare.Index] + pieceVals[c] + positionalVals[c,m.TargetSquare.Index])*color;

        double s = isSearch ? search(board, depth-1, alpha, beta) : horizonSearch(board, depth-1, alpha, beta);

        board.UndoMove(m);
        boardEval = oldEval;
        return s;
    }

    Move[] pruneOrdering (Board board, Boolean captures, int depth)
    {
        Move[] moves = board.GetLegalMoves(captures);
        double[] vals = new double[moves.Length];
        for (int i=0; i<moves.Length; i++)
        {
            Move m = moves[i];
            if (m.IsCapture) {vals[i] += (pieceVals[(int) m.CapturePieceType] - pieceVals[(int) m.MovePieceType]);}
            if (m.Equals(bestMove[depth-1])) {vals[i] += 1000;}
            //if (board.SquareIsAttackedByOpponent(m.TargetSquare)) {vals[i] -= 0.2*pieceVals[(int) m.MovePieceType];}
        }

        Array.Sort(vals, moves);
        Array.Reverse(moves);
        return moves;
    }


    double max(double a, double b)
    {
        return a > b ? a : b;
    }

    double min(double a, double b)
    {
        return a < b ? a : b;
    }

    double evaluation(Board board)
    {
        if (board.IsInCheckmate()) {if (board.IsWhiteToMove) {return negInf;}else {return inf;}}
        if (board.IsDraw()) {return 0;}
        double eval = boardEval;
        
        if (endgame)
        {
            Square bS = board.GetKingSquare(false);
            Square wS = board.GetKingSquare(true);
            eval -= 50*Math.Exp(-bS.Rank*bS.Rank-bS.File*bS.File); //Keep king away from the edge, also white king push other towards the edge
            eval += 50*Math.Exp(-wS.Rank*wS.Rank-wS.File*wS.File);

            //Push kings towards each other
        }
        
        return eval;
    }
    
}

/*double positionalMultiplier(Piece p, int i)
    {
        double mult = 1;
        int col = i%8-3;
        int r = i/8 - 3;
        if (p.IsQueen || p.IsBishop || p.IsKnight)
        {
            mult += 0.1*Math.Exp(-r*r-col*col); //Modeled after z = e^-(x^2 + y^2)
        }
        
        if ((p.IsBishop || p.IsKnight) && (i < 8 || i>55)) {mult -= 0.2;}
        if (p.IsWhite)
        {
            if (p.IsPawn && i==27 || i==28) {mult += 0.1;}
            if (p.IsRook && r==3) {mult += 0.1;}
        }else {
            if (p.IsPawn && i==35 ||i==36) {mult += 0.1;}
            if (p.IsRook && r==-2) {mult += 0.1;}
        }
        //if (p.IsKing && (col < 0 || col > 2)) {mult += 0.1;}
        return mult;
    }*/


/*
        //White
        openingRep[13227872743731781434] = "e2e4";

        //Black

        //Scillian: Response to e4
        openingRep[15607329186585411972] = "c7c5";
        openingRep[9341647070118419100] = "d7d6";
        openingRep[10159683761016029796] = "d7d6";

        //Response to d4
        openingRep[13920910881790336478] = "g8f6";
        
        /*string move;
        if (openingRep.TryGetValue(board.ZobristKey, out move))
        {
            return new Move(move, board);
        }*/



         /*
        for (int j=0; j<5; j++)
        {
            eval += pieceVals[j+1]*(pl[j].Count-pl[j+6].Count);
        }*/

        /*if (!board.IsInCheck())
        {
            int color = board.IsWhiteToMove ? 1 : -1;
            eval += 2*board.GetLegalMoves().Length*color;
            board.TrySkipTurn();
            eval += 2*board.GetLegalMoves().Length*-color;
            board.UndoSkipTurn();
        }*/