///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO.Ports;

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
            var pair = _resultSetProcessor.ProcessViewResult(_newData, null, true);
            if (pair == null)
                return null;
            if (pair.First == null)
                return null;
            return pair.First[0];
        }
    }
}
