///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// List of built-in annotations.
    /// </summary>
    public class BuiltinAnnotation {
    
        /// <summary>
        /// List of built-in annotations.
        /// </summary>
        public readonly static IDictionary<String, Type> BUILTIN = new Dictionary<String, Type>();
    
        static BuiltinAnnotation() {
            foreach (Type clazz in new Type[] {
                    typeof(AuditAttribute),
                    typeof(DescriptionAttribute),
                    typeof(DropAttribute),
                    typeof(DurableAttribute),
                    typeof(EventRepresentationAttribute),
                    typeof(ExternalDWAttribute),
                    typeof(ExternalDWKeyAttribute),
                    typeof(ExternalDWListenerAttribute),
                    typeof(ExternalDWQueryAttribute),
                    typeof(ExternalDWSettingAttribute),
                    typeof(ExternalDWValueAttribute),
                    typeof(HintAttribute),
                    typeof(HookAttribute),
                    typeof(IterableUnboundAttribute),
                    typeof(NameAttribute),
                    typeof(NoLockAttribute),
                    typeof(OverflowAttribute),
                    typeof(PriorityAttribute),
                    typeof(ResilientAttribute),
                    typeof(TagAttribute),
                    typeof(TransientAttribute)
            }) {
                BUILTIN.Put(clazz.Name.ToLower(), clazz);
            }
        }
    }
}
