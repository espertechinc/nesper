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
    public class BookDesc
    {
        public BookDesc(string bookId, string title, string author, double price, Review[] reviews)
        {
            BookId = bookId;
            Title = title;
            Author = author;
            Price = price;
            Reviews = reviews;
        }

        public string BookId { get; private set; }

        public string Title { get; private set; }

        public string Author { get; private set; }

        public double Price { get; private set; }
        
        public Review[] Reviews { get; private set; }

    }
}
