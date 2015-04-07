///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Container for filter values for use by the <seealso cref="FilterService" /> to filter and distribute incoming events.
    /// </summary>
    public class FilterValueSetImpl : FilterValueSet
    {
        private readonly EventType _eventType;
        private readonly FilterValueSetParam[][] _parameters;
    
        /// <summary>Ctor. </summary>
        /// <param name="eventType">type of event to filter for</param>
        /// <param name="parameters">list of filter parameters</param>
        public FilterValueSetImpl(EventType eventType, FilterValueSetParam[][] parameters)
        {
            _eventType = eventType;
            _parameters = parameters;
        }

        /// <summary>Returns event type to filter for. </summary>
        /// <value>event type to filter for</value>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>Returns list of filter parameters. </summary>
        /// <value>list of filter parameters</value>
        public FilterValueSetParam[][] Parameters
        {
            get { return _parameters; }
        }

        public override String ToString()
        {
            return "FilterValueSetImpl{" +
                    "eventType=" + _eventType.Name +
                    ", parameters=" + _parameters.Render() +
                    '}';
        }
    
        public void AppendTo(TextWriter writer)
        {
            writer.Write(_eventType.Name);
            writer.Write("(");
            String delimiter = "";
            foreach (FilterValueSetParam[] param in _parameters)
            {
                writer.Write(delimiter);
                AppendTo(writer, param);
                delimiter = " or ";
            }
            writer.Write(")");
        }

        private void AppendTo(TextWriter writer, FilterValueSetParam[] parameters)
        {
            String delimiter = "";
            foreach (FilterValueSetParam param in parameters)
            {
                writer.Write(delimiter);
                param.AppendTo(writer);
                delimiter = ",";
            }
        }
    }
}
