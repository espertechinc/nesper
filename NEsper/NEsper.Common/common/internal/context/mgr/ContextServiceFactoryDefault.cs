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
    public class ContextServiceFactoryDefault : ContextServiceFactory
    {
        public static readonly ContextServiceFactoryDefault INSTANCE = new ContextServiceFactoryDefault();

        private ContextServiceFactoryDefault()
        {
        }

        public ContextControllerKeyedFactory KeyedFactory()
        {
            return new ContextControllerKeyedFactory();
        }

        public ContextControllerCategoryFactory CategoryFactory()
        {
            return new ContextControllerCategoryFactory();
        }

        public ContextControllerHashFactory HashFactory()
        {
            return new ContextControllerHashFactory();
        }

        public ContextControllerInitTermFactory InitTermFactory()
        {
            return new ContextControllerInitTermFactory();
        }

        public DataInputOutputSerdeWCollation<object>[] GetContextPartitionKeyBindings(
            ContextDefinition contextDefinition)
        {
            return null;
        }

        public ContextStatementEventEvaluator ContextStatementEventEvaluator =>
            ContextStatementEventEvaluatorDefault.INSTANCE;

        public ContextPartitionIdService GetContextPartitionIdService(
            StatementContext statementContextCreateContext, 
            DataInputOutputSerdeWCollation<object>[] bindings)
        {
            return new ContextPartitionIdServiceImpl();
        }
    }
} // end of namespace