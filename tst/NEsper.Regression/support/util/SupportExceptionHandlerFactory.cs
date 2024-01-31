///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.exception;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportExceptionHandlerFactory : ExceptionHandlerFactory
    {
        public static IList<ExceptionHandlerFactoryContext> FactoryContexts { get; } =
            new List<ExceptionHandlerFactoryContext>();

        public static IList<SupportExceptionHandler> Handlers { get; } = new List<SupportExceptionHandler>();

        public ExceptionHandler GetHandler(ExceptionHandlerFactoryContext context)
        {
            FactoryContexts.Add(context);
            var handler = new SupportExceptionHandler();
            Handlers.Add(handler);
            return handler.Handle;
        }

        public class SupportExceptionHandler // ExceptionHandlerInboundPool
        {
            public IList<ExceptionHandlerContext> Contexts { get; } = new List<ExceptionHandlerContext>();

            public IList<ExceptionHandlerContextUnassociated> InboundPoolContexts { get; } =
                new List<ExceptionHandlerContextUnassociated>();

            public void Handle(
                object sender,
                ExceptionHandlerEventArgs args)
            {
                if (args.IsInboundPoolException) {
                    InboundPoolContexts.Add(args.InboundPoolContext);
                }
                else {
                    Contexts.Add(args.Context);
                }
            }
        }
    }
} // end of namespace