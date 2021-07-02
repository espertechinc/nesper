///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethodWithArrayMapForge : EventBeanCopyMethodForge
    {
        private readonly int[] _arrayIndexes;
        private readonly ObjectArrayEventType _eventType;
        private readonly int[] _mapIndexes;

        public ObjectArrayEventBeanCopyMethodWithArrayMapForge(
            ObjectArrayEventType eventType,
            ISet<string> mapPropertiesToCopy,
            ISet<string> arrayPropertiesToCopy,
            IDictionary<string, int> propertiesIndexes)
        {
            this._eventType = eventType;

            ISet<int> mapIndexesToCopy = new HashSet<int>();
            foreach (var prop in mapPropertiesToCopy) {
                if (propertiesIndexes.TryGetValue(prop, out var index)) {
                    mapIndexesToCopy.Add(index);
                }
            }

            _mapIndexes = IntArrayUtil.ToArray(mapIndexesToCopy);

            ISet<int> arrayIndexesToCopy = new HashSet<int>();
            foreach (var prop in arrayPropertiesToCopy) {
                if (propertiesIndexes.TryGetValue(prop, out var index)) {
                    arrayIndexesToCopy.Add(index);
                }
            }

            _arrayIndexes = IntArrayUtil.ToArray(arrayIndexesToCopy);
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance<ObjectArrayEventBeanCopyMethodWithArrayMap>(
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF)),
                factory,
                Constant(_mapIndexes),
                Constant(_arrayIndexes));
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new ObjectArrayEventBeanCopyMethodWithArrayMap(
                _eventType,
                eventBeanTypedEventFactory,
                _mapIndexes,
                _arrayIndexes);
        }
    }
} // end of namespace