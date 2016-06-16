///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.dispatch;
using com.espertech.esper.timer;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
	public class EPStatementFactoryDefault : EPStatementFactory
	{
	    public EPStatementSPI Make(
	        string expressionNoAnnotations,
	        bool isPattern,
	        DispatchService dispatchService,
	        StatementLifecycleSvcImpl statementLifecycleSvc,
	        long timeLastStateChange,
	        bool preserveDispatchOrder,
	        bool isSpinLocks,
	        long blockingTimeout,
	        TimeSourceService timeSource,
	        StatementMetadata statementMetadata,
	        object statementUserObject,
	        StatementContext statementContext,
	        bool isFailed,
	        bool nameProvided)
	    {
	        return new EPStatementImpl(
	            expressionNoAnnotations, isPattern, dispatchService, statementLifecycleSvc, timeLastStateChange,
	            preserveDispatchOrder, isSpinLocks, blockingTimeout, timeSource, statementMetadata, statementUserObject,
	            statementContext, isFailed, nameProvided);
	    }

        public StopCallback MakeStopMethod(StatementAgentInstanceFactoryResult startResult)
        {
            return startResult.StopCallback;
        }
	}
} // end of namespace
