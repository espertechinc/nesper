///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.compiletime
{
    public class StmtClassForgeableJsonUtil
    {
        public static void MakeNoSuchElementDefault(
            CodegenStatementSwitch switchStmt,
            CodegenExpressionRef name)
        {
            switchStmt.DefaultBlock
                .BlockThrow(NewInstance(typeof(NoSuchElementException), Concat(Constant("Field named "), name)));
        }

        public static CodegenExpression[] GetCasesNumberNtoM(StmtClassForgeableJsonDesc desc)
        {
            var cases = new CodegenExpression[desc.PropertiesThisType.Count];
            var index = 0;
            foreach (var property in desc.PropertiesThisType) {
                var field = desc.FieldDescriptorsInclSupertype.Get(property.Key);
                cases[index] = Constant(field.FieldName);
                index++;
            }

            return cases;
        }
    }
} // end of namespace