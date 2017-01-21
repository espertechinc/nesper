///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.named
{
    public interface NamedWindowConsumerMgmtService
    {
	    void AddConsumer(StatementContext statementContext, NamedWindowConsumerStreamSpec namedSpec);
	    void Start(string statementName);
	    void Stop(string statementName);
	    void Destroy(string statementName);
	    void RemoveReferences(string statementName);
	}
} // end of namespace
