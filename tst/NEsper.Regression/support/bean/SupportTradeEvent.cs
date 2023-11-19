///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportTradeEvent
    {
        public SupportTradeEvent(
            int id,
            string userId,
            string ccypair,
            string direction)
        {
            Id = id;
            UserId = userId;
            Ccypair = ccypair;
            Direction = direction;
        }

        public SupportTradeEvent(
            int id,
            string userId,
            int amount)
        {
            Id = id;
            UserId = userId;
            Amount = amount;
        }

        public int Id { get; }

        public string UserId { get; }

        public string Ccypair { get; }

        public string Direction { get; }

        public int Amount { get; }

        public override string ToString()
        {
            return "Id=" +
                   Id +
                   " UserId=" +
                   UserId +
                   " Ccypair=" +
                   Ccypair +
                   " direction=" +
                   Direction;
        }
    }
} // end of namespace