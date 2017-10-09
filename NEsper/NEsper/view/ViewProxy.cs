///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Castle.DynamicProxy;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    public class ViewProxy : IInterceptor
    {
        private static readonly MethodInfo UpdateMethod = typeof(View).GetMethod("Update");

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly String _viewName;
        private readonly View _view;

        public static Object NewInstance(String engineURI, String statementName, String viewName, View view)
        {
            var generator = new ProxyGenerator();
            return generator.CreateInterfaceProxyWithoutTarget(
                typeof(View),
                view.GetType().GetInterfaces(),
                new ViewProxy(engineURI, statementName, viewName, view));
        }

        public ViewProxy(String engineURI, String statementName, String viewName, View view) 
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _viewName = viewName;
            _view = view;
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = invocation.Method.Invoke(_view, invocation.Arguments);

            if (invocation.Method == UpdateMethod)
            {
                if (AuditPath.IsAuditEnabled)
                {
                    var newData = (EventBean[])invocation.Arguments[0];
                    var oldData = (EventBean[])invocation.Arguments[1];
                    AuditPath.AuditLog(
                        _engineURI, _statementName, AuditEnum.VIEW,
                        _viewName + " insert {" + EventBeanUtility.Summarize(newData) + "} remove {" + EventBeanUtility.Summarize(oldData) + "}");
                }
            }
        }
    }
}
