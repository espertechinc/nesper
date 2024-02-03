///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lambdaParamNum">
        ///   number of parameters that are lambda-parameters, i.e. zero for no-lambda, or 1 for "a =&gt; ..." or
        ///    2 for "(a,b) =&gt; ..."</param>
        /// <param name="description">parameter description</param>
        /// <param name="paramType">parameter type</param>
        /// <exception cref="ArgumentException"></exception>
        public DotMethodFPParam(
            int lambdaParamNum,
            string description,
            EPLExpressionParamType paramType)
        {
            LambdaParamNum = lambdaParamNum;
            Description = description;
            ParamType = paramType;
            SpecificType = null;
            if (paramType == EPLExpressionParamType.SPECIFIC) {
                throw new ArgumentException("Invalid ctor for specific-type parameter");
            }
        }

        public DotMethodFPParam(
            string description,
            EPLExpressionParamType paramType)
            : this(description, paramType, null)
        {
        }

        public DotMethodFPParam(
            string description,
            EPLExpressionParamType paramType,
            params Type[] specificType)
        {
            Description = description;
            ParamType = paramType;
            SpecificType = specificType;
            LambdaParamNum = 0;
        }

        public int LambdaParamNum { get; }

        public string Description { get; }

        public EPLExpressionParamType ParamType { get; }

        public Type[] SpecificType { get; }
    }
} // end of namespace