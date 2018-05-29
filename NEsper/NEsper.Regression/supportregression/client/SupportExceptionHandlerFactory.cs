///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client.hook;


namespace com.espertech.esper.supportregression.client
{
    public class SupportExceptionHandlerFactory : ExceptionHandlerFactory
    {
        private static readonly List<ExceptionHandlerFactoryContext> factoryContexts = 
            new List<ExceptionHandlerFactoryContext>();
        private static readonly List<SupportExceptionHandler> handlers = 
            new List<SupportExceptionHandler>();
    
        public ExceptionHandler GetHandler(ExceptionHandlerFactoryContext context)
        {
            factoryContexts.Add(context);

            SupportExceptionHandler handler = new SupportExceptionHandler();
            handlers.Add(handler);
            return handler.Handle;
        }

        public static IList<ExceptionHandlerFactoryContext> FactoryContexts => factoryContexts;

        public static IList<SupportExceptionHandler> Handlers => handlers;

        public class SupportExceptionHandler
        {
            private readonly List<ExceptionHandlerContext> _contexts = 
                new List<ExceptionHandlerContext>();
            private readonly List<ExceptionHandlerContextUnassociated> _inboundPoolContexts =
                new List<ExceptionHandlerContextUnassociated>();

            public void Handle(object sender, ExceptionHandlerEventArgs eventArgs)
            {
                if (eventArgs.IsInboundPoolException) {
                    _inboundPoolContexts.Add(eventArgs.InboundPoolContext);
                }
                else {
                    _contexts.Add(eventArgs.Context);
                }
            }

            public IList<ExceptionHandlerContext> Contexts => _contexts;

            public IList<ExceptionHandlerContextUnassociated> InboundPoolContexts => _inboundPoolContexts;
        }
    }
}
