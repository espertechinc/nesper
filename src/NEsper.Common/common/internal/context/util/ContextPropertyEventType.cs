///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextPropertyEventType
    {
        public const string PROP_CTX_NAME = "name";
        public const string PROP_CTX_ID = "id";
        public const string PROP_CTX_LABEL = "label";
        public const string PROP_CTX_STARTTIME = "startTime";
        public const string PROP_CTX_ENDTIME = "endTime";
        public const string PROP_CTX_KEY_PREFIX = "key";
        public const string PROP_CTX_KEY_PREFIX_SINGLE = "key1";

        public static void AddEndpointTypes(
            ContextSpecCondition endpoint,
            IDictionary<string, object> properties,
            ISet<string> allTags)
        {
            if (endpoint is ContextSpecConditionFilter filter) {
                if (filter.OptionalFilterAsName != null) {
                    allTags.Add(filter.OptionalFilterAsName);
                    properties.Put(filter.OptionalFilterAsName, filter.FilterSpecCompiled.FilterForEventType);
                }
            }

            if (endpoint is ContextSpecConditionPattern pattern) {
                if (pattern.AsName == null) {
                    foreach (var entry in pattern.PatternCompiled.TaggedEventTypes) {
                        if (properties.ContainsKey(entry.Key) && !properties.Get(entry.Key).Equals(entry.Value.First)) {
                            throw new ExprValidationException(
                                "The stream or tag name '" + entry.Key + "' is already declared");
                        }

                        allTags.Add(entry.Key);
                        properties.Put(entry.Key, entry.Value.First);
                    }
                }
                else {
                    if (properties.ContainsKey(pattern.AsName) || allTags.Contains(pattern.AsName)) {
                        throw new ExprValidationException(
                            "The stream or tag name '" + pattern.AsName + "' is already declared");
                    }

                    if (pattern.AsNameEventType == null) {
                        throw new IllegalStateException("no event type assigned");
                    }

                    properties.Put(pattern.AsName, pattern.AsNameEventType);
                    allTags.Add(pattern.AsName);
                }
            }
        }

        public static int GetStreamNumberForNestingLevel(
            int nestingLevel,
            bool startCondition)
        {
            return nestingLevel * 10 + (startCondition ? 0 : 1);
        }
    }
} // end of namespace