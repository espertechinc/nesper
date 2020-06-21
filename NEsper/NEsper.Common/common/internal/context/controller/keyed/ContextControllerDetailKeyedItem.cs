///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerDetailKeyedItem
    {
        public EventPropertyValueGetter Getter { get; set; }

        public FilterSpecActivatable FilterSpecActivatable { get; set; }

        public string OptionalInitConditionAsName { get; set; }

        public ExprFilterSpecLookupable[] Lookupables { get; set; }

        public Type[] PropertyTypes { get; set; }

        public string AliasName { get; set; }
        
        private DataInputOutputSerde<object> KeySerde { get; set; }
    }
} // end of namespace