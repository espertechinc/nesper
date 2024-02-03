///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    /// <summary>
    ///     Method to transform an event based on the select expression.
    /// </summary>
    public class ResultSetProcessorHandtruTransform : TransformEventMethod
    {
        private readonly EventBean[] newData;
        private readonly ResultSetProcessor resultSetProcessor;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="resultSetProcessor">is applying the select expressions to the events for the transformation</param>
        public ResultSetProcessorHandtruTransform(ResultSetProcessor resultSetProcessor)
        {
            this.resultSetProcessor = resultSetProcessor;
            newData = new EventBean[1];
        }

        public EventBean Transform(EventBean theEvent)
        {
            newData[0] = theEvent;
            var pair = resultSetProcessor.ProcessViewResult(newData, null, true);
            return pair?.First?[0];
        }
    }
} // end of namespace