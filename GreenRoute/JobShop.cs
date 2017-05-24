using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using LK.GPXUtils;
using System.Xml;
using System.IO;

class JobShop {

    private int machinesCount { get; set; }
    private int jobsCount { get; set; }
    private List<int> allMachines { get; set; }
    private List<int> allJobs { get; set; }
    private List<List<int>> machines { get; set; }
    private List<List<int>> processingTimes { get; set; }
    private List<List<int>> jobIds { get; set; }
    private int horizon = 0;
    private GPXPoint bucketInfo { get; set; }
    private int cycleTime { get; set; }
    public const int timeLimitInMs = 0;

    // Constructor
    public JobShop(List<List<int>> machines, List<List<int>> processingTime, List<List<int>> jobIds, GPXPoint bucketInfo, int cycleTime) {
        this.machinesCount = machines.Count;
        this.jobsCount = processingTime.Count;
        this.bucketInfo = bucketInfo;
        this.cycleTime = cycleTime;
        this.jobIds = jobIds;

        Console.WriteLine("machinesCount: " + this.machinesCount);
        Console.WriteLine("jobsCount: " + jobsCount);
        
        this.allMachines = new List<int>();
        for (var i = 0; i < this.machinesCount; i++) this.allMachines.Add(i);

        this.allJobs = new List<int>();
        for (var i = 0; i < this.jobsCount; i++) this.allJobs.Add(i);

        // Define data.
        this.machines = machines;
        this.processingTimes = processingTime;


        // Computes horizon
        for (var i = 0; i < allMachines.Count; i++) {
            this.horizon += processingTimes.ElementAt(i).Sum();
        }
        Console.WriteLine("Horizon: " + this.horizon);
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
                //Console.WriteLine("Job_" + i + "_" + j);
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
            allEnds[i] = allTasks["Job_" + i + "_" + (machines.ElementAt(i).Count - 1)].EndExpr().Var();
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

        if (solutionFound) {
            //The index of the solution from the collector
            const int SOLUTION_INDEX = 0;
            Assignment solution = collector.Solution(SOLUTION_INDEX);

            string solLine = "";
            string solLineTasks = "";
            Console.WriteLine("Time Intervals for Tasks\n");

            List<List<TimeSpan>> tuplesSolution = new List<List<TimeSpan>>();

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

                // First GreenTime
                //TimeSpan timeToAdd = tuplesSolution.First().Last();
                TimeSpan timeEndBucket = this.bucketInfo.EndBucket;
                TimeSpan timeStartBucket = this.bucketInfo.StartBucket;
                

                int solutionSize = tuplesSolution.Count;
                bool isEnd = false;

                List<int> list_id = jobIds.ElementAt(m);
                // Adding GreenTime to Solution
                while (timeStartBucket.CompareTo(timeEndBucket) < 0)
                {
                    foreach (int taskIndex in storedSequence)
                    {
                        IntervalVar task = seq.Interval(taskIndex);

                        var startValue = TimeSpan.FromSeconds(collector.Value(0, task.StartExpr().Var()));
                        var endValue = TimeSpan.FromSeconds(collector.Value(0, task.EndExpr().Var()));

                        TimeSpan greenTime = endValue.Subtract(startValue);
                        TimeSpan timeEnd;

                        timeEnd = timeStartBucket.Add(greenTime);


                        List<TimeSpan> tuple = new List<TimeSpan>();
                        tuple.Add(timeStartBucket);
                        if (timeEndBucket.CompareTo(timeEnd) < 0)
                        {
                            timeEnd = timeEndBucket;
                            isEnd = true;
                        }
                                        
                        tuple.Add(timeEnd);
                        tuplesSolution.Add(tuple);
                        if (taskIndex + 1 < list_id.Count() && list_id.ElementAt(taskIndex) == list_id.ElementAt(taskIndex + 1))
                            timeStartBucket = timeStartBucket.Add(TimeSpan.FromSeconds(this.cycleTime));
                        else
                            timeStartBucket = timeEnd;
                        if (isEnd)
                            break;
                        
                    }
                }

                //
                // Saving the Solution to a XML file
                //
                JobShop.save(m, tuplesSolution);

                //solLine += "\n";
                //solLineTasks += "\n";

                //Console.WriteLine(solLineTasks);
                //Console.WriteLine(solLine);
            }


        }
        else {
            Console.WriteLine("No solution found!");
        }
    }

    public static void save(int machine, List<List<TimeSpan>> tuplesSolution)
    {
        JobShop.createNewXML(machine, tuplesSolution);
    }

    public static void createNewXML(int machine, List<List<TimeSpan>> tuplesSolution)
    {
        XmlDocument doc = new XmlDocument();

        // Initial Setup
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        XmlElement root = doc.DocumentElement;
        doc.InsertBefore(xmlDeclaration, root);

        XmlElement jobshop = doc.CreateElement(string.Empty, "jobshop", string.Empty);
        doc.AppendChild(jobshop);

        XmlElement machineLabel = doc.CreateElement(string.Empty, "machine", string.Empty);
        jobshop.AppendChild(machineLabel);

        XmlAttribute id = doc.CreateAttribute("id");
        id.Value = machine.ToString();
        machineLabel.Attributes.Append(id);

        for (var i = 0; i < tuplesSolution.Count; i++)
        {
            XmlElement green = doc.CreateElement(string.Empty, "green", string.Empty);
            machineLabel.AppendChild(green);

            XmlAttribute start = doc.CreateAttribute("start");
            start.Value = tuplesSolution[i][0].ToString();
            green.Attributes.Append(start);

            XmlAttribute end = doc.CreateAttribute("end");
            end.Value = tuplesSolution[i][1].ToString();
            green.Attributes.Append(end);
        }

        String fileout = Path.Combine(Path.GetDirectoryName("."), "SolutionJobShopMachine" + machine + ".xml");
        doc.Save(fileout);
    }

    /*public static void Main(String[] args) {
        
    }*/
}
