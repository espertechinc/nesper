///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.events
{
	public class ValueWithExistsFlag
    {
	    private readonly bool _exists;
	    private readonly object _value;

	    private ValueWithExistsFlag(bool exists, object value) {
	        _exists = exists;
	        _value = value;
	    }

	    public bool IsExists() {
	        return _exists;
	    }

	    public object GetValue() {
	        return _value;
	    }

	    public static ValueWithExistsFlag NotExists() {
	        return new ValueWithExistsFlag(false, null);
	    }

	    public static ValueWithExistsFlag Exists(object value) {
	        return new ValueWithExistsFlag(true, value);
	    }

	    public static ValueWithExistsFlag[] MultipleNotExists(int count) {
	        ValueWithExistsFlag[] flagged = new ValueWithExistsFlag[count];
	        for (int i = 0; i < flagged.Length; i++) {
	            flagged[i] = NotExists();
	        }
	        return flagged;
	    }

	    public static ValueWithExistsFlag[] AllExist(params object[] values) {
	        ValueWithExistsFlag[] flagged = new ValueWithExistsFlag[values.Length];
	        for (int i = 0; i < values.Length; i++) {
	            flagged[i] = Exists(values[i]);
	        }
	        return flagged;
	    }
	}
} // end of namespace
