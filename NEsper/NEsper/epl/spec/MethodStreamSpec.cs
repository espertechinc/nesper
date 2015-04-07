///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification object for historical data poll via database SQL statement.
    /// </summary>
    [Serializable]
    public class MethodStreamSpec 
        : StreamSpecBase
        , StreamSpecRaw
        , StreamSpecCompiled
        , MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="optionalStreamName">is the stream name or null if none defined</param>
        /// <param name="viewSpecs">is an list of view specifications</param>
        /// <param name="ident">the prefix in the clause</param>
        /// <param name="className">the class name</param>
        /// <param name="methodName">the method name</param>
        /// <param name="expressions">the parameter expressions</param>
        public MethodStreamSpec(String optionalStreamName, ViewSpec[] viewSpecs, String ident, String className, String methodName, IList<ExprNode> expressions)
            : base(optionalStreamName, viewSpecs, new StreamSpecOptions())
        {
            Ident = ident;
            ClassName = className;
            MethodName = methodName;
            Expressions = expressions;
        }

        /// <summary>Returns the prefix (method) for the method invocation syntax. </summary>
        /// <value>identifier</value>
        public string Ident { get; private set; }

        /// <summary>Returns the class name. </summary>
        /// <value>class name</value>
        public string ClassName { get; private set; }

        /// <summary>Returns the method name. </summary>
        /// <value>method name</value>
        public string MethodName { get; private set; }

        /// <summary>Returns the parameter expressions. </summary>
        /// <value>parameter expressions</value>
        public IList<ExprNode> Expressions { get; private set; }

        public StreamSpecCompiled Compile(
            StatementContext context,
            ICollection<String> eventTypeReferences,
            bool isInsertInto,
            ICollection<int> assignedTypeNumberStack,
            bool isJoin,
            bool isContextDeclaration,
            bool isOnTrigger)
        {
            if (!Ident.Equals("method"))
            {
                throw new ExprValidationException("Expecting keyword 'method', found '" + Ident + "'");
            }
            if (MethodName == null)
            {
                throw new ExprValidationException("No method name specified for method-based join");
            }
            return this;
        }
    }
}
