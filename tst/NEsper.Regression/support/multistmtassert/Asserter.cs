///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.multistmtassert
{
    public delegate void Asserter<T>(
        int eventIndex,
        string statementName,
        string failMessage);

#if DEPRECATED_INTERFACE
    public interface Asserter<T> {
	    void Assertion(int eventIndex, string statementName, string failMessage);
	}
#endif
} // end of namespace