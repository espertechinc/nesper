///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Converts from a map of prior matching events to a events per stream for resultion by expressions.
    /// </summary>
    public class MatchedEventConvertorForge
    {
        private readonly ISet<string> allTags;
        private readonly IDictionary<string, Pair<EventType, string>> arrayEventTypes;
        private readonly IDictionary<string, Pair<EventType, string>> filterTypes;

        public MatchedEventConvertorForge(
            IDictionary<string, Pair<EventType, string>> filterTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ISet<string> allTags)
        {
            this.filterTypes = new LinkedHashMap<string, Pair<EventType, string>>(filterTypes);
            if (arrayEventTypes != null) {
                this.arrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>(arrayEventTypes);
            }
            else {
                this.arrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            }

            this.allTags = allTags;
        }

        public CodegenMethod Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var size = filterTypes.Count + arrayEventTypes.Count;
            var method = parent.MakeChild(typeof(EventBean[]), GetType(), classScope)
                .AddParam(typeof(MatchedEventMap), "mem");
            if (size == 0) {
                method.Block.MethodReturn(PublicConstValue(typeof(CollectionUtil), "EVENTBEANARRAY_EMPTY"));
                return method;
            }

            method.Block
                .DeclareVar<EventBean[]>("events", NewArrayByLength(typeof(EventBean), Constant(size)))
                .DeclareVar<object[]>("buf", ExprDotMethod(Ref("mem"), "getMatchingEvents"));

            var count = 0;
            foreach (var entry in filterTypes) {
                var indexTag = FindTag(allTags, entry.Key);
                method.Block.AssignArrayElement(
                    Ref("events"),
                    Constant(count),
                    Cast(typeof(EventBean), ArrayAtIndex(Ref("buf"), Constant(indexTag))));
                count++;
            }

            foreach (var entry in arrayEventTypes) {
                var indexTag = FindTag(allTags, entry.Key);
                method.Block
                    .DeclareVar<EventBean[]>(
                        "arr" + count,
                        Cast(typeof(EventBean[]), ArrayAtIndex(Ref("buf"), Constant(indexTag))))
                    .DeclareVar<IDictionary<object, object>>(
                        "map" + count,
                        StaticMethod(typeof(Collections), "singletonMap", Constant(entry.Key), Ref("arr" + count)))
                    .AssignArrayElement(
                        Ref("events"),
                        Constant(count),
                        NewInstance<MapEventBean>(Ref("map" + count), ConstantNull()));
                count++;
            }

            method.Block.MethodReturn(Ref("events"));
            return method;
        }

        private int FindTag(
            ISet<string> allTags,
            string tag)
        {
            var index = 0;
            foreach (var oneTag in allTags) {
                if (tag.Equals(oneTag)) {
                    return index;
                }

                index++;
            }

            throw new IllegalStateException("Unexpected tag '" + tag + "'");
        }

        public CodegenExpression MakeAnonymous(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var clazz = NewAnonymousClass(method.Block, typeof(MatchedEventConvertor));
            var convert = CodegenMethod.MakeParentNode(typeof(EventBean[]), GetType(), classScope)
                .AddParam(typeof(MatchedEventMap), "events");
            clazz.AddMethod("convert", convert);
            convert.Block.MethodReturn(LocalMethod(Make(convert, classScope), Ref("events")));
            return clazz;
        }
    }
} // end of namespace