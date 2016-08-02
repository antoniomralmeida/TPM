using System;
using NDesk.Options;
z
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Solvers;

namespace LK.TGreeWave
{
    class Program
    {
        static void Main(string[] args)
        {
            SimplexSolver solver = new SimplexSolver();

            int savid, vzvid;
            solver.AddVariable("Saudi Arabia", out savid);
            solver.SetBounds(savid, 0, 9000);
            solver.AddVariable("Venezuela", out vzvid);
            solver.SetBounds(vzvid, 0, 6000);

        }
        
    }
}
