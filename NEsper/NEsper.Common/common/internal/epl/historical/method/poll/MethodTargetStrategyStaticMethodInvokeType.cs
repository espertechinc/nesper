///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public enum MethodTargetStrategyStaticMethodInvokeType
    {
        NOPARAM,
        SINGLE,
        MULTIKEY
    }

    public static class MethodTargetStrategyStaticMethodInvokeTypeExtensions
    {
        public static MethodTargetStrategyStaticMethodInvokeType GetInvokeType(MethodInfo method)
        {
            var parameterTypes = method.GetParameterTypes();
            if (parameterTypes.Length == 0) {
                return MethodTargetStrategyStaticMethodInvokeType.NOPARAM;
            }
            else if (parameterTypes.Length == 1) {
                return MethodTargetStrategyStaticMethodInvokeType.SINGLE;
            }
            else {
                return MethodTargetStrategyStaticMethodInvokeType.MULTIKEY;
            }
        }
    }
} // end of namespace