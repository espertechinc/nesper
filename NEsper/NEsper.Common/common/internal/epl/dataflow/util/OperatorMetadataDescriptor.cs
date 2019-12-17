///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.annotation.AnnotationUtil;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class OperatorMetadataDescriptor
    {
        public OperatorMetadataDescriptor()
        {
        }

        public OperatorMetadataDescriptor(
            Type forgeClass,
            string operatorPrettyPrint,
            Attribute[] operatorAnnotations,
            int numOutputPorts,
            string operatorName)
        {
            ForgeClass = forgeClass;
            OperatorPrettyPrint = operatorPrettyPrint;
            OperatorAnnotations = operatorAnnotations;
            NumOutputPorts = numOutputPorts;
            OperatorName = operatorName;
        }

        public Type ForgeClass { get; set; }

        public string OperatorPrettyPrint { get; set; }

        public Attribute[] OperatorAnnotations { get; set; }

        public int NumOutputPorts { get; set; }

        public string OperatorName { get; set; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(OperatorMetadataDescriptor), GetType(), classScope);
            method.Block
                .DeclareVar<OperatorMetadataDescriptor>("op", NewInstance(typeof(OperatorMetadataDescriptor)))
                .SetProperty(Ref("op"), "ForgeClass", Constant(ForgeClass))
                .SetProperty(Ref("op"), "OperatorPrettyPrint", Constant(OperatorPrettyPrint))
                .SetProperty(
                    Ref("op"),
                    "OperatorAnnotations",
                    OperatorAnnotations == null
                        ? ConstantNull()
                        : LocalMethod(MakeAnnotations(typeof(Attribute[]), OperatorAnnotations, method, classScope)))
                .SetProperty(Ref("op"), "NumOutputPorts", Constant(NumOutputPorts))
                .SetProperty(Ref("op"), "OperatorName", Constant(OperatorName))
                .MethodReturn(Ref("op"));
            return LocalMethod(method);
        }
    }
} // end of namespace