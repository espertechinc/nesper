///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>An stream that polls from a method.</summary>
    public class MethodInvocationStream : Stream{
        /// <summary>Ctor.</summary>
        public MethodInvocationStream()
        {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        public MethodInvocationStream(string className, string methodName, string optStreamName)
            : base(optStreamName)
        {
            ClassName = className;
            MethodName = methodName;
            ParameterExpressions = new List<Expression>();
        }
    
        /// <summary>
        /// Creates a new method-invocation-based stream without parameters.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <returns>stream</returns>
        public static MethodInvocationStream Create(string className, string methodName) {
            return new MethodInvocationStream(className, methodName, null);
        }
    
        /// <summary>
        /// Creates a new method-invocation-based stream without parameters.
        /// </summary>
        /// <param name="className">is the name of the class providing the method</param>
        /// <param name="methodName">is the name of the public static method</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        /// <returns>stream</returns>
        public static MethodInvocationStream Create(string className, string methodName, string optStreamName) {
            return new MethodInvocationStream(className, methodName, optStreamName);
        }

        /// <summary>
        /// Returns the name of the class providing the method.
        /// </summary>
        /// <value>class name</value>
        public string ClassName { get; set; }

        /// <summary>
        /// Returns the name of the static method to invoke in the from-clause.
        /// </summary>
        /// <value>method name</value>
        public string MethodName { get; set; }

        /// <summary>
        /// Returns a list of expressions that are parameters to the method.
        /// </summary>
        /// <value>list of parameter expressions</value>
        public IList<Expression> ParameterExpressions { get; set; }

        /// <summary>
        /// Returns the optional event type name
        /// </summary>
        /// <value>event type name name</value>
        public string OptionalEventTypeName { get; set; }

        /// <summary>
        /// Adds a parameters to the method invocation.
        /// </summary>
        /// <param name="parameterExpression">is the expression to add</param>
        /// <returns>stream</returns>
        public MethodInvocationStream AddParameter(Expression parameterExpression) {
            ParameterExpressions.Add(parameterExpression);
            return this;
        }
    
        public override void ToEPLStream(TextWriter writer, EPStatementFormatter formatter) {
            writer.Write("method:");
            writer.Write(ClassName);
            writer.Write(".");
            writer.Write(MethodName);
            writer.Write("(");
    
            string delimiter = "";
            foreach (Expression expr in ParameterExpressions) {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
            writer.Write(")");
    
            if (OptionalEventTypeName != null) {
                writer.Write(" @Type(");
                writer.Write(OptionalEventTypeName);
                writer.Write(")");
            }
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            writer.Write("method:");
            writer.Write(ClassName);
            writer.Write(".");
            writer.Write(MethodName);
            writer.Write("(..)");
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {
        }
    }
} // end of namespace
