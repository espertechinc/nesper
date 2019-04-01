///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.supportregression.client
{
    [Serializable]
    public class MyConcatAggregationFunction : AggregationMethod
    {
        private const string DELIMITER = " ";
        private StringBuilder _builder;
        private String _delimiter;

        public MyConcatAggregationFunction()
        {
            _builder = new StringBuilder();
            _delimiter = "";
        }

        public void Clear()
        {
            _builder = new StringBuilder();
        }

        public void Enter(Object value)
        {
            if (value != null)
            {
                _builder.Append(_delimiter);
                _builder.Append(value.ToString());
                _delimiter = DELIMITER;
            }
        }

        public void Leave(Object value)
        {
            if (value != null)
            {
                _builder.Remove(0, value.ToString().Length + 1);
            }
        }

        public object Value
        {
            get { return _builder.ToString(); }
        }
    }
}
