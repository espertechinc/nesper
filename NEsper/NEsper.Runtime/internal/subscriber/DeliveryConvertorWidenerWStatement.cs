///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    /// Implementation of a convertor for column results that renders the result as an object array itself.
    /// </summary>
    public class DeliveryConvertorWidenerWStatement : DeliveryConvertor
    {
        private readonly TypeWidener[] _wideners;
        private readonly EPStatement _statement;

        public DeliveryConvertorWidenerWStatement(TypeWidener[] wideners, EPStatement statement)
        {
            _wideners = wideners;
            _statement = statement;
        }

        public object[] ConvertRow(object[] columns)
        {
            var values = new object[columns.Length + 1];
            values[0] = _statement;
            var offset = 1;
            for (var i = 0; i < columns.Length; i++)
            {
                if (_wideners[i] == null)
                {
                    values[offset] = columns[i];
                }
                else
                {
                    values[offset] = _wideners[i].Widen(columns[i]);
                }
                offset++;
            }
            return values;
        }
    }
} // end of namespace