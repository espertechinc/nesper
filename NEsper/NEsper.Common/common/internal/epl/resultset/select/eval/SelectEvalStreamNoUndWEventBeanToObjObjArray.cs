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
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamNoUndWEventBeanToObjObjArray : SelectEvalStreamBaseObjectArray
    {
        private readonly int[] eventBeanToObjectIndexesArray;

        public SelectEvalStreamNoUndWEventBeanToObjObjArray(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            IList<SelectClauseStreamCompiledSpec> namedStreams,
            bool usingWildcard,
            ISet<string> eventBeanToObjectProps)
            : base(selectExprForgeContext, resultEventType, namedStreams, usingWildcard)

        {
            var eventBeanToObjectIndexes = new HashSet<int>();
            var type = (ObjectArrayEventType) resultEventType;
            foreach (var name in eventBeanToObjectProps) {
                if (type.PropertiesIndexes.TryGetValue(name, out var index)) {
                    eventBeanToObjectIndexes.Add(index);
                }
            }

            eventBeanToObjectIndexesArray = eventBeanToObjectIndexes.ToArray();
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpressionRef props,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(SelectEvalStreamNoUndWEventBeanToObjObjArray),
                "ProcessSelectExprbeanToObjArray",
                props,
                Constant(eventBeanToObjectIndexesArray),
                eventBeanFactory,
                resultEventType);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="props">props</param>
        /// <param name="eventBeanToObjectIndexes">indexes</param>
        /// <param name="eventBeanTypedEventFactory">svc</param>
        /// <param name="resultEventType">type</param>
        /// <returns>bean</returns>
        public static EventBean ProcessSelectExprbeanToObjArray(
            object[] props,
            int[] eventBeanToObjectIndexes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType resultEventType)
        {
            foreach (var propertyIndex in eventBeanToObjectIndexes) {
                var value = props[propertyIndex];
                if (value is EventBean) {
                    props[propertyIndex] = ((EventBean) value).Underlying;
                }
            }

            return eventBeanTypedEventFactory.AdapterForTypedObjectArray(props, resultEventType);
        }
    }
} // end of namespace