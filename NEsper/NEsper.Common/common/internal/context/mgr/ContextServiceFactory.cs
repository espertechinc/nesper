///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.context.cpidsvc;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.serde;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public interface ContextServiceFactory
    {
        ContextStatementEventEvaluator ContextStatementEventEvaluator { get; }
        ContextControllerKeyedFactory KeyedFactory();

        ContextControllerCategoryFactory CategoryFactory();

        ContextControllerHashFactory HashFactory();

        ContextControllerInitTermFactory InitTermFactory();

        ContextPartitionIdService GetContextPartitionIdService(
            StatementContext statementContextCreateContext,
            DataInputOutputSerdeWCollation<object>[] bindings);

        DataInputOutputSerdeWCollation<object>[] GetContextPartitionKeyBindings(
            ContextDefinition contextDefinition);
    }
} // end of namespace