///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public interface EnumForgeDescFactory
    {
        EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum);

        EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices services);
    }

    public class ProxyEnumForgeDescFactory : EnumForgeDescFactory
    {
        public Func<int, EnumForgeLambdaDesc> ProcGetLambdaStreamTypesForParameter { get; set; }

        public EnumForgeLambdaDesc GetLambdaStreamTypesForParameter(int parameterNum) =>
            ProcGetLambdaStreamTypesForParameter.Invoke(parameterNum);

        public Func<IList<ExprDotEvalParam>, int, StatementCompileTimeServices, EnumForgeDesc> ProcMakeEnumForgeDesc { get; set; }

        public EnumForgeDesc MakeEnumForgeDesc(
            IList<ExprDotEvalParam> bodiesAndParameters,
            int streamCountIncoming,
            StatementCompileTimeServices services) =>
            ProcMakeEnumForgeDesc.Invoke(bodiesAndParameters, streamCountIncoming, services);

        public ProxyEnumForgeDescFactory()
        {
        }

        public ProxyEnumForgeDescFactory(Func<int, EnumForgeLambdaDesc> procGetLambdaStreamTypesForParameter,
            Func<IList<ExprDotEvalParam>, int, StatementCompileTimeServices, EnumForgeDesc> procMakeEnumForgeDesc)
        {
            ProcGetLambdaStreamTypesForParameter = procGetLambdaStreamTypesForParameter;
            ProcMakeEnumForgeDesc = procMakeEnumForgeDesc;
        }
    }
} // end of namespace