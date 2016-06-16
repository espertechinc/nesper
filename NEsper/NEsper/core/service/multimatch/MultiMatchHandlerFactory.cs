///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.service.multimatch
{
	public interface MultiMatchHandlerFactory
    {
        MultiMatchHandler GetDefaultHandler();
        MultiMatchHandler MakeNoDedupNoSubq();
        MultiMatchHandler MakeNoDedupSubselectPreval();
        MultiMatchHandler MakeNoDedupSubselectPosteval();
        MultiMatchHandler MakeDedupNoSubq();
        MultiMatchHandler MakeDedupSubq(bool isSubselectPreeval);
    }
} // end of namespace
