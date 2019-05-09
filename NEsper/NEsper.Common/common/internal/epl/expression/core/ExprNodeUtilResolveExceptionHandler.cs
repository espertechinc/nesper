///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public interface ExprNodeUtilResolveExceptionHandler
    {
        ExprValidationException Handle(Exception e);
    }

    public class ProxyExprNodeUtilResolveExceptionHandler : ExprNodeUtilResolveExceptionHandler
    {
        public Func<Exception, ExprValidationException> ProcHandle { get; set; }

        public ProxyExprNodeUtilResolveExceptionHandler()
        {
        }

        public ProxyExprNodeUtilResolveExceptionHandler(Func<Exception, ExprValidationException> procHandle)
        {
            ProcHandle = procHandle;
        }

        public ExprValidationException Handle(Exception e)
        {
            return ProcHandle.Invoke(e);
        }
    }
}