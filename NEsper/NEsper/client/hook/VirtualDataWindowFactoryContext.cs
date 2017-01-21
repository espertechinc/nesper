///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Context for use with virtual data window factory <seealso cref="VirtualDataWindowFactory" />
    /// provides contextual information about the named window and the type of events held, handle 
    /// for posting insert and remove streams and factory for event bean instances. 
    /// </summary>
    public class VirtualDataWindowFactoryContext
    {
        private readonly EventType _eventType;
        private readonly Object[] _parameters;
        private readonly ExprNode[] _parameterExpressions;
        private readonly EventBeanFactory _eventFactory;
        private readonly String _namedWindowName;
        private readonly ViewFactoryContext _viewFactoryContext;
        private readonly object _customConfiguration;
    
        /// <summary>Ctor. </summary>
        /// <param name="eventType">the event type that the named window is declared to hold.</param>
        /// <param name="parameters">the parameters passed when declaring the named window, for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here.</param>
        /// <param name="eventFactory">factory for converting row objects to EventBean instances</param>
        /// <param name="namedWindowName">the name of the named window</param>
        /// <param name="parameterExpressions">parameter expressions passed to the virtual data window</param>
        /// <param name="viewFactoryContext">context of services</param>
        /// <param name="customConfiguration">additional configuration</param>
        public VirtualDataWindowFactoryContext(EventType eventType, Object[] parameters, ExprNode[] parameterExpressions, EventBeanFactory eventFactory, String namedWindowName, ViewFactoryContext viewFactoryContext, object customConfiguration)
        {
            _eventType = eventType;
            _parameters = parameters;
            _parameterExpressions = parameterExpressions;
            _eventFactory = eventFactory;
            _namedWindowName = namedWindowName;
            _viewFactoryContext = viewFactoryContext;
            _customConfiguration = customConfiguration;
        }

        /// <summary>Returns the event type of the events held in the virtual data window as per declaration of the named window. </summary>
        /// <value>event type</value>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>Returns the parameters passed; for example "create window ABC.my:vdw("10.0.0.1")" passes one paramater here. </summary>
        /// <value>parameters</value>
        public object[] Parameters
        {
            get { return _parameters; }
        }

        /// <summary>Returns the factory for creating instances of EventBean from rows. </summary>
        /// <value>event bean factory</value>
        public EventBeanFactory EventFactory
        {
            get { return _eventFactory; }
        }

        /// <summary>Returns the name of the named window used in connection with the virtual data window. </summary>
        /// <value>named window</value>
        public string NamedWindowName
        {
            get { return _namedWindowName; }
        }

        /// <summary>Returns the expressions passed as parameters to the virtual data window. </summary>
        /// <value>parameter expressions</value>
        public ExprNode[] ParameterExpressions
        {
            get { return _parameterExpressions; }
        }

        /// <summary>Returns the engine services context. </summary>
        /// <value>engine services context</value>
        public ViewFactoryContext ViewFactoryContext
        {
            get { return _viewFactoryContext; }
        }

        /// <summary>Returns any additional configuration provided. </summary>
        /// <value>additional config</value>
        public object CustomConfiguration
        {
            get { return _customConfiguration; }
        }

        /// <summary>Returns the statement contextual information and services. </summary>
        /// <value>statement context</value>
        public StatementContext StatementContext
        {
            get { return _viewFactoryContext.StatementContext; }
        }
    }
}
