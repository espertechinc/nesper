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

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenMethod : CodegenMethodScope
    {
        internal CodegenMethod(
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
            Block = new CodegenBlock(this);
            if (env.IsDebug) {
                AdditionalDebugInfo = GetGeneratorDetail(generator);
            }
            else {
                AdditionalDebugInfo = generator.GetSimpleName();
            }
        }

        public Guid Id { get; } = Guid.NewGuid();

        public CodegenSymbolProvider OptionalSymbolProvider { get; }

        public IList<CodegenMethod> Children { get; private set; } =
            Collections.GetEmptyList<CodegenMethod>();

        public IList<CodegenExpressionRef> Environment { get; private set; } =
            Collections.GetEmptyList<CodegenExpressionRef>();

        public MemberModifier Modifiers { get; set; }

        public bool IsOverride => Modifiers.IsOverride();

        public bool IsVirtual => Modifiers.IsVirtual();

        public bool IsStatic => Modifiers.IsStatic();

        public Type ReturnType { get; }

        public string ReturnTypeName { get; }

        public string AdditionalDebugInfo { get; }

        public CodegenBlock Block { get; }

        public IList<CodegenNamedParam> LocalParams { get; private set; } =
            Collections.GetEmptyList<CodegenNamedParam>();

        public ISet<string> DeepParameters { get; set; }

        public CodegenMethodWGraph AssignedMethod { get; set; }

        public string AssignedProviderClassName { get; set; }

        public CodegenMethod WithStatic(bool value)
        {
            Modifiers = Modifiers.EnableDisable(MemberModifier.STATIC, value);
            return this;
        }

        public CodegenMethod WithVirtual()
        {
            Modifiers = Modifiers
                .Enable(MemberModifier.VIRTUAL)
                .Disable(MemberModifier.OVERRIDE);
            return this;
        }

        public CodegenMethod WithOverride()
        {
            Modifiers = Modifiers
                .Enable(MemberModifier.OVERRIDE)
                .Disable(MemberModifier.VIRTUAL);
            return this;
        }

        public SyntaxTokenList GetModifiers()
        {
            var modifiers = TokenList();

            if (IsStatic) {
                modifiers.Add(Token(SyntaxKind.StaticKeyword));
            }

            if (IsOverride) {
                modifiers.Add(Token(SyntaxKind.OverrideKeyword));
            }

            if (IsVirtual) {
                modifiers.Add(Token(SyntaxKind.VirtualKeyword));
            }

            modifiers.Add(Token(SyntaxKind.PublicKeyword));
            return modifiers;
        }


        public CodegenMethod MakeChild(
            Type returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenMethod(returnType, null, generator, null, env));
        }

        public CodegenMethod MakeChild(
            string returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenMethod(null, returnType, generator, null, env));
        }

        public CodegenMethod MakeChildWithScope(
            Type returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenMethod(returnType, null, generator, symbolProvider, env));
        }

        public static CodegenMethod MakeMethod(
            Type returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return new CodegenMethod(returnType, null, generator, CodegenSymbolProviderEmpty.INSTANCE, env);
        }

        public static CodegenMethod MakeMethod(
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

            return new CodegenMethod(returnType, null, generator, symbolProvider, env);
        }

        public static CodegenMethod MakeParentNode<T>(
            Type generator,
            CodegenScope env)
        {
            return MakeMethod(typeof(T), generator, env);
        }

        public static CodegenMethod MakeParentNode(
            Type returnType,
            Type generator,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return new CodegenMethod(returnType, null, generator, CodegenSymbolProviderEmpty.INSTANCE, env);
        }

        public static CodegenMethod MakeParentNode(
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

            return new CodegenMethod(returnType, null, generator, symbolProvider, env);
        }

        public static CodegenMethod MakeParentNode(
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

            return new CodegenMethod(null, returnTypeName, generator, symbolProvider, env);
        }

        public CodegenMethod MakeChildWithScope(
            string returnType,
            Type generator,
            CodegenSymbolProvider symbolProvider,
            CodegenScope env)
        {
            if (returnType == null) {
                throw new ArgumentException("Invalid null return type");
            }

            return AddChild(new CodegenMethod(null, returnType, generator, symbolProvider, env));
        }

        public CodegenMethod AddSymbol(CodegenExpressionRef symbol)
        {
            if (Environment.IsEmpty()) {
                Environment = new List<CodegenExpressionRef>(4);
            }

            Environment.Add(symbol);
            return this;
        }

        CodegenMethodScope CodegenMethodScope.AddSymbol(CodegenExpressionRef symbol)
        {
            return AddSymbol(symbol);
        }

        public virtual void MergeClasses(ISet<Type> classes)
        {
            Block.MergeClasses(classes);
            if (ReturnType != null) {
                classes.AddToSet(ReturnType);
            }

            foreach (var param in LocalParams) {
                param.MergeClasses(classes);
            }
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer)
        {
            Block.TraverseExpressions(consumer);
        }

        public CodegenMethod AddParam<T>(
            string name)
        {
            return AddParam<T>(name);
        }

        public CodegenMethod AddParam(
            Type type,
            string name)
        {
            if (LocalParams.IsEmpty()) {
                LocalParams = new List<CodegenNamedParam>(4);
            }

            LocalParams.Add(new CodegenNamedParam(type, name));
            return this;
        }

        public CodegenMethod AddParam(
            string typeName,
            string name)
        {
            if (LocalParams.IsEmpty()) {
                LocalParams = new List<CodegenNamedParam>(4);
            }

            LocalParams.Add(new CodegenNamedParam(typeName, name));
            return this;
        }

        public CodegenMethod AddParam(IList<CodegenNamedParam> @params)
        {
            if (LocalParams.IsEmpty()) {
                LocalParams = new List<CodegenNamedParam>(@params.Count);
            }

            LocalParams.AddAll(@params);
            return this;
        }

        public CodegenMethod AddParam(CodegenNamedParam param)
        {
            if (LocalParams.IsEmpty()) {
                LocalParams = new List<CodegenNamedParam>();
            }

            LocalParams.Add(param);
            return this;
        }

        private string GetGeneratorDetail(Type generator)
        {
#if DEBUG && STACKTRACE
            var stack = new StackTrace(true);
            string stackString = null;
            for (var i = 1; i < 10; i++) {
                var frame = stack.GetFrame(i);
                var method = frame.GetMethod();
                if (method.DeclaringType.Namespace.Contains(typeof(CodegenMethod).Namespace)) {
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
#else
            return generator.GetSimpleName();
#endif
        }

#if DEBUG && STACKTRACE
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
#endif

        private CodegenMethod AddChild(CodegenMethod methodNode)
        {
            if (Children.IsEmpty()) {
                Children = new List<CodegenMethod>();
            }

            Children.Add(methodNode);
            return methodNode;
        }

        public ICollection<CodegenExpressionLambda> GetLambdas()
        {
            return Block.Statements.OfType<CodegenExpressionLambda>().ToList();
        }

        public override string ToString()
        {
            return AssignedMethod == null
                ? "CodegenMethod"
                : "CodegenMethod{name=" + AssignedMethod.Name + "}";
        }
    }
} // end of namespace