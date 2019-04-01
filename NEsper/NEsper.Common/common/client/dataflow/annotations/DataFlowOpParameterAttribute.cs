///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.dataflow.annotations
{
    /// <summary>
    ///     Annotation for use with data flow fields on operator forges to receive parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class DataFlowOpParameterAttribute : Attribute
    {
        public DataFlowOpParameterAttribute()
            : this(string.Empty, false)
        {
        }

        public DataFlowOpParameterAttribute(string name)
            : this(name, false)
        {
        }

        public DataFlowOpParameterAttribute(bool all)
            : this(string.Empty, all)
        {
        }

        public DataFlowOpParameterAttribute(string name, bool isAll)
        {
            Name = name;
            IsAll = isAll;
        }

        public string Name { get; set; }
        public bool IsAll { get; set; }
    }
} // end of namespace