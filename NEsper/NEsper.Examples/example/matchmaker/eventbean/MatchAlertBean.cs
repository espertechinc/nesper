///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace NEsper.Examples.MatchMaker.eventbean
{
    public class MatchAlertBean
    {
        private readonly int _otherUserId;
        private readonly int _selfUserId;

        public MatchAlertBean(int otherUserId, int selfUserId)
        {
            _otherUserId = otherUserId;
            _selfUserId = selfUserId;
        }

        public int SelfUserId
        {
            get { return _selfUserId; }
        }

        public int OtherUserId
        {
            get { return _otherUserId; }
        }
    }
}
