///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.xml
{
    public class XPathPropertyGetterCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly BaseXMLEventType baseXMLEventType;
        private readonly XPathPropertyGetter xPathPropertyGetter;

        public XPathPropertyGetterCodegenFieldSharable(
            BaseXMLEventType baseXMLEventType,
            XPathPropertyGetter xPathPropertyGetter)
        {
            this.baseXMLEventType = baseXMLEventType;
            this.xPathPropertyGetter = xPathPropertyGetter;
        }

        public Type Type()
        {
            return typeof(XPathPropertyGetter);
        }

        public CodegenExpression InitCtorScoped()
        {
            return StaticMethod(
                typeof(XPathPropertyGetterCodegenFieldSharable),
                "resolveXPathPropertyGetter",
                EventTypeUtility.ResolveTypeCodegen(baseXMLEventType, EPStatementInitServicesConstants.REF),
                Constant(xPathPropertyGetter.Property));
        }

        public static XPathPropertyGetter ResolveXPathPropertyGetter(
            EventType eventType,
            string propertyName)
        {
            if (!(eventType is BaseXMLEventType)) {
                throw new EPException(
                    "Failed to obtain xpath property getter, expected an xml event type but received type '" +
                    eventType.Metadata.Name +
                    "'");
            }

            var type = (BaseXMLEventType) eventType;
            var getter = type.GetGetter(propertyName);
            if (!(getter is XPathPropertyGetter)) {
                throw new EPException(
                    "Failed to obtain xpath property getter for property '" +
                    propertyName +
                    "', expected " +
                    typeof(XPathPropertyGetter).Name +
                    " but received " +
                    getter);
            }

            return (XPathPropertyGetter) getter;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (XPathPropertyGetterCodegenFieldSharable) o;

            if (!baseXMLEventType.Equals(that.baseXMLEventType)) {
                return false;
            }

            return xPathPropertyGetter.Equals(that.xPathPropertyGetter);
        }

        public override int GetHashCode()
        {
            var result = baseXMLEventType.GetHashCode();
            result = 31 * result + xPathPropertyGetter.GetHashCode();
            return result;
        }
    }
} // end of namespace