///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace NEsper.Benchmark.Stats
{
    public class Program
    {
        public static void Main(String[] args)
        {
            var stats = new Server.Stats("a", "any", 10, 20);
            stats.Update(1);
            stats.Update(2);
            stats.Update(10);
            stats.Update(15);
            stats.Update(25);
            //stats.Dump();

            var stats2 = new Server.Stats("b", "any", 10, 20);
            stats2.Update(1);
            stats.Merge(stats2);
            stats.Dump();

            long l = 100;
            long l2 = 3;
            Console.WriteLine("{0}", (float)l / l2);
            Console.WriteLine("{0:4F}", (float)l / l2);
            Console.ReadLine();
        }
    }
} // End of namespace
