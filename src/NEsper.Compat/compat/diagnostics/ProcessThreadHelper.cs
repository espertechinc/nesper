///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace com.espertech.esper.compat.diagnostics
{
    public static class ProcessThreadHelper
    {
#if NETCORE
#else
        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern int GetCurrentWin32ThreadId();
#endif
        
        public static ProcessThread GetProcessThread()
        {
#if NETCORE
            return null;
#else
            return GetProcessThread(GetCurrentWin32ThreadId());
#endif
        }

        public static ProcessThread GetProcessThread(int threadId)
        {
            var process = Process.GetCurrentProcess();
            return process.Threads
                .Cast<ProcessThread>()
                .FirstOrDefault(processThread => processThread.Id == threadId);
        }
    }
}