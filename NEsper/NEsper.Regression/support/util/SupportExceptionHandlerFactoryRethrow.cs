///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.exception;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportExceptionHandlerFactoryRethrow : ExceptionHandlerFactory
    {
        public ExceptionHandler GetHandler(ExceptionHandlerFactoryContext context)
        {
            return new SupportExceptionHandlerRethrow().Handle;
        }

        public class SupportExceptionHandlerRethrow
        {
            public void Handle(
                object sender,
                ExceptionHandlerEventArgs args)
            {
                var context = args.Context;
                throw new EPException(
                    "Unexpected exception in statement '" +
                    context.StatementName +
                    "': " +
                    context.Exception.Message,
                    context.Exception);
            }
        }
    }
} // end of namespace