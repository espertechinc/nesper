///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.support.bean
{
	public class SupportPositionReport
	{
	    private int _VID;
        private int _timestamp;
        private int _spd;
        private int _seg;

	    public SupportPositionReport(int VID, int timestamp, int spd, int seg)
	    {
            this._VID = VID;
            this._timestamp = timestamp;
            this._spd = spd;
            this._seg = seg;
	    }

	    public int VID
	    {
            get {return _VID;}
	    }

	    public int Timestamp
	    {
            get {return _timestamp;}
	    }

	    public int Spd
	    {
            get {return _spd;}
	    }

	    public int Seg
	    {
            get {return _seg;}
	    }
	}
} // End of namespace
