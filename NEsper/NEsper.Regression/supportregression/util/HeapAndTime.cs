///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.util
{
    public class HeapAndTime
    {
        private readonly long _thenVirtualMemory;
        private readonly long _thenPeakVirtualMemory;
        private readonly long _thenMsec = PerformanceObserver.MilliTime;
        private readonly long _thenNano = PerformanceObserver.NanoTime;
    
        public HeapAndTime()
        {
            var myProcess = Process.GetCurrentProcess();
            _thenVirtualMemory = myProcess.VirtualMemorySize64;
            _thenPeakVirtualMemory = myProcess.PeakVirtualMemorySize64;
        }
    
        public String Report()
        {
            var myProcess = Process.GetCurrentProcess();

            long deltaVirtualMemory = myProcess.VirtualMemorySize64 - _thenVirtualMemory;
            long deltaPeakVirtualMemory = myProcess.PeakVirtualMemorySize64 - _thenPeakVirtualMemory;
            long deltaMsec = PerformanceObserver.MilliTime - _thenMsec;
            long deltaNano = PerformanceObserver.NanoTime - _thenNano;
    
            return "Delta: Sec " + deltaMsec / 1000d +
                   "  SecHres " + deltaNano / 1000000000d +
                   "  MaxMB " + deltaPeakVirtualMemory / 1024d / 1024d +
                   "  SizeMB " + deltaVirtualMemory +
                   "  (heap max " + _thenPeakVirtualMemory/ 1024d / 1024d + ")";
        }
    }
}
