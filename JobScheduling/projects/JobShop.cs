using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;

class JobShop {

    private int machinesCount { get; set; }
    private int jobsCount { get; set; }
    private List<int> allMachines { get; set; }
    private List<int> allJobs { get; set; }
    private List<List<int>> machines { get; set; }
    private List<List<int>> processingTimes { get; set; }
    private int horizon = 0;
    public const int timeLimitInMs = 0;

    // Constructor
    public JobShop() {
        this.machinesCount = 3;
        this.jobsCount = 3;

        this.allMachines = new List<int>();
        for (var i = 0; i < this.machinesCount; i++) this.allMachines.Add(i);

        this.allJobs = new List<int>();
        for (var i = 0; i < this.jobsCount; i++) this.allJobs.Add(i);

        // Define data.
        this.machines = new List<List<int>>();
        this.machines.Add(new List<int>(){0, 1, 2});
        this.machines.Add(new List<int>(){0, 2, 1});
        this.machines.Add(new List<int>(){1, 2});

        this.processingTimes = new List<List<int>>();
        this.processingTimes.Add(new List<int>(){3, 2, 2});
        this.processingTimes.Add(new List<int>(){2, 1, 4});
        this.processingTimes.Add(new List<int>(){4, 3});

        // Computes horizon
        for (var i = 0; i < allMachines.Count; i++) {
            this.horizon += processingTimes.ElementAt(i).Sum();
        }

    }

    public void RunJobShopScheduling(String solverType) {

        Solver solver = new Solver(solverType);

        if (solver == null) {
            Console.WriteLine("JobShop failed to create a solver " + solverType);
            return;
        }

        // All tasks
        Dictionary<string, IntervalVar> allTasks = new Dictionary<string, IntervalVar>();

        // Creates jobs
        for (int i = 0; i < allJobs.Count; i++) {
            for (int j = 0; j < machines.ElementAt(i).Count; j++) {
                IntervalVar oneTask = solver.MakeFixedDurationIntervalVar (0,
                                              horizon,
                                              processingTimes.ElementAt(i).ElementAt(j),
                                              false,
                                              "Job_" + i + "_" + j);

                allTasks.Add("Job_" + i + "_" + j, oneTask);
            }
        }

        // Create sequence variables and add disjuctive constraints
        List<SequenceVar> allSequences = new List<SequenceVar>();
        foreach (var machine in allMachines) {

            List<IntervalVar> machinesJobs = new List<IntervalVar>();
            for (int i = 0; i < allJobs.Count; i++) {
                for (int k = 0; k < machines.ElementAt(i).Count; k++) {
                    if (machines.ElementAt(i).ElementAt(k) == machine) {
                        machinesJobs.Add(allTasks["Job_" + i + "_" + k]);
                    }
                }
            }

            DisjunctiveConstraint disj = solver.MakeDisjunctiveConstraint (machinesJobs.ToArray(),
                                                                           "machine " + machine);
            allSequences.Add(disj.SequenceVar());
            solver.Add(disj);
        }

        // Add conjunctive constraints
        foreach (var job in allJobs) {
            for (int j = 0; j < machines.ElementAt(job).Count - 1; j++) {
              solver.Add(allTasks["Job_" + job + "_" + (j+1)].StartsAfterEnd(allTasks["Job_" + job + "_" + j]));
            }
        }

        // Set the objective
        IntVar[] allEnds = new IntVar[jobsCount];
        for (int i = 0; i < allJobs.Count; i++)
        {
          allEnds[i] = allTasks["Job_" + i + "_" + (machines.ElementAt(i).Count-1)].EndExpr().Var();
        }

        // Objective: minimize the makespan (maximum end times of all tasks)
        // of the problem.
        IntVar objectiveVar = solver.MakeMax(allEnds).Var();
        OptimizeVar objectiveMonitor = solver.MakeMinimize(objectiveVar, 1);

        // Create search phases
        DecisionBuilder sequencePhase = solver.MakePhase(allSequences.ToArray(), Solver.SEQUENCE_DEFAULT);
        DecisionBuilder varsPhase = solver.MakePhase(objectiveVar, Solver.CHOOSE_FIRST_UNBOUND, Solver.ASSIGN_MIN_VALUE);

        // The main decision builder (ranks all tasks, then fixes the
        // objectiveVariable).
        DecisionBuilder mainPhase = solver.Compose(sequencePhase, varsPhase);

        SolutionCollector collector = solver.MakeLastSolutionCollector();
        collector.Add(allSequences.ToArray());
        collector.Add(objectiveVar);

        foreach (var i in allMachines) {
            SequenceVar sequence = allSequences.ElementAt(i);
            long sequenceCount = sequence.Size();
            for (int j = 0; j < sequenceCount; j++) {
                IntervalVar t = sequence.Interval(j);
                collector.Add(t.StartExpr().Var());
                collector.Add(t.EndExpr().Var());
            }
        }

        // Search.
        bool solutionFound = solver.Solve(mainPhase, null, objectiveMonitor, null, collector);

        if(solutionFound) {
            //The index of the solution from the collector
            const int SOLUTION_INDEX = 0;
            Assignment solution = collector.Solution(SOLUTION_INDEX);

            string solLine = "";
            string solLineTasks = "";
            Console.WriteLine("Time Intervals for Tasks\n");

            for (int m = 0; m < this.machinesCount; m++) {
                //Console.WriteLine("MachineCount: " + this.machinesCount);
                solLine = "Machine " + m + " :";
                solLineTasks = "Machine " + m + ": ";

                SequenceVar seq = allSequences.ElementAt(m);
                int[] storedSequence = collector.ForwardSequence(SOLUTION_INDEX, seq);

                foreach (int taskIndex in storedSequence) {
                    //Console.WriteLine("taskIndex: " + taskIndex);
                    IntervalVar task = seq.Interval(taskIndex);
                    solLineTasks += task.Name() + " ";
                    //Console.WriteLine("Len: " + storedSequence.Length);
                }

                foreach (int taskIndex in storedSequence) {
                    IntervalVar task = seq.Interval(taskIndex);
                    string solTemp = "[" + collector.Value(0,task.StartExpr().Var()) + ",";
                    solTemp += collector.Value(0,task.EndExpr().Var()) + "] ";
                    solLine += solTemp;
                }

                //solLine += "\n";
                solLineTasks += "\n";

                //Console.WriteLine(solLineTasks);
                Console.WriteLine(solLine);
            }


        }
        else {
            Console.WriteLine("No solution found!");
        }
    }

    public static void Main(String[] args) {
        Console.WriteLine("\n---- Job shop Scheduling Program ----");
        JobShop jobShop = new JobShop();
        jobShop.RunJobShopScheduling("Jobshop");
    }
}
