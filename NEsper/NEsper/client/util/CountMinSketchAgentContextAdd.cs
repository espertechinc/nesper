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
    /// Count-min sketch context object for add-operations.
    /// </summary>
    public class CountMinSketchAgentContextAdd : CountMinSketchAgentContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="state">the state</param>
        public CountMinSketchAgentContextAdd(CountMinSketchState state) : base(state)
        {
        }

        /// <summary>
        /// Returns the value.
        /// </summary>
        /// <value>value</value>
        public object Value { get; set; }
    }
}
