///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bookexample
{
    [Serializable]
    public class OrderWithItems
    {
        public OrderWithItems(
            string orderId,
            OrderItem[] items)
        {
            Items = items;
            OrderId = orderId;
        }

        public OrderItem[] Items { get; }

        public string OrderId { get; }
    }
} // end of namespace