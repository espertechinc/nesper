///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using Castle.DynamicProxy;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.view
{
    public class ViewFactoryProxy : IInterceptor
    {
        private static readonly MethodInfo MakeViewMethod = typeof(ViewFactory).GetMethod("MakeView");

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly ViewFactory _viewFactory;
        private readonly String _viewName;

        public static ViewFactory NewInstance(String engineURI, String statementName, ViewFactory viewFactory, String viewName)
        {
            var generator = new ProxyGenerator();
            return (ViewFactory) generator.CreateInterfaceProxyWithoutTarget(
                typeof(ViewFactory),
                viewFactory.GetType().GetInterfaces(),
                new ViewFactoryProxy(engineURI, statementName, viewFactory, viewName));
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method != MakeViewMethod)
            {
                invocation.ReturnValue = invocation.Method.Invoke(
                    _viewFactory, invocation.Arguments);
                return;
            }

            var view = (View)invocation.Method.Invoke(_viewFactory, invocation.Arguments);
            invocation.ReturnValue = ViewProxy.NewInstance(_engineURI, _statementName, _viewName, view);
        }

        public ViewFactoryProxy(String engineURI, String statementName, ViewFactory viewFactory, String viewName)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _viewFactory = viewFactory;
            _viewName = viewName;
        }
    }
}
