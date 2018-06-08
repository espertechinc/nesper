///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean.bookexample
{
    [Serializable]
    public class Order
    {
        private String orderId;
        private OrderItem[] items;
    
        public Order(String orderId, OrderItem[] items)
        {
            this.items = items;
            this.orderId = orderId;
        }

        public OrderItem[] Items
        {
            get { return items; }
        }

        public string OrderId
        {
            get { return orderId; }
        }
    }
}
