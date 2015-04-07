///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Method to transform an event based on the select expression.
    /// </summary>
    public class ResultSetProcessorSimpleTransform
    {
        private readonly ResultSetProcessorBaseSimple _resultSetProcessor;
        private readonly EventBean[] _newData;
    
        /// <summary>Ctor. </summary>
        /// <param name="resultSetProcessor">is applying the select expressions to the events for the transformation</param>
        public ResultSetProcessorSimpleTransform(ResultSetProcessorBaseSimple resultSetProcessor) {
            _resultSetProcessor = resultSetProcessor;
            _newData = new EventBean[1];
        }
    
        public EventBean Transform(EventBean theEvent)
        {
            _newData[0] = theEvent;
            UniformPair<EventBean[]> pair = _resultSetProcessor.ProcessViewResult(_newData, null, true);
            return pair.First[0];
        }
    }
}
