///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvided
    {
        public ClassProvided()
        {
        }

        public ClassProvided(
            IRuntimeArtifact artifact,
            string className)
        {
            Artifact = artifact;
            ClassName = className;
        }

        public IRuntimeArtifact Artifact { get; set; }

        public string ModuleName { get; set; }

        public NameAccessModifier Visibility { get; set; } = NameAccessModifier.TRANSIENT;

        public IList<Type> ClassesMayNull { get; private set; }

        public string ClassName { get; set; }

        public void LoadClasses(TypeResolver parentTypeResolver)
        {
            ClassesMayNull = new List<Type>();
            foreach (var clazz in Artifact.Assembly.ExportedTypes) {
                ClassesMayNull.Add(clazz);
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            ModuleClassProvidedInitializeSymbol symbols)
        {
            var method = parent
                .MakeChild(typeof(ClassProvided), GetType(), classScope);
            if (Artifact == null) {
                method.Block.DeclareVar<IRuntimeArtifact>(
                    "artifact",
                    ConstantNull());
            }
            else {
                method.Block.DeclareVar<IRuntimeArtifact>(
                    "artifact",
                    ExprDotMethod(
                        symbols.GetAddInitSvc(method),
                        "ResolveArtifact",
                        Constant(Artifact.Id)));
            }

            method.Block
                .DeclareVar<ClassProvided>("cp", NewInstance(typeof(ClassProvided)))
                .SetProperty(Ref("cp"), "Artifact", Ref("artifact"))
                .SetProperty(Ref("cp"), "ClassName", Constant(ClassName))
                .SetProperty(Ref("cp"), "ModuleName", Constant(ModuleName))
                .SetProperty(Ref("cp"), "Visibility", Constant(Visibility))
                .MethodReturn(Ref("cp"));
            return LocalMethod(method);
        }
    }
} // end of namespace