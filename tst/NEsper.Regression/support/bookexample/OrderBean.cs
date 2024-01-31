///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bookexample
{
    public class OrderBean
    {
        public OrderBean(
            OrderWithItems order,
            BookDesc[] books,
            GameDesc[] games)
        {
            Books = books;
            Games = games;
            OrderDetail = order;
        }

        public BookDesc[] Books { get; }

        public OrderWithItems OrderDetail { get; }

        public GameDesc[] Games { get; }
    }
} // end of namespace