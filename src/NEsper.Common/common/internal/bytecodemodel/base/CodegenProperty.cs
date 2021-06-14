///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenProperty : CodegenPropertyScope
    {
        protected CodegenProperty(
            Type returnType,
            string returnTypeName,
            Type generator,
            CodegenSymbolProvider optionalSymbolProvider,
            CodegenScope env)
        {
            if (generator == null) {
                throw new ArgumentException("Invalid null generator");
            }

            ReturnType = returnType;
            ReturnTypeName = returnTypeName;
            OptionalSymbolProvider = optionalSymbolProvider;
            Block = new CodegenBlock(); // no return from the property block
            GetterStatement = new Getter(Block);
            SetterStatement = new Getter(Block);
            GetterBlock = new CodegenBlock(GetterStatement);
            SetterBlock = new CodegenBlock(SetterStatement);
            if (env.IsDebug) {
                AdditionalDebugInfo = GetGeneratorDetail(generator);
            }
            else {
                AdditionalDebugInfo = generator.GetSimpleName();
            }
        }

        public CodegenSymbolProvider OptionalSymbolProvider { get; }

        public IList<CodegenMethod> Children { get; } = Collections.GetEmptyList<CodegenMethod>();

        public IList<CodegenExpressionRef> Environment { get; private set; } =
            Collections.GetEmptyList<CodegenExpressionRef>();

        public Type ReturnType { get; }

        public string ReturnTypeName { get; }

        public string AdditionalDebugInfo { get; }

        public CodegenBlock Block { get; }

        public CodegenStatementWBlockBase GetterStatement { get; }

        public CodegenBlock GetterBlock { get; }

        public CodegenStatementWBlockBase SetterStatement { get; }

        public CodegenBlock SetterBlock { get; }

        public MemberModifier Modifiers { get; set; }

        public bool IsOverride => Modifiers.IsOverride();

        public bool IsVirtual => Modifiers.IsVirtual();

        public bool IsStatic => Modifiers.IsStatic();

        public CodegenPropertyWGraph AssignedProperty { get; set; }
        
        public String AssignedProviderClassName { get; set;  }

        public CodegenProperty WithStatic(bool value = true)
        {
            Modifiers = Modifiers.Enable(MemberModifier.STATIC);
            return this;
        }

        public CodegenProperty WithVirtual()
        {
            Modifiers = Modifiers
                .Enable(MemberModifier.VIRTUAL)
                .Disable(MemberModifier.OVERRIDE);
            return this;
        }
        
        public CodegenProperty WithOverride()
        {
            Modifiers = Modifiers
                .Enable(MemberModifier.OVERRIDE)
                .Disable(MemberModifier.VIRTUAL);
            return this;
        }
        
        public CodegenProperty MakeChild(
            Type returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenProperty(returnType, null, generator, null, env));
        }

        public CodegenProperty MakeChild(
            string returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenProperty(null, returnType, generator, null, env));
        }

        public CodegenProperty MakeChildWithScope(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenProperty(returnType, null, generator, symbolProvider, env));
        }

        public CodegenProperty MakeChildWithScope(
            string returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenProperty(null, returnType, generator, symbolProvider, env));
        }

        public static CodegenProperty MakePropertyNode<T>(
            Type generator,
            CodegenScope env)
        {
            return MakePropertyNode(typeof(T), generator, env);
        }

        public static CodegenProperty MakePropertyNode(
            Type returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return new CodegenProperty(returnType, null, generator, CodegenSymbolProviderEmpty.INSTANCE, env);
        }

        public static CodegenProperty MakePropertyNode(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            if (symbolProvider == null) {
                throw new ArgumentException("No symbol provider");
            }

            return new CodegenProperty(returnType, null, generator, symbolProvider, env);
        }

        public static CodegenProperty MakePropertyNode(
            string returnTypeName,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnTypeName == null) {
                throw new ArgumentException("Invalid null return type");
            }

            if (symbolProvider == null) {
                throw new ArgumentException("No symbol provider");
            }

            return new CodegenProperty(null, returnTypeName, generator, symbolProvider, env);
        }

        public CodegenProperty AddSymbol(CodegenExpressionRef symbol)
        {
            if (Environment.IsEmpty()) {
                Environment = new List<CodegenExpressionRef>(4);
            }

            Environment.Add(symbol);
            return this;
        }

        public virtual void MergeClasses(ISet<Type> classes)
        {
            GetterBlock.MergeClasses(classes);
            classes.AddToSet(ReturnType);
        }

        private string GetGeneratorDetail(Type generator)
        {
            var stack = new StackTrace();
            string stackString = null;
            for (var i = 1; i < 10; i++) {
                var frame = stack.GetFrame(i);
                var method = frame.GetMethod();
                if (method.DeclaringType.Namespace.Contains(typeof(CodegenProperty).Namespace)) {
                    continue;
                }

                stackString = GetStackString(i, stack);
                break;
            }

            if (stackString == null) {
                stackString = GetStackString(3, stack);
            }

            if (stackString.Contains("MakeSelectExprProcessors")) {
                throw new UnsupportedOperationException();
            }

            return generator.GetSimpleName() + " --- " + stackString;
        }

        private string GetStackString(
            int i,
            StackTrace stack)
        {
            var stackFrame = stack.GetFrame(i);
            var fullClassName = stackFrame.GetMethod().DeclaringType.FullName;
            var className = fullClassName.Substring(fullClassName.LastIndexOf(".") + 1);
            var methodName = stackFrame.GetMethod().Name;
            var lineNumber = stackFrame.GetFileLineNumber();
            return className + "." + methodName + "():" + lineNumber;
        }

        private CodegenProperty AddChild(CodegenProperty propertyNode)
        {
#if false
            if (Children.IsEmpty()) {
                Children = new List<CodegenMethod>();
            }

            Children.Add(propertyNode);
#endif
            return propertyNode;
        }
    }

    public class Getter : CodegenStatementWBlockBase
    {
        public Getter(CodegenBlock parent) : base(parent)
        {
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            throw new NotImplementedException();
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            throw new NotImplementedException();
        }

        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            throw new NotImplementedException();
        }
    }

    public class Setter : CodegenStatementWBlockBase
    {
        public Setter(CodegenBlock parent) : base(parent)
        {
        }

        public override void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            throw new NotImplementedException();
        }

        public override void MergeClasses(ISet<Type> classes)
        {
            throw new NotImplementedException();
        }
        
        public override void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            throw new NotImplementedException();
        }
    }
} // end of namespace