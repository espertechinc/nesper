///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.approx;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Count-min sketch base context object.
    /// </summary>
    public abstract class CountMinSketchAgentContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="state">the state</param>
        protected CountMinSketchAgentContext(CountMinSketchState state)
        {
            State = state;
        }

        /// <summary>
        /// Returns state
        /// </summary>
        /// <value>state</value>
        public CountMinSketchState State { get; private set; }
    }
}
