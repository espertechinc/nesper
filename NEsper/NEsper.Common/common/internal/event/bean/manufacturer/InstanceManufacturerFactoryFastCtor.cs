///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class InstanceManufacturerFactoryFastCtor : InstanceManufacturerFactory
    {
        private readonly ExprForge[] forges;

        public InstanceManufacturerFactoryFastCtor(
            Type targetClass,
            ConstructorInfo ctor,
            ExprForge[] forges)
        {
            TargetClass = targetClass;
            Ctor = ctor;
            this.forges = forges;
        }

        public Type TargetClass { get; }

        public ConstructorInfo Ctor { get; }

        public InstanceManufacturer MakeEvaluator()
        {
            return new InstanceManufacturerFastCtor(this, ExprNodeUtilityQuery.GetEvaluatorsNoCompile(forges));
        }

        public CodegenExpression Codegen(
            object forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return InstanceManufacturerFastCtor.Codegen(
                codegenMethodScope, exprSymbol, codegenClassScope, TargetClass, forges);
        }
    }
} // end of namespace