///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.util;

namespace com.espertech.esper.epl.methodbase
{
    public class DotMethodFPParam
    {
        public DotMethodFPParam(int lambdaParamNum, String description, EPLExpressionParamType type)
        {
            LambdaParamNum = lambdaParamNum;
            Description = description;
            ParamType = type;
            SpecificType = null;
            if (type == EPLExpressionParamType.SPECIFIC)
            {
                throw new ArgumentException("Invalid ctor for specific-type parameter");
            }
        }

        public DotMethodFPParam(String description, EPLExpressionParamType type, params Type[] specificType)
        {
            Description = description;
            ParamType = type;
            SpecificType = specificType;
            LambdaParamNum = 0;
        }

        public int LambdaParamNum { get; private set; }

        public string Description { get; private set; }

        public EPLExpressionParamType ParamType { get; private set; }

        public Type[] SpecificType { get; private set; }
    }
}
