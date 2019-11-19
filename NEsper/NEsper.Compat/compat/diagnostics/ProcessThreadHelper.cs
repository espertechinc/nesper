///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace com.espertech.esper.compat.diagnostics
{
    public static class ProcessThreadHelper
    {
        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern int GetCurrentWin32ThreadId();

        public static ProcessThread GetProcessThread(int threadId)
        {
            var process = Process.GetCurrentProcess();
            foreach (ProcessThread processThread in process.Threads) {
                if (processThread.Id == threadId) {
                    return processThread;
                }
            }

            return null;
        }

        public static ProcessThread GetProcessThread()
        {
            return GetProcessThread(GetCurrentWin32ThreadId());
        }
    }
}