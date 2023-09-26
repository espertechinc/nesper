///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.context.cpidsvc;
using com.espertech.esper.common.@internal.context.util;


namespace com.espertech.esper.common.@internal.context.mgr
{
    public interface ContextServiceFactory
    {
        ContextControllerKeyedFactory KeyedFactory(
            StateMgmtSetting terminationStateMgmtSettings,
            StateMgmtSetting ctxStateMgmtSettings);

        ContextControllerCategoryFactory CategoryFactory(StateMgmtSetting stateMgmtSettings);

        ContextControllerHashFactory HashFactory(StateMgmtSetting stateMgmtSettings);

        ContextControllerInitTermFactory InitTermFactory(
            StateMgmtSetting distinctStateMgmtSettings,
            StateMgmtSetting ctxStateMgmtSettings);

        ContextPartitionIdService GetContextPartitionIdService(
            StatementContext statementContextCreateContext,
            DataInputOutputSerde[] bindings,
            StateMgmtSetting stateMgmtSettings);

        DataInputOutputSerde[] GetContextPartitionKeyBindings(ContextDefinition contextDefinition);

        ContextStatementEventEvaluator ContextStatementEventEvaluator { get; }
    }
} // end of namespace