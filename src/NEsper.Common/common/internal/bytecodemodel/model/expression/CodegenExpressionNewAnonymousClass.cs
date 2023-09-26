///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    [Obsolete]
    public class CodegenExpressionNewAnonymousClass : CodegenStatementWBlockBase,
        CodegenExpression
    {
        private readonly IList<CodegenExpression> ctorParams;
        private readonly Type interfaceOrSuperClass;
        private readonly IList<Pair<string, CodegenMethod>> methods = new List<Pair<string, CodegenMethod>>();

        public CodegenExpressionNewAnonymousClass(
            CodegenBlock parentBlock,
            Type interfaceOrSuperClass,
            IList<CodegenExpression> ctorParams)
            : base(
                parentBlock)
        {
            this.interfaceOrSuperClass = interfaceOrSuperClass;
            this.ctorParams = ctorParams;
        }

#if false
        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            Render(builder, isInnerClass, 4, new CodegenIndent(true));
        }
#endif

        public override void MergeClasses(ISet<Type> classes)
        {
            classes.AddToSet(interfaceOrSuperClass);
            foreach (var expr in ctorParams) {
                expr.MergeClasses(classes);
            }

            foreach (var additional in methods) {
                additional.Second.MergeClasses(classes);
            }
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            builder.Append("new ");
            AppendClassName(builder, interfaceOrSuperClass);
            builder.Append("(");
            RenderExpressions(builder, ctorParams.ToArray(), isInnerClass);
            builder.Append(") {\n");

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            foreach (var pair in this.methods) {
                CodegenStackGenerator.RecursiveBuildStack(
                    pair.Second,
                    pair.First,
                    methods,
                    properties);
            }

            // public properties
            var delimiter = "";
            foreach (var publicMethod in properties.PublicProperties) {
                builder.Append(delimiter);
                publicMethod.Render(builder, true, isInnerClass, indent, level + 1);
                delimiter = "\n";
            }

            // private properties
            delimiter = "";
            foreach (var method in properties.PrivateProperties) {
                builder.Append(delimiter);
                method.Render(builder, false, isInnerClass, indent, level + 1);
                delimiter = "\n";
            }

            // public methods
            delimiter = "";
            foreach (var publicMethod in methods.PublicMethods) {
                builder.Append(delimiter);
                publicMethod.Render(builder, true, isInnerClass, indent, level + 1);
                delimiter = "\n";
            }

            // private methods
            delimiter = "";
            foreach (var method in methods.PrivateMethods) {
                builder.Append(delimiter);
                method.Render(builder, false, isInnerClass, indent, level + 1);
                delimiter = "\n";
            }

            indent.Indent(builder, level);
            builder.Append("}");
        }

        public void AddMethod(
            string name,
            CodegenMethod methodNode)
        {
            methods.Add(new Pair<string, CodegenMethod>(name, methodNode));
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
        }
    }
} // end of namespace