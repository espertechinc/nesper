///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     An stream that polls from a method.
    /// </summary>
    [Serializable]
    public class MethodInvocationStream : Stream
    {
        private string className;
        private string methodName;
        private string optionalEventTypeName;
        private IList<Expression> parameterExpressions;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public MethodInvocationStream()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        public MethodInvocationStream(
            string className,
            string methodName,
            string optStreamName)
            : base(optStreamName)
        {
            this.className = className;
            this.methodName = methodName;
            parameterExpressions = new List<Expression>();
        }

        /// <summary>
        ///     Returns the name of the class providing the method.
        /// </summary>
        /// <returns>class name</returns>
        public string ClassName {
            get => className;
            set => className = value;
        }

        /// <summary>
        ///     Returns the name of the static method to invoke in the from-clause.
        /// </summary>
        /// <returns>method name</returns>
        public string MethodName {
            get => methodName;
            set => methodName = value;
        }

        /// <summary>
        ///     Returns a list of expressions that are parameters to the method.
        /// </summary>
        /// <returns>list of parameter expressions</returns>
        public IList<Expression> ParameterExpressions {
            get => parameterExpressions;
            set => parameterExpressions = value;
        }

        /// <summary>
        ///     Returns the optional event type name
        /// </summary>
        /// <returns>event type name name</returns>
        public string OptionalEventTypeName {
            get => optionalEventTypeName;
            set => optionalEventTypeName = value;
        }

        /// <summary>
        ///     Creates a new method-invocation-based stream without parameters.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <returns>stream</returns>
        public static MethodInvocationStream Create(
            string className,
            string methodName)
        {
            return new MethodInvocationStream(className, methodName, null);
        }

        /// <summary>
        ///     Creates a new method-invocation-based stream without parameters.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        /// <returns>stream</returns>
        public static MethodInvocationStream Create(
            string className,
            string methodName,
            string optStreamName)
        {
            return new MethodInvocationStream(className, methodName, optStreamName);
        }

        /// <summary>
        ///     Adds a parameters to the method invocation.
        /// </summary>
        /// <param name="parameterExpression">is the expression to add</param>
        /// <returns>stream</returns>
        public MethodInvocationStream AddParameter(Expression parameterExpression)
        {
            parameterExpressions.Add(parameterExpression);
            return this;
        }

        public override void ToEPLStream(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("method:");
            writer.Write(className);
            writer.Write(".");
            writer.Write(methodName);
            writer.Write("(");

            var delimiter = "";
            foreach (var expr in parameterExpressions) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            writer.Write(")");

            if (optionalEventTypeName != null) {
                writer.Write(" @type(");
                writer.Write(optionalEventTypeName);
                writer.Write(")");
            }
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            writer.Write("method:");
            writer.Write(className);
            writer.Write(".");
            writer.Write(methodName);
            writer.Write("(..)");
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {
        }
    }
} // end of namespace