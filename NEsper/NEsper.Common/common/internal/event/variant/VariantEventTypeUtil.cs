///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.variant
{
    public class VariantEventTypeUtil
    {
        public static CodegenExpressionField GetField(
            VariantEventType variantEventType,
            CodegenClassScope codegenClassScope)
        {
            return codegenClassScope.AddFieldUnshared(
                true, typeof(VariantEventType),
                Cast(typeof(VariantEventType), EventTypeUtility.ResolveTypeCodegen(variantEventType, EPStatementInitServicesConstants.REF)));
        }

        public static void ValidateInsertedIntoEventType(
            EventType eventType,
            VariantEventType variantEventType)
        {
            if (variantEventType.IsVariantAny) {
                return;
            }

            if (eventType == null) {
                throw new ExprValidationException(GetMessage(variantEventType.Name));
            }

            // try each permitted type
            var variants = variantEventType.Variants;
            foreach (var variant in variants) {
                if (variant == eventType) {
                    return;
                }
            }

            // test if any of the supertypes of the eventtype is a variant type
            foreach (var variant in variants) {
                // Check all the supertypes to see if one of the matches the full or delta types
                IEnumerator<EventType> deepSupers = eventType.DeepSuperTypes;
                if (deepSupers == null) {
                    continue;
                }

                EventType superType;
                for (; deepSupers.HasNext;) {
                    superType = deepSupers.Next();
                    if (superType == variant) {
                        return;
                    }
                }
            }

            throw new ExprValidationException(GetMessage(variantEventType.Name));
        }

        private static string GetMessage(string name)
        {
            return "Selected event type is not a valid event type of the variant stream '" + name + "'";
        }
    }
} // end of namespace