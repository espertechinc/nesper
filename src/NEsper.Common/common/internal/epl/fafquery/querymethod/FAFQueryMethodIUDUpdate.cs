///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.epl.updatehelper;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodIUDUpdate : FAFQueryMethodIUDBase
    {
        private Table table;

        public ExprTableAccessNode[] OptionalTableNodes { set; get; }

        public Table Table {
            set {
                table = value;
                try {
                    TableUpdateStrategy = TableUpdateStrategyFactory.ValidateGetTableUpdateStrategy(
                        value.MetaData,
                        UpdateHelperTable,
                        false);
                }
                catch (ExprValidationException e) {
                    throw new EPException(e.Message, e);
                }
            }
        }

        public ExprEvaluator OptionalWhereClause { get; set; }

        public EventBeanUpdateHelperWCopy UpdateHelperNamedWindow { get; set; }

        public override QueryGraph QueryGraph { get; set; }

        public EventBeanUpdateHelperNoCopy UpdateHelperTable { get; set; }

        public TableUpdateStrategy TableUpdateStrategy { get; private set; }

        protected override EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessUpdate(this);
        }
    }
} // end of namespace