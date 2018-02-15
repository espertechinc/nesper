///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
	[Serializable]
    public class SupportTradeEvent
	{
	    private int _id;
        private String _userId;
        private String _ccypair;
        private String _direction;
	    private int _amount;

	    public SupportTradeEvent(int id, String userId, String ccypair, String direction)
	    {
            this._id = id;
            this._userId = userId;
            this._ccypair = ccypair;
            this._direction = direction;
	    }


        public SupportTradeEvent(int id, String userId, int amount)
        {
            this._id = id;
            this._userId = userId;
            this._amount = amount;
        }

	    public int Id
	    {
            get { return _id; }
	    }

	    public String UserId
	    {
            get {return _userId;}
	    }

	    public String CCYPair
	    {
            get {return _ccypair;}
	    }

	    public String Direction
	    {
            get {return _direction;}
	    }

        public int Amount
        {
            get { return _amount; }
        }

        public override String ToString()
	    {
            return "id=" + _id +
                   " userId=" + _userId +
                   " ccypair=" + _ccypair +
                   " direction=" + _direction;
	    }
	}
} // End of namespace
