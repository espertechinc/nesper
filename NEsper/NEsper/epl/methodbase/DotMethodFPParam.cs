///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.methodbase
{
    public class DotMethodFPParam
    {
        public DotMethodFPParam(int lambdaParamNum, String description, DotMethodFPParamTypeEnum type)
        {
            LambdaParamNum = lambdaParamNum;
            Description = description;
            ParamType = type;
            SpecificType = null;
            if (type == DotMethodFPParamTypeEnum.SPECIFIC)
            {
                throw new ArgumentException("Invalid ctor for specific-type parameter");
            }
        }

        public DotMethodFPParam(String description, DotMethodFPParamTypeEnum type, Type specificType)
        {
            Description = description;
            ParamType = type;
            SpecificType = specificType;
            LambdaParamNum = 0;
        }

        public int LambdaParamNum { get; private set; }

        public string Description { get; private set; }

        public DotMethodFPParamTypeEnum ParamType { get; private set; }

        public Type SpecificType { get; private set; }
    }
}
