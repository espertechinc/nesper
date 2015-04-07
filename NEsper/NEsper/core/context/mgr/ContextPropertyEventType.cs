///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextPropertyEventType
    {
        public static readonly String PROP_CTX_NAME = "name";
        public static readonly String PROP_CTX_ID = "id";
        public static readonly String PROP_CTX_LABEL = "label";
        public static readonly String PROP_CTX_STARTTIME = "startTime";
        public static readonly String PROP_CTX_ENDTIME = "endTime";
        public static readonly String PROP_CTX_KEY_PREFIX = "key";

        private readonly static List<ContextProperty> LIST_INITIATEDTERM_PROPS;
        private readonly static List<ContextProperty> LIST_CATEGORY_PROPS;
        private readonly static List<ContextProperty> LIST_PARTITION_PROPS;
        private readonly static List<ContextProperty> LIST_HASH_PROPS;
        private readonly static List<ContextProperty> LIST_NESTED_PROPS;

        static ContextPropertyEventType()
        {
            LIST_INITIATEDTERM_PROPS = new List<ContextProperty>();
            LIST_INITIATEDTERM_PROPS.Add(new ContextProperty(PROP_CTX_ID, typeof(int)));
            LIST_INITIATEDTERM_PROPS.Add(new ContextProperty(PROP_CTX_NAME, typeof(String)));
            LIST_INITIATEDTERM_PROPS.Add(new ContextProperty(PROP_CTX_STARTTIME, typeof(long)));
            LIST_INITIATEDTERM_PROPS.Add(new ContextProperty(PROP_CTX_ENDTIME, typeof(long)));

            LIST_CATEGORY_PROPS = new List<ContextProperty>
            {
                new ContextProperty(PROP_CTX_NAME, typeof (String)),
                new ContextProperty(PROP_CTX_ID, typeof (int)),
                new ContextProperty(PROP_CTX_LABEL, typeof (String))
            };

            LIST_PARTITION_PROPS = new List<ContextProperty>
            {
                new ContextProperty(PROP_CTX_NAME, typeof (String)),
                new ContextProperty(PROP_CTX_ID, typeof (int))
            };

            LIST_HASH_PROPS = new List<ContextProperty>
            {
                new ContextProperty(PROP_CTX_NAME, typeof (String)),
                new ContextProperty(PROP_CTX_ID, typeof (int))
            };

            LIST_NESTED_PROPS = new List<ContextProperty>
            {
                new ContextProperty(PROP_CTX_NAME, typeof (String)),
                new ContextProperty(PROP_CTX_ID, typeof (int))
            };
        }

        public static IDictionary<String, Object> GetCategorizedType()
        {
            return MakeEventType(LIST_CATEGORY_PROPS, Collections.EmptyDataMap);
        }

        public static IDictionary<String, Object> GetCategorizedBean(String contextName, int agentInstanceId, String label)
        {
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put(PROP_CTX_NAME, contextName);
            props.Put(PROP_CTX_ID, agentInstanceId);
            props.Put(PROP_CTX_LABEL, label);
            return props;
        }

        public static IDictionary<string, object> GetInitiatedTerminatedType()
        {
            return MakeEventType(LIST_INITIATEDTERM_PROPS, Collections.EmptyDataMap);
        }

        public static void AddEndpointTypes(String contextName, ContextDetailCondition endpoint, IDictionary<String, Object> properties, ICollection<String> allTags)
        {
            if (endpoint is ContextDetailConditionFilter)
            {
                var filter = (ContextDetailConditionFilter)endpoint;
                if (filter.OptionalFilterAsName != null)
                {
                    if (properties.ContainsKey(filter.OptionalFilterAsName))
                    {
                        throw new ExprValidationException("For context '" + contextName + "' the stream or tag name '" + filter.OptionalFilterAsName + "' is already declared");
                    }
                    allTags.Add(filter.OptionalFilterAsName);
                    properties.Put(filter.OptionalFilterAsName, filter.FilterSpecCompiled.FilterForEventType);
                }
            }
            if (endpoint is ContextDetailConditionPattern)
            {
                var pattern = (ContextDetailConditionPattern)endpoint;
                foreach (var entry in pattern.PatternCompiled.TaggedEventTypes)
                {
                    if (properties.ContainsKey(entry.Key) && !properties.Get(entry.Key).Equals(entry.Value.First))
                    {
                        throw new ExprValidationException("For context '" + contextName + "' the stream or tag name '" + entry.Key + "' is already declared");
                    }
                    allTags.Add(entry.Key);
                    properties.Put(entry.Key, entry.Value.First);
                }
            }
        }

        public static IDictionary<String, Object> GetTempOverlapBean(String contextName, int agentInstanceId, IDictionary<String, Object> matchEvent, EventBean theEvent, String filterAsName)
        {
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put(PROP_CTX_NAME, contextName);
            props.Put(PROP_CTX_ID, agentInstanceId);
            if (matchEvent != null)
            {
                props.PutAll(matchEvent);
            }
            else
            {
                props.Put(filterAsName, theEvent);
            }
            return props;
        }

        public static IDictionary<String, Object> GetPartitionType(ContextDetailPartitioned segmentedSpec, Type[] propertyTypes)
        {
            IDictionary<String, Object> props = new LinkedHashMap<String, Object>();
            for (var i = 0; i < segmentedSpec.Items[0].PropertyNames.Count; i++)
            {
                var propertyName = PROP_CTX_KEY_PREFIX + (i + 1);
                props.Put(propertyName, propertyTypes[i]);
            }
            return MakeEventType(LIST_PARTITION_PROPS, props);
        }

        public static IDictionary<String, Object> GetPartitionBean(String contextName, int agentInstanceId, Object keyValue, IList<string> propertyNames)
        {
            Object[] agentInstanceProperties;
            if (propertyNames.Count == 1)
            {
                agentInstanceProperties = new[] { keyValue };
            }
            else
            {
                agentInstanceProperties = ((MultiKeyUntyped)keyValue).Keys;
            }

            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put(PROP_CTX_NAME, contextName);
            props.Put(PROP_CTX_ID, agentInstanceId);
            for (var i = 0; i < agentInstanceProperties.Length; i++)
            {
                var propertyName = PROP_CTX_KEY_PREFIX + (i + 1);
                props.Put(propertyName, agentInstanceProperties[i]);
            }
            return props;
        }

        public static IDictionary<String, Object> GetNestedTypeBase()
        {
            IDictionary<String, Object> props = new LinkedHashMap<String, Object>();
            return MakeEventType(LIST_NESTED_PROPS, props);
        }

        public static IDictionary<String, Object> GetNestedBeanBase(String contextName, int contextPartitionId)
        {
            IDictionary<String, Object> props = new LinkedHashMap<String, Object>();
            props.Put(PROP_CTX_NAME, contextName);
            props.Put(PROP_CTX_ID, contextPartitionId);
            return props;
        }

        public static IDictionary<String, Object> GetHashType()
        {
            return MakeEventType(LIST_HASH_PROPS, Collections.EmptyDataMap);
        }

        public static IDictionary<String, Object> GetHashBean(String contextName, int agentInstanceId)
        {
            IDictionary<String, Object> props = new Dictionary<String, Object>();
            props.Put(PROP_CTX_NAME, contextName);
            props.Put(PROP_CTX_ID, agentInstanceId);
            return props;
        }

        private static IDictionary<String, Object> MakeEventType(IEnumerable<ContextProperty> builtin, IDictionary<String, Object> additionalProperties)
        {
            IDictionary<String, Object> properties = new LinkedHashMap<String, Object>();
            properties.PutAll(additionalProperties);
            foreach (var prop in builtin)
            {
                properties.Put(prop.PropertyName, prop.PropertyType);
            }
            return properties;
        }

        public class ContextProperty
        {
            public ContextProperty(String propertyName, Type propertyType)
            {
                PropertyName = propertyName;
                PropertyType = propertyType;
            }

            public string PropertyName { get; private set; }

            public Type PropertyType { get; private set; }
        }
    }
}
