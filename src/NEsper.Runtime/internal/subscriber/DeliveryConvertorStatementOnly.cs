///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    public class DeliveryConvertorStatementOnly : DeliveryConvertor
    {
        private readonly EPStatement statement;

        public DeliveryConvertorStatementOnly(EPStatement statement)
        {
            this.statement = statement;
        }

        public object[] ConvertRow(object[] row)
        {
            return new object[] { statement };
        }
    }
} // end of namespace