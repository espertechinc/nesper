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
    public class GameDesc
    {
        public GameDesc(
            string gameId,
            string title,
            string publisher,
            Review[] reviews)
        {
            Publisher = publisher;
            GameId = gameId;
            Title = title;
            Reviews = reviews;
        }

        public string GameId { get; }

        public string Title { get; }

        public string Publisher { get; }

        public Review[] Reviews { get; }
    }
} // end of namespace