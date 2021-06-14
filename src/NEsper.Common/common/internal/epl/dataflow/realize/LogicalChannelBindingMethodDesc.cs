///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class LogicalChannelBindingMethodDesc
    {
        public LogicalChannelBindingMethodDesc(
            MethodInfo method,
            LogicalChannelBindingType bindingType)
        {
            Method = method;
            BindingType = bindingType;
        }

        public MethodInfo Method { get; private set; }

        public LogicalChannelBindingType BindingType { get; private set; }
    }
}