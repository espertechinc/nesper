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
    public class Review
    {
        public Review(
            int reviewId,
            string comment)
        {
            ReviewId = reviewId;
            Comment = comment;
        }

        public int ReviewId { get; }

        public string Comment { get; }
    }
} // end of namespace