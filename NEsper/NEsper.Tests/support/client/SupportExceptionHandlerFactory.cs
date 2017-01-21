///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

using com.espertech.esper.client.hook;


namespace com.espertech.esper.support.client
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

        public static IList<ExceptionHandlerFactoryContext> FactoryContexts
        {
            get { return factoryContexts; }
        }

        public static IList<SupportExceptionHandler> Handlers
        {
            get { return handlers; }
        }

        public class SupportExceptionHandler
        {
            private readonly List<ExceptionHandlerContext> _contexts = 
                new List<ExceptionHandlerContext>();

            public void Handle(ExceptionHandlerContext context) {
                _contexts.Add(context);
            }

            public IList<ExceptionHandlerContext> Contexts
            {
                get { return _contexts; }
            }
        }
    }
}
