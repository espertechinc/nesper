///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.approx.countminsketch;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Count-min sketch base context object.
    /// </summary>
    public abstract class CountMinSketchAgentContext
    {
        private readonly CountMinSketchState state;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="state">the state</param>
        protected CountMinSketchAgentContext(CountMinSketchState state)
        {
            this.state = state;
        }

        /// <summary>
        /// Returns state
        /// </summary>
        /// <returns>state</returns>
        public CountMinSketchState State => state;
    }
} // end of namespace