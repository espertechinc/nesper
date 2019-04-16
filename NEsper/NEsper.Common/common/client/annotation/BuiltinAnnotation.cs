///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>List of built-in annotations.</summary>
    /// <summary>
    /// List of built-in annotations.
    /// </summary>
    public class BuiltinAnnotation
    {
        /// <summary>
        /// List of built-in annotations.
        /// </summary>
        public static readonly Type[] VALUES = new Type[] {
            typeof(AuditAttribute),
            typeof(DescriptionAttribute),
            typeof(DropAttribute),
            typeof(EventRepresentationAttribute),
            typeof(HintAttribute),
            typeof(HookAttribute),
            typeof(IterableUnboundAttribute),
            typeof(NameAttribute),
            typeof(NoLockAttribute),
            typeof(PriorityAttribute),
            typeof(TagAttribute)
        };

        /// <summary>
        /// Dictionary of built-in annotations.
        /// </summary>
        public static readonly IDictionary<string, Type> BUILTIN = new Dictionary<string, Type>();

        static BuiltinAnnotation()
        {
            foreach (var clazz in VALUES) {
                BUILTIN.Put(clazz.Name.ToLower(), clazz);
            }
        }
    }
} // end of namespace