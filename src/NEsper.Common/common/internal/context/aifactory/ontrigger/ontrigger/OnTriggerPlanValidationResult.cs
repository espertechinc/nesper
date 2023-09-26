///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.fabric;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.ontrigger
{
    public class OnTriggerPlanValidationResult
    {
        private readonly IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges;
        private readonly IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges;
        private readonly ResultSetProcessorDesc resultSetProcessorPrototype;
        private readonly ExprNode validatedJoin;
        private readonly string zeroStreamAliasName;
        private readonly IList<StmtClassForgeableFactory> additionalForgeables;
        private readonly FabricCharge fabricCharge;

        public OnTriggerPlanValidationResult(
            IDictionary<ExprSubselectNode, SubSelectFactoryForge> subselectForges,
            IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> tableAccessForges,
            ResultSetProcessorDesc resultSetProcessorPrototype,
            ExprNode validatedJoin,
            string zeroStreamAliasName,
            IList<StmtClassForgeableFactory> additionalForgeables,
            FabricCharge fabricCharge)
        {
            this.subselectForges = subselectForges;
            this.tableAccessForges = tableAccessForges;
            this.resultSetProcessorPrototype = resultSetProcessorPrototype;
            this.validatedJoin = validatedJoin;
            this.zeroStreamAliasName = zeroStreamAliasName;
            this.additionalForgeables = additionalForgeables;
            this.fabricCharge = fabricCharge;
        }

        public IDictionary<ExprSubselectNode, SubSelectFactoryForge> SubselectForges => subselectForges;

        public IDictionary<ExprTableAccessNode, ExprTableEvalStrategyFactoryForge> TableAccessForges =>
            tableAccessForges;

        public ResultSetProcessorDesc ResultSetProcessorPrototype => resultSetProcessorPrototype;

        public ExprNode ValidatedJoin => validatedJoin;

        public string ZeroStreamAliasName => zeroStreamAliasName;

        public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace