///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Wrapper for an aggregation spec consisting of a stream number.
    /// </summary>
    public class AggregationSpec
    {
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">stream number</param>
        public AggregationSpec(int streamNum)
        {
            StreamNum = streamNum;
        }

        /// <summary>Returns stream number. </summary>
        /// <value>stream number</value>
        public int StreamNum { get; set; }
    }
}
