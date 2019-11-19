///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    /// Implementation of a convertor for column results that renders the result as an object array itself.
    /// </summary>
    public class DeliveryConvertorObjectArrWStatement : DeliveryConvertor
    {
        private readonly EPStatement _statement;

        public DeliveryConvertorObjectArrWStatement(EPStatement statement)
        {
            _statement = statement;
        }

        public object[] ConvertRow(object[] columns)
        {
            return new object[] { _statement, columns };
        }
    }
} // end of namespace