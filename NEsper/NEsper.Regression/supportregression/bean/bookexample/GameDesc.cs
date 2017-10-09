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
    public class GameDesc
    {
        private readonly String gameId;
        private readonly String title;
        private readonly String publisher;
        private readonly Review[] reviews;
    
        public GameDesc(String gameId, String title, String publisher, Review[] reviews)
        {
            this.publisher = publisher;
            this.gameId = gameId;
            this.title = title;
            this.reviews = reviews;
        }

        public string GameId
        {
            get { return gameId; }
        }

        public string Title
        {
            get { return title; }
        }

        public string Publisher
        {
            get { return publisher; }
        }

        public Review[] Reviews
        {
            get { return reviews; }
        }
    }
}
