///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.unique
{
    /// <summary>
    ///     Factory for <seealso cref="UniqueByPropertyView" /> instances.
    /// </summary>
    public class UniqueByPropertyViewFactory : DataWindowViewFactory
    {
        public const string NAME = "Unique-By";

        public EventType EventType { get; set; }

        public ExprEvaluator CriteriaEval { get; set; }

        public Type[] CriteriaTypes { get; set; }

        public DataInputOutputSerde<object> KeySerde { get; set; }

        public string ViewName => ViewEnum.UNIQUE_BY_PROPERTY.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new UniqueByPropertyView(this, agentInstanceViewFactoryContext);
        }
    }
} // end of namespace