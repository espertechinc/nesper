///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.fireandforget;

namespace com.espertech.esper.regressionlib.support.util
{
    public delegate void IndexAssertionFAF(EPFireAndForgetQueryResult result);

#if DEPRECATED_INTERFACE
    public interface IndexAssertionFAF {
	    void Run(EPFireAndForgetQueryResult result);
	}
#endif
} // end of namespace