using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.supportregression.bean
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	public class SupportValueEvent {
	    private readonly int value;

	    public SupportValueEvent(int value) {
	        this.value = value;
	    }

	    public int GetValue() {
	        return value;
	    }
	}
} // end of namespace
