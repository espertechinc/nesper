///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.named
{
	public class NamedWindowConsumerMgmtServiceImpl : NamedWindowConsumerMgmtService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    public readonly static NamedWindowConsumerMgmtServiceImpl INSTANCE = new NamedWindowConsumerMgmtServiceImpl();

	    private NamedWindowConsumerMgmtServiceImpl() {
	    }

	    public void AddConsumer(StatementContext statementContext, NamedWindowConsumerStreamSpec namedSpec) {
	        if (Log.IsDebugEnabled) {
	            Log.Debug("Statement '{0} registers consumer for '{1}'", statementContext.StatementName, namedSpec.WindowName);
	        }
	    }

	    public void Start(string statementName) {
	        if (Log.IsDebugEnabled) {
	            Log.Debug("Statement '{0} starts consuming", statementName);
	        }
	    }

	    public void Stop(string statementName) {
	        if (Log.IsDebugEnabled) {
	            Log.Debug("Statement '{0} stop consuming", statementName);
	        }
	    }

	    public void Destroy(string statementName) {
	        if (Log.IsDebugEnabled) {
	            Log.Debug("Statement '{0} destroyed", statementName);
	        }
	    }

	    public void RemoveReferences(string statementName) {
	        if (Log.IsDebugEnabled) {
	            Log.Debug("Statement '{0} removing references", statementName);
	        }
	    }
	}
} // end of namespace
