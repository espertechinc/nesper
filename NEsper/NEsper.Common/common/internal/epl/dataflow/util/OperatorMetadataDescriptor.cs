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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.annotation.AnnotationUtil;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class OperatorMetadataDescriptor
    {
        private Type forgeClass;
        private string operatorPrettyPrint;
        private Attribute[] operatorAnnotations;
        private int numOutputPorts;
        private string operatorName;

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
            this.forgeClass = forgeClass;
            this.operatorPrettyPrint = operatorPrettyPrint;
            this.operatorAnnotations = operatorAnnotations;
            this.numOutputPorts = numOutputPorts;
            this.operatorName = operatorName;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(OperatorMetadataDescriptor), this.GetType(), classScope);
            method.Block
                .DeclareVar<OperatorMetadataDescriptor>("op", NewInstance(typeof(OperatorMetadataDescriptor)))
                .SetProperty(Ref("op"), "ForgeClass", Constant(forgeClass))
                .SetProperty(Ref("op"), "OperatorPrettyPrint", Constant(operatorPrettyPrint))
                .SetProperty(
                    Ref("op"),
                    "OperatorAnnotations",
                    operatorAnnotations == null
                        ? ConstantNull()
                        : LocalMethod(MakeAnnotations(typeof(Attribute[]), operatorAnnotations, method, classScope)))
                .SetProperty(Ref("op"), "NumOutputPorts", Constant(numOutputPorts))
                .SetProperty(Ref("op"), "OperatorName", Constant(operatorName))
                .MethodReturn(@Ref("op"));
            return LocalMethod(method);
        }

        public Type ForgeClass {
            get => forgeClass;
        }

        public string OperatorPrettyPrint {
            get => operatorPrettyPrint;
        }

        public Attribute[] OperatorAnnotations {
            get => operatorAnnotations;
        }

        public void SetForgeClass(Type forgeClass)
        {
            this.forgeClass = forgeClass;
        }

        public void SetOperatorPrettyPrint(string operatorPrettyPrint)
        {
            this.operatorPrettyPrint = operatorPrettyPrint;
        }

        public void SetOperatorAnnotations(Attribute[] operatorAnnotations)
        {
            this.operatorAnnotations = operatorAnnotations;
        }

        public int NumOutputPorts {
            get => numOutputPorts;
        }

        public void SetNumOutputPorts(int numOutputPorts)
        {
            this.numOutputPorts = numOutputPorts;
        }

        public string OperatorName {
            get => operatorName;
        }

        public void SetOperatorName(string operatorName)
        {
            this.operatorName = operatorName;
        }
    }
} // end of namespace