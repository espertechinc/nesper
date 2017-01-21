///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.rowregex
{
	public class MatchRecognizeStatePoolStmtHandler
    {
	    private int _count;

	    public int Count
	    {
	        get { return _count; }
	    }

	    public void DecreaseCount() {
	        _count--;
	        if (_count < 0) {
	            _count = 0;
	        }
	    }

	    public void DecreaseCount(int num) {
	        _count-=num;
	        if (_count < 0) {
	            _count = 0;
	        }
	    }

	    public void IncreaseCount() {
	        _count++;
	    }
	}
} // end of namespace
