///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerPlanValidationResult
    {
        public OnTriggerPlanValidationResult(
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges,
            ResultSetProcessorDesc resultSetProcessorPrototype,
            ExprNode validatedJoin,
            string zeroStreamAliasName)
        {
            SubselectForges = subselectForges;
            TableAccessForges = tableAccessForges;
            ResultSetProcessorPrototype = resultSetProcessorPrototype;
            ValidatedJoin = validatedJoin;
            ZeroStreamAliasName = zeroStreamAliasName;
        }

        public IDictionary<ExprSubselectNode, SubSelectFactoryForge> SubselectForges { get; }

        public IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> TableAccessForges { get; }

        public ResultSetProcessorDesc ResultSetProcessorPrototype { get; }

        public ExprNode ValidatedJoin { get; }

        public string ZeroStreamAliasName { get; }
    }
} // end of namespace