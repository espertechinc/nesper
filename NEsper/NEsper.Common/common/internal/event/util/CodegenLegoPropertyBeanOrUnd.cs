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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.util
{
    // if (!(valueMap instanceof TYPE)) {
    //   if (value instanceof EventBean) {
    //     return getter.XXX((EventBean) value);
    //   }
    //   return XXXX;
    // }
    // return getter.getXXXX(value);
    public class CodegenLegoPropertyBeanOrUnd
    {
        public enum AccessType
        {
            GET,
            EXISTS,
            FRAGMENT
        }

        public static CodegenMethod From(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope,
            Type expectedUnderlyingType,
            EventPropertyGetterSPI innerGetter,
            AccessType accessType,
            Type generator)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    accessType == AccessType.EXISTS ? typeof(bool) : typeof(object), generator, codegenClassScope)
                .AddParam(typeof(object), "value");
            var block = methodNode.Block
                .IfNotInstanceOf("value", expectedUnderlyingType)
                .IfInstanceOf("value", typeof(EventBean))
                .DeclareVarWCast(typeof(EventBean), "bean", "value");

            if (accessType == AccessType.GET) {
                block = block.BlockReturn(
                    innerGetter.EventBeanGetCodegen(Ref("bean"), codegenMethodScope, codegenClassScope));
            }
            else if (accessType == AccessType.EXISTS) {
                block = block.BlockReturn(
                    innerGetter.EventBeanExistsCodegen(Ref("bean"), codegenMethodScope, codegenClassScope));
            }
            else if (accessType == AccessType.FRAGMENT) {
                block = block.BlockReturn(
                    innerGetter.EventBeanFragmentCodegen(Ref("bean"), codegenMethodScope, codegenClassScope));
            }
            else {
                throw new UnsupportedOperationException("Invalid access type " + accessType);
            }

            block = block.BlockReturn(Constant(accessType == AccessType.EXISTS ? (bool?) false : null));

            CodegenExpression expression;
            if (accessType == AccessType.GET) {
                expression = innerGetter.UnderlyingGetCodegen(
                    Cast(expectedUnderlyingType, Ref("value")), codegenMethodScope, codegenClassScope);
            }
            else if (accessType == AccessType.EXISTS) {
                expression = innerGetter.UnderlyingExistsCodegen(
                    Cast(expectedUnderlyingType, Ref("value")), codegenMethodScope, codegenClassScope);
            }
            else if (accessType == AccessType.FRAGMENT) {
                expression = innerGetter.UnderlyingFragmentCodegen(
                    Cast(expectedUnderlyingType, Ref("value")), codegenMethodScope, codegenClassScope);
            }
            else {
                throw new UnsupportedOperationException("Invalid access type " + accessType);
            }

            block.MethodReturn(expression);
            return methodNode;
        }
    }
}