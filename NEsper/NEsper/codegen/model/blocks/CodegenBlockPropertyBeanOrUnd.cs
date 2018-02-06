///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.events;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.codegen.model.blocks
{
    /// <summary>
    /// if (!(valueMap is TYPE)) {
    /// if (value is EventBean) {
    /// return Getter.XXX((EventBean) value);
    /// }
    /// return XXXX;
    /// }
    /// return Getter.GetXXXX(value);
    /// </summary>
    public class CodegenBlockPropertyBeanOrUnd
    {
        public static string From(
            ICodegenContext context,
            Type expectedUnderlyingType,
            EventPropertyGetterSPI innerGetter,
            AccessType accessType,
            Type generator)
        {
            var block = context.AddMethod(accessType == AccessType.EXISTS ? typeof(bool) : typeof(object),
                    typeof(object), "value", generator)
                .IfNotInstanceOf("value", expectedUnderlyingType)
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value");

            switch (accessType)
            {
                case AccessType.GET:
                    block = block.BlockReturn(innerGetter.CodegenEventBeanGet(Ref("bean"), context));
                    break;
                case AccessType.EXISTS:
                    block = block.BlockReturn(innerGetter.CodegenEventBeanExists(Ref("bean"), context));
                    break;
                case AccessType.FRAGMENT:
                    block = block.BlockReturn(innerGetter.CodegenEventBeanFragment(Ref("bean"), context));
                    break;
                default:
                    throw new UnsupportedOperationException("Invalid access type " + accessType);
            }

            block = block.BlockReturn(Constant(accessType == AccessType.EXISTS ? (object) false : null));

            ICodegenExpression expression;
            switch (accessType)
            {
                case AccessType.GET:
                    expression = innerGetter.CodegenUnderlyingGet(Cast(
                        expectedUnderlyingType, Ref("value")), context);
                    break;
                case AccessType.EXISTS:
                    expression = innerGetter.CodegenUnderlyingExists(Cast(
                        expectedUnderlyingType, Ref("value")), context);
                    break;
                case AccessType.FRAGMENT:
                    expression = innerGetter.CodegenUnderlyingFragment(
                        Cast(expectedUnderlyingType, Ref("value")), context);
                    break;
                default:
                    throw new UnsupportedOperationException("Invalid access type " + accessType);
            }

            return block.MethodReturn(expression);
        }

        public enum AccessType
        {
            GET,
            EXISTS,
            FRAGMENT
        }
    }
} // end of namespace
