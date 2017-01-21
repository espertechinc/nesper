///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.pattern
{
	/// <summary>
    /// Used for anything that requires to be informed of matching events which would be stored
	/// in the MatchedEventMap structure passed to the implementation.
	/// </summary>

    public delegate void PatternMatchCallback(IDictionary<String, Object> matchEvent);
}
