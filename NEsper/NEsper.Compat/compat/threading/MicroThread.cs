///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

namespace com.espertech.esper.compat.threading
{
    public class MicroThread
    {
        public static void Sleep(long uSeconds)
        {
            var spins = PerformanceObserverWin.SpinIterationsPerMicro;
            var uHead = PerformanceObserver.MicroTime;
            var uTail = uHead + uSeconds;
            var uApprox = uSeconds/spins;

            do
            {
                Thread.SpinWait((int) uApprox);
                uHead = PerformanceObserver.MicroTime;
                if (uHead >= uTail)
                    return;

                uApprox = (uTail - uHead)/spins;
            } while (true);
        }

        public static void SleepNano(long nanoSeconds)
        {
            var spins = 1000*PerformanceObserverWin.SpinIterationsPerMicro;
            var uHead = PerformanceObserver.NanoTime;
            var uTail = uHead + nanoSeconds;
            var uApprox = nanoSeconds/spins;

            do
            {
                Thread.SpinWait((int)uApprox);
                uHead = PerformanceObserver.NanoTime;
                if (uHead >= uTail)
                    return;

                uApprox = (uTail - uHead)/spins;
            } while (true);
        }
    }
}
