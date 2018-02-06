///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.aggregator
{
    public class AggregatorNthFilter : AggregatorNth
    {
        public AggregatorNthFilter(int sizeBuf) : base(sizeBuf)
        {
        }

        public override void Enter(object value)
        {
            var arr = (object[]) value;
            var pass = arr[arr.Length - 1];
            if (true.Equals(pass)) EnterValues(arr);
        }

        public override void Leave(object value)
        {
            var arr = (object[]) value;
            var pass = arr[arr.Length - 1];
            if (true.Equals(pass)) base.Leave(arr);
        }
    }
} // end of namespace