///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeDateDesc
    {
        public ExprCastNodeDateDesc(
            bool iso8601Format,
            ExprForge dynamicDateFormat,
            string staticDateFormat,
            bool deployTimeConstant)
        {
            IsIso8601Format = iso8601Format;
            DynamicDateFormat = dynamicDateFormat;
            StaticDateFormat = staticDateFormat;
            IsDeployTimeConstant = deployTimeConstant;
        }

        public bool IsIso8601Format { get; }

        public ExprForge DynamicDateFormat { get; }

        public string StaticDateFormat { get; }

        public bool IsDeployTimeConstant { get; }
    }
} // end of namespace