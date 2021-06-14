///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    /// <summary>Implementation that does not convert columns. </summary>
    public class DeliveryConvertorNull : DeliveryConvertor
    {
        public static readonly DeliveryConvertorNull INSTANCE =
            new DeliveryConvertorNull();

        private DeliveryConvertorNull()
        {
        }

        public Object[] ConvertRow(Object[] columns)
        {
            return columns;
        }
    }
}