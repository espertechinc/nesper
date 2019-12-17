///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>
    /// Implementation that does not convert columns.
    /// </summary>
    public class DeliveryConvertorNullWStatement : DeliveryConvertor
    {
        private readonly EPStatement _statement;

        public DeliveryConvertorNullWStatement(EPStatement statement)
        {
            this._statement = statement;
        }

        public object[] ConvertRow(object[] columns)
        {
            var deliver = new object[columns.Length + 1];
            deliver[0] = _statement;
            Array.Copy(columns, 0, deliver, 1, columns.Length);
            return deliver;
        }
    }
} // end of namespace