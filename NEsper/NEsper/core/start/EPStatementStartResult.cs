///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>Result holder returned by @{link EPStatementStartMethod}. </summary>
    public class EPStatementStartResult
    {
        /// <summary>Ctor. </summary>
        /// <param name="viewable">last view to attach listeners to</param>
        /// <param name="stopMethod">method to stop</param>
        public EPStatementStartResult(Viewable viewable,
                                      EPStatementStopMethod stopMethod)
        {
            Viewable = viewable;
            StopMethod = stopMethod;
            DestroyMethod = null;
        }

        /// <summary>Ctor. </summary>
        /// <param name="viewable">last view to attach listeners to</param>
        /// <param name="stopMethod">method to stop</param>
        /// <param name="destroyMethod">method to call when destroying</param>
        public EPStatementStartResult(Viewable viewable,
                                      EPStatementStopMethod stopMethod,
                                      EPStatementDestroyMethod destroyMethod)
        {
            Viewable = viewable;
            StopMethod = stopMethod;
            DestroyMethod = destroyMethod;
        }

        /// <summary>Returns last view to attached to. </summary>
        /// <value>view</value>
        public Viewable Viewable { get; private set; }

        /// <summary>Returns stop method. </summary>
        /// <value>stop method.</value>
        public EPStatementStopMethod StopMethod { get; private set; }

        /// <summary>Returns destroy method. </summary>
        /// <value>destroy method</value>
        public EPStatementDestroyMethod DestroyMethod { get; private set; }
    }
}