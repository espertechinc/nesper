///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.keyed;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextPropertyRegistry
    {
        public const string CONTEXT_PREFIX = "context";

        private readonly ContextControllerPortableInfo[] controllerValidations;

        public ContextPropertyRegistry(ContextMetaData metaData)
        {
            ContextEventType = metaData.EventType;
            controllerValidations = metaData.ValidationInfos;
        }

        public EventType ContextEventType { get; }

        public bool IsPartitionProperty(
            EventType fromType,
            string propertyName)
        {
            var name = GetPartitionContextPropertyName(fromType, propertyName);
            return name != null;
        }

        public string GetPartitionContextPropertyName(
            EventType fromType,
            string propertyName)
        {
            if (controllerValidations.Length == 1) {
                if (controllerValidations[0] is ContextControllerKeyedValidation) {
                    var partitioned = (ContextControllerKeyedValidation)controllerValidations[0];
                    foreach (var item in partitioned.Items) {
                        if (item.EventType == fromType) {
                            for (var i = 0; i < item.PropertyNames.Length; i++) {
                                if (item.PropertyNames[i].Equals(propertyName)) {
                                    return ContextPropertyEventType.PROP_CTX_KEY_PREFIX + (i + 1);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public bool IsContextPropertyPrefix(string prefixName)
        {
            return prefixName != null &&
                   prefixName.ToLowerInvariant() == CONTEXT_PREFIX;
        }
    }
} // end of namespace