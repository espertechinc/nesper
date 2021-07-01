///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class MatchedEventMapMeta
    {
        private const int MIN_MAP_LOOKUP = 3;

        private readonly IDictionary<string, int> _tagsPerIndexMap;

        public MatchedEventMapMeta(
            string[] tagsPerIndex,
            EventType[] eventTypes,
            string[] arrayTags)
        {
            TagsPerIndex = tagsPerIndex;
            EventTypes = eventTypes;
            ArrayTags = arrayTags;
            _tagsPerIndexMap = GetMap(tagsPerIndex);
        }

        public string[] TagsPerIndex { get; }

        public EventType[] EventTypes { get; }

        public bool HasArrayProperties => ArrayTags != null;

        public string[] ArrayTags { get; }

        public string[] NonArrayTags {
            get {
                if (!HasArrayProperties) {
                    return TagsPerIndex;
                }

                var result = new string[TagsPerIndex.Length - ArrayTags.Length];
                var count = 0;
                for (var i = 0; i < TagsPerIndex.Length; i++) {
                    var isArray = false;
                    for (var j = 0; j < ArrayTags.Length; j++) {
                        if (ArrayTags[j].Equals(TagsPerIndex[i])) {
                            isArray = true;
                            break;
                        }
                    }

                    if (!isArray) {
                        result[count++] = TagsPerIndex[i];
                    }
                }

                return result;
            }
        }

        public int GetTagFor(string key)
        {
            if (_tagsPerIndexMap != null) {
                if (_tagsPerIndexMap.TryGetValue(key, out var result)) {
                    return result;
                }

                return -1;
            }

            for (var i = 0; i < TagsPerIndex.Length; i++) {
                if (TagsPerIndex[i].Equals(key)) {
                    return i;
                }
            }

            return -1;
        }

        private IDictionary<string, int> GetMap(string[] tagsPerIndex)
        {
            if (tagsPerIndex.Length < MIN_MAP_LOOKUP) {
                return null;
            }

            IDictionary<string, int> map = new Dictionary<string, int>();
            for (var i = 0; i < tagsPerIndex.Length; i++) {
                map.Put(tagsPerIndex[i], i);
            }

            return map;
        }

        public CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(MatchedEventMapMeta), GetType(), classScope);
            method.Block.DeclareVar<string[]>("tagsPerIndex", Constant(TagsPerIndex))
                .DeclareVar<EventType[]>(
                    "eventTypes",
                    EventTypeUtility.ResolveTypeArrayCodegen(EventTypes, symbols.GetAddInitSvc(method)))
                .MethodReturn(
                    NewInstance<MatchedEventMapMeta>(
                        Ref("tagsPerIndex"),
                        Ref("eventTypes"),
                        Constant(ArrayTags)));
            return method;
        }

        public object GetEventTypeForTag(string tag)
        {
            return EventTypes[GetTagFor(tag)];
        }
    }
} // end of namespace