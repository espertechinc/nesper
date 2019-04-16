///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.methodbase
{
    public class DotMethodFPParam
    {
        public DotMethodFPParam(
            int lambdaParamNum,
            string description,
            EPLExpressionParamType type)
        {
            LambdaParamNum = lambdaParamNum;
            Description = description;
            Type = type;
            SpecificType = null;
            if (type == EPLExpressionParamType.SPECIFIC) {
                throw new ArgumentException("Invalid ctor for specific-type parameter");
            }
        }

        public DotMethodFPParam(
            string description,
            EPLExpressionParamType type)
            : this(description, type, null)
        {
        }

        public DotMethodFPParam(
            string description,
            EPLExpressionParamType type,
            params Type[] specificType)
        {
            Description = description;
            Type = type;
            SpecificType = specificType;
            LambdaParamNum = 0;
        }

        public int LambdaParamNum { get; }

        public string Description { get; }

        public EPLExpressionParamType Type { get; }

        public Type[] SpecificType { get; }
    }
} // end of namespace