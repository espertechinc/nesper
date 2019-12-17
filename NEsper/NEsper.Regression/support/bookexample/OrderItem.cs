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
    public class OrderItem
    {
        public OrderItem(
            string itemId,
            string productId,
            int amount,
            double price)
        {
            ItemId = itemId;
            Amount = amount;
            ProductId = productId;
            Price = price;
        }

        public string ItemId { get; }

        public int Amount { get; }

        public string ProductId { get; }

        public double Price { get; }
    }
} // end of namespace