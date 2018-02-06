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
    public class OrderItem
    {
        private String itemId;
        private String productId;
        private int amount;
        private double price;
    
        public OrderItem(String itemId, String productId, int amount, double price)
        {
            this.itemId = itemId;
            this.amount = amount;
            this.productId = productId;
            this.price = price;
        }

        public string ItemId
        {
            get { return itemId; }
        }

        public int Amount
        {
            get { return amount; }
        }

        public string ProductId
        {
            get { return productId; }
        }

        public double Price
        {
            get { return price; }
        }
    }
}
