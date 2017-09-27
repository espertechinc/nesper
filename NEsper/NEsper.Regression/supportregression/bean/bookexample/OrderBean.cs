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
    public class OrderBean
    {
        private Order orderdetail;
        private BookDesc[] books;
        private GameDesc[] games;
    
        public OrderBean(Order order, BookDesc[] books, GameDesc[] games)
        {
            this.books = books;
            this.games = games;
            this.orderdetail = order;
        }

        public BookDesc[] Books
        {
            get { return books; }
        }

        public Order Orderdetail
        {
            get { return orderdetail; }
        }

        public GameDesc[] Games
        {
            get { return games; }
        }
    }
}
