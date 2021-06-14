///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.approx.countminsketch;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Count-min sketch context object for topk-operations.
    /// </summary>
    public class CountMinSketchAgentContextFromBytes : CountMinSketchAgentContext
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="state">the state</param>
        public CountMinSketchAgentContextFromBytes(CountMinSketchState state)
            : base(state)
        {
        }

        /// <summary>
        ///     Returns the byte value.
        /// </summary>
        /// <returns>bytes</returns>
        public byte[] Bytes { get; set; }
    }
} // end of namespace