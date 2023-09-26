///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.client.hook.aggfunc;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportConcatWManagedAggregationFunction : AggregationFunction
    {
        private const char DELIMITER = ' ';
        private StringBuilder builder;
        private string delimiter;

        public SupportConcatWManagedAggregationFunction()
        {
            builder = new StringBuilder();
            delimiter = "";
        }

        public SupportConcatWManagedAggregationFunction(StringBuilder builder)
        {
            this.builder = builder;
            this.delimiter = Convert.ToString(DELIMITER);
        }

        public void Enter(object value)
        {
            if (value != null) {
                builder.Append(delimiter);
                builder.Append(value);
                delimiter = Convert.ToString(DELIMITER);
            }
        }

        public void Leave(object value)
        {
            if (value != null) {
                builder.Remove(0, value.ToString().Length + 1);
            }
        }

        public object Value => builder.ToString();

        public void Clear()
        {
            builder = new StringBuilder();
            delimiter = "";
        }
    }
} // end of namespace