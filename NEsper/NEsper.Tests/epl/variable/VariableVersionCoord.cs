///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.epl.variable
{
    public class VariableVersionCoord
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly VariableService variableService;
        private int currentMark;
    
        public VariableVersionCoord(VariableService variableService)
        {
            this.variableService = variableService;
        }
    
        public int SetVersionGetMark()
        {
            lock(this) {
                currentMark++;
                variableService.SetLocalVersion();
                Log.Debug(".SetVersionGetMark Thread " + Thread.CurrentThread.ManagedThreadId + " *** mark=" + currentMark + " ***");
                return currentMark;
            }
        }
    
        public int IncMark()
        {
            lock (this) {
                currentMark++;
                return currentMark;
            }
        }
    }
}
