///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using Castle.DynamicProxy;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.annotation
{
    /// <summary>
    ///     Invocation handler for EPL and application-specified annotations.
    /// </summary>
    public class EPLAnnotationInvocationHandler : IInterceptor
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        private int? hashCode;
        private string toStringResult;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="annotationClass">annotation class</param>
        /// <param name="attributes">attribute values</param>
        public EPLAnnotationInvocationHandler(
            Type annotationClass,
            IDictionary<string, object> attributes)
        {
            AnnotationClass = annotationClass;
            Attributes = attributes;
        }

        public Type AnnotationClass { get; }

        public IDictionary<string, object> Attributes { get; }

        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (invocation.Method.IsSpecialName) {
                if (methodName.StartsWith("get_")) {
                    var propertyName = methodName.Substring(4);
                    if (Attributes.TryGetValue(propertyName, out var propertyValue)) {
                        invocation.ReturnValue = propertyValue;
                    }
                }
            }
            else {
                if (methodName == "Equals") {
                    if (invocation.Arguments.Length != 0) {
                        invocation.ReturnValue = HandleEquals(invocation.Arguments[0]);
                    }
                }
                else if (methodName == "GetHashCode") {
                    invocation.ReturnValue = HandleHashCode();
                }
                else if (methodName == "ToString") {
                    invocation.ReturnValue = HandleToString();
                }
                else if (methodName == "AnnotationType") {
                    invocation.ReturnValue = AnnotationClass;
                }
            }
        }

        public Attribute CreateProxyInstance()
        {
            var interfaces = AnnotationClass
                .GetInterfaces()
                .ToArray();

            return (Attribute) Generator.CreateClassProxy(
                AnnotationClass, interfaces, ProxyGenerationOptions.Default, this);
        }

        private string HandleToString()
        {
            if (toStringResult == null) {
                var buf = new StringBuilder();
                buf.Append("@");
                buf.Append(AnnotationClass.GetSimpleName());
                if (!Attributes.IsEmpty()) {
                    var delimiter = "";
                    buf.Append("(");

                    if (Attributes.Count == 1 && Attributes.ContainsKey("Value")) {
                        if (Attributes.Get("Value") is string) {
                            buf.Append("\"");
                            buf.Append(Attributes.Get("Value"));
                            buf.Append("\"");
                        }
                        else {
                            buf.Append(Attributes.Get("Value"));
                        }
                    }
                    else {
                        foreach (var attribute in Attributes) {
                            buf.Append(delimiter);
                            buf.Append(attribute.Key);
                            buf.Append("=");
                            if (attribute.Value is string) {
                                buf.Append("\"");
                                buf.Append(attribute.Value);
                                buf.Append("\"");
                            }
                            else {
                                buf.Append(attribute.Value);
                            }

                            delimiter = ", ";
                        }
                    }

                    buf.Append(")");
                }

                toStringResult = buf.ToString();
            }

            return toStringResult;
        }

        private object HandleEquals(object arg)
        {
            if (this == arg) {
                return true;
            }

            if (!(arg is Attribute)) {
                return false;
            }

            var other = (Attribute) arg;
            if (other.GetType() != AnnotationClass) {
                return false;
            }

            // Apparently we intend to grab the other object, verify that it has an interceptor
            // and if it does to then compare the attributes exposed by the interceptor??? This
            // feels like an area we want to understand better before we attempt to model it
            // here.  Might be better served by DynamicProxy or Expando.

            if (!ProxyUtil.IsProxy(other)) {
                return false;
            }

#if false
// Don't know if Castle will create a real proxy or a fake proxy - TBD
            if (!(other is EPLAnnotationInvocationHandler handler)) {
                return false;
            }

            var that = (EPLAnnotationInvocationHandler) handler;
            if (annotationClass != null
                ? !annotationClass.Equals(that.annotationClass)
                : that.annotationClass != null) {
                return false;
            }

            foreach (KeyValuePair<string, object> entry in attributes) {
                if (!that.attributes.ContainsKey(entry.Key)) {
                    return false;
                }

                object thisValue = entry.Value;
                object thatValue = that.attributes.Get(entry.Key);
                if (thisValue != null ? !thisValue.Equals(thatValue) : thatValue != null) {
                    return false;
                }
            }
#endif

            return true;
        }

        private object HandleHashCode()
        {
            if (hashCode != null) {
                return hashCode;
            }

            var result = AnnotationClass.GetHashCode();
            foreach (var key in Attributes.Keys) {
                result = 31 * result + key.GetHashCode();
            }

            hashCode = result;
            return hashCode;
        }
    }
} // end of namespace