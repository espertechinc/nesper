///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bookexample
{
    public class BookDesc
    {
        public BookDesc(
            string bookId,
            string title,
            string author,
            double price,
            Review[] reviews)
        {
            Author = author;
            BookId = bookId;
            Title = title;
            Price = price;
            Reviews = reviews;
        }

        public string Author { get; }

        public string BookId { get; }

        public string Title { get; }

        public Review[] Reviews { get; }

        public double Price { get; }
    }
} // end of namespace