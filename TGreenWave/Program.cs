using System;


namespace LK.GreenRoute
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             You are the operations director for a processing plant at a large mine. Your job is to devise a program (i.e., plan) for the plant that maximizes profits while operating within the given constraints. The plant produces four major minerals: copper, gold, silver and platinum. Your plan must specify how much copper, silver, gold, and platinum (all in ounces) you will produce, and you will need to report how much profit you will achieve.
            In a given week, the mine can produce up to 2,000 tons of ore that must be processed into some combination of the four major minerals. A ton of ore contains 10 ounces of copper, 2 ounces of gold, 3 ounces of silver, and 1 ounce of platinum. To keep things simple, you may only extract one mineral from any batch (of arbitrary size) of ore processed (note that the amounts of ore processed for each type need not be integer valued).
            Processing the ore requires power, water, labor and use of processing lines. Producing each of the minerals requires different amounts of processing resources.
            One ounce of copper requires exactly:
            30 kW hours
            1,000 gallons of water
            50 hours of labor
            4 hours of processing time on a processing line
            One ounce of gold requires exactly:
            15 kW hours
            6,000 gallons of water
            20 hours of labor
            6 hours of processing time on a processing line
            One ounce of silver requires exactly:
            19 kW hours
            4,100 gallons of water
            21 hours of labor
            19 hours of processing time on a processing line
            One ounce of platinum requires exactly:
            12 kW hours
            9,100 gallons of water
            10 hours of labor
            30 hours of processing time on a processing line
            There is only a limited amount of power, water, labor and processing line time available. In a given week, you may use the following resources (but no more):
            1,000 kW hours of power
            1,000,000 gallons of water
            640 hours of labor
            3 processing lines for 24 hours a day for 6 days a week for a total of 432 processing line hours
            Finally, each ounce of finished mineral has an associated profit:
            an ounce of copper is worth $10.20
            an ounce of gold is worth $422.30
            an ounce of silver is worth $6.91
            and an ounce of platinum is worth $853.00
            KW hours <= 1000


                KW hours <= 1000
                gallons water <=1000000
                hours of labor<=640
                hours of processing line<=432
                minerio= 7,0548e+7

                x1-copper
                x2-gold
                x3-silver
                x4-platinum

                f(x) = 10.20 x1 + 422.30 x2 + 6.91 x3 + 853.00 x4
                10*x1+2*x2+3*x3+x4<= 7,0548e+7
                30 * x1 + 15 * x2 + 19 * x3 + 12 * x4 <= 1000
                1000*x1+6000*x2+4100*x3+9100*x4<=1000000
                50*x1+20*x2+21*x3+10*x4<=640
                4*x1+6*x2+19*x3+30*x4<=432

             */


            var s = new Simplex(
                    new[] { 10.2, 422.3, 6.91, 853 },
                    new[,] {
                {10.0, 2, 3, 1},
                {30, 15, 19, 12},
                {1000, 6000, 4100, 9100},
                {50, 20, 21, 10},
                {4, 6, 19, 30}
                    },
                    new double[] { 7.0548e+7, 1000, 1000000, 640, 432 }
                  );

                  var answer = s.maximize();
                  Console.WriteLine("Item1="+ answer.Item1);
                  Console.WriteLine("Item2:"+string.Join("; ", answer.Item2)); 
        }


    }
}