///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    public class ExpressionDeclItem
    {
        public ExpressionDeclItem(string name, string[] parametersNames, bool alias)
        {
            Name = name;
            ParametersNames = parametersNames;
            IsAlias = alias;
        }

        public string Name { get; }

        public string[] ParametersNames { get; }

        public bool IsAlias { get; }

        public Expression OptionalSoda { get; set; }

        public Supplier<byte[]> OptionalSodaBytes { get; set; }

        public string ModuleName { get; set; }

        public NameAccessModifier Visibility { get; set; } = NameAccessModifier.TRANSIENT;

        public CodegenExpression Make(
            CodegenMethod parent, ModuleExpressionDeclaredInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ExpressionDeclItem), GetType(), classScope);

            var supplierSodaBytes = NewAnonymousClass(method.Block, typeof(Supplier<byte[]>));
            var get = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope);
            supplierSodaBytes.AddMethod("get", get);
            get.Block.MethodReturn(Constant(OptionalSodaBytes.Invoke()));

            method.Block
                .DeclareVar(
                    typeof(ExpressionDeclItem), "item",
                    NewInstance(
                        typeof(ExpressionDeclItem), Constant(Name), Constant(ParametersNames), Constant(IsAlias)))
                .ExprDotMethod(Ref("item"), "setOptionalSodaBytes", supplierSodaBytes)
                .ExprDotMethod(Ref("item"), "setModuleName", Constant(ModuleName))
                .ExprDotMethod(Ref("item"), "setVisibility", Constant(Visibility))
                .MethodReturn(Ref("item"));
            return LocalMethod(method);
        }
    }
} // end of namespace