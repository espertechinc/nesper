///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextPropertyRegistryImpl : ContextPropertyRegistry
    {
        public static readonly String CONTEXT_PREFIX = "context";
        public static ContextPropertyRegistry EMPTY_REGISTRY = new ContextPropertyRegistryImpl(null);

        private readonly EventType _contextEventType;
        private readonly IList<ContextDetailPartitionItem> _partitionProperties;

        public ContextPropertyRegistryImpl(IList<ContextDetailPartitionItem> partitionProperties,
                                           EventType contextEventType)
        {
            _partitionProperties = partitionProperties;
            _contextEventType = contextEventType;
        }

        public ContextPropertyRegistryImpl(EventType contextEventType)
        {
            _partitionProperties = new List<ContextDetailPartitionItem>();
            _contextEventType = contextEventType;
        }

        #region ContextPropertyRegistry Members

        public bool IsPartitionProperty(EventType fromType,
                                        String propertyName)
        {
            string name = GetPartitionContextPropertyName(fromType, propertyName);
            return name != null;
        }

        public String GetPartitionContextPropertyName(EventType fromType,
                                                      String propertyName)
        {
            foreach (ContextDetailPartitionItem item in _partitionProperties)
            {
                if (item.FilterSpecCompiled.FilterForEventType == fromType)
                {
                    for (int i = 0; i < item.PropertyNames.Count; i++)
                    {
                        if (item.PropertyNames[i] == propertyName)
                        {
                            return ContextPropertyEventType.PROP_CTX_KEY_PREFIX + (i + 1);
                        }
                    }
                }
            }
            return null;
        }

        public bool IsContextPropertyPrefix(String prefixName)
        {
            return (prefixName != null) && (String.Equals(prefixName, CONTEXT_PREFIX, StringComparison.InvariantCultureIgnoreCase));
        }

        public EventType ContextEventType
        {
            get { return _contextEventType; }
        }

        #endregion
    }
}