///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamNoUndWEventBeanToObj : SelectEvalStreamBaseMap
    {
        private readonly string[] eventBeanToObjectProps;

        public SelectEvalStreamNoUndWEventBeanToObj(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard,
            ISet<string> eventBeanToObjectProps)
            : base(selectExprForgeContext, resultEventType, namedStreams, usingWildcard)

        {
            this.eventBeanToObjectProps = eventBeanToObjectProps.ToArray();
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(SelectEvalStreamNoUndWEventBeanToObj),
                "ProcessSelectExprbeanToMap",
                props,
                Constant(eventBeanToObjectProps),
                eventBeanFactory,
                resultEventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="props">props</param>
        /// <param name="eventBeanToObjectProps">indexes</param>
        /// <param name="eventAdapterService">svc</param>
        /// <param name="resultEventType">type</param>
        /// <returns>bean</returns>
        public static EventBean ProcessSelectExprbeanToMap(
            IDictionary<string, object> props,
            string[] eventBeanToObjectProps,
            EventBeanTypedEventFactory eventAdapterService,
            EventType resultEventType)
        {
            foreach (var property in eventBeanToObjectProps) {
                var value = props.Get(property);
                if (value is EventBean bean) {
                    props.Put(property, bean.Underlying);
                }
            }

            return eventAdapterService.AdapterForTypedMap(props, resultEventType);
        }
    }
} // end of namespace