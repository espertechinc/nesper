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
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenBlock
    {
        private readonly CodegenCtor _parentCtor;
        private readonly CodegenMethod _parentMethodNode;
        private readonly CodegenStatementWBlockBase _parentWBlock;
        private bool _closed;

        private IList<CodegenStatement> _statements = new List<CodegenStatement>(4);

        /// <summary>
        /// Returns the current set of statements.
        /// </summary>
        public IList<CodegenStatement> Statements => _statements;

        /// <summary>
        /// Returns true if the block is closed.
        /// </summary>
        public bool IsClosed => _closed;

        //private BlockSyntax blockSyntax;

        private CodegenBlock(
            CodegenCtor parentCtor,
            CodegenMethod parentMethodNode,
            CodegenStatementWBlockBase parentWBlock)
        {
            this._parentCtor = parentCtor;
            this._parentMethodNode = parentMethodNode;
            this._parentWBlock = parentWBlock;
        }

        public CodegenBlock()
            : this(null, null, null)
        {
        }

        public CodegenBlock(CodegenCtor parentCtor)
            : this(parentCtor, null, null)
        {
        }

        public CodegenBlock(CodegenMethod parentMethodNode)
            : this(null, parentMethodNode, null)
        {
        }

        public CodegenBlock(CodegenStatementWBlockBase parentWBlock)
            : this(null, null, parentWBlock)
        {
        }

        public bool IsEmpty()
        {
            return _statements.IsEmpty();
            //return blockSyntax.Statements.IsNullOrEmpty();
        }

        public bool IsNotEmpty()
        {
            return _statements.IsNotEmpty();
            //return !blockSyntax.Statements.IsNullOrEmpty();
        }

        public CodegenBlock Expression(CodegenExpression expression)
        {
            CheckClosed();

            _statements.Add(new CodegenStatementExpression(expression));
            return this;
        }

        public CodegenBlock Decrement(CodegenExpression expression)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Decrement(expression)));
            return this;
        }

        public CodegenBlock DecrementRef(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.DecrementRef(@ref)));
            return this;
        }

        public CodegenBlock Increment(CodegenExpression expression)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Increment(expression)));
            return this;
        }

        public CodegenBlock IncrementRef(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.IncrementRef(@ref)));
            return this;
        }

        public CodegenBlock IfConditionReturnConst(
            CodegenExpression condition,
            object constant)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfConditionReturnConst(condition, constant));
            return this;
        }

        public CodegenBlock IfNotInstanceOf(
            string name,
            Type clazz)
        {
            return IfInstanceOf(name, clazz, true);
        }

        public CodegenBlock IfInstanceOf(
            string name,
            Type clazz)
        {
            return IfInstanceOf(name, clazz, false);
        }

        private CodegenBlock IfInstanceOf(
            string name,
            Type clazz,
            bool not)
        {
            return IfCondition(!not ? InstanceOf(Ref(name), clazz) : NotInstanceOf(Ref(name), clazz));
        }

        public CodegenBlock IfRefNull(string @ref)
        {
            return IfCondition(EqualsNull(Ref(@ref)));
        }

        public CodegenBlock IfNull(CodegenExpression expression)
        {
            return IfCondition(EqualsNull(expression));
        }

        public CodegenBlock IfRefNotNull(string @ref)
        {
            return IfCondition(NotEqualsNull(Ref(@ref)));
        }

        public CodegenBlock IfCondition(CodegenExpression condition)
        {
            CheckClosed();
            var builder = new CodegenStatementIf(this);
            _statements.Add(builder);
            return builder.IfBlock(condition);
        }

        public CodegenBlock LockOn(CodegenExpression expression)
        {
            CheckClosed();
            var builder = new CodegenStatementSynchronized(this, expression);
            _statements.Add(builder);
            return builder.MakeBlock();
        }

        public CodegenBlock ForLoopIntSimple(
            string name,
            CodegenExpression upperLimit)
        {
            CheckClosed();
            var forStmt = new CodegenStatementForIntSimple(this, name, upperLimit);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public CodegenBlock ForLoop(
            Type type,
            string name,
            CodegenExpression initialization,
            CodegenExpression termination,
            CodegenExpression increment)
        {
            CheckClosed();
            var forStmt = new CodegenStatementFor(this, type, name, initialization, termination, increment);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public CodegenBlock ForEach(
            Type type,
            string name,
            CodegenExpression target)
        {
            CheckClosed();
            var forStmt = new CodegenStatementForEach(this, type, name, target);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public CodegenBlock ForEach<T>(
            string name,
            CodegenExpression target)
        {
            CheckClosed();
            var type = typeof(T);
            var forStmt = new CodegenStatementForEach(this, type, name, target);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public CodegenBlock ForEachVar(
            string name,
            CodegenExpression target)
        {
            CheckClosed();
            var forStmt = new CodegenStatementForEach(this, name, target);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public CodegenBlock TryCatch()
        {
            CheckClosed();
            var tryCatch = new CodegenStatementTryCatch(this);
            var block = new CodegenBlock(tryCatch);
            tryCatch.Try = block;
            _statements.Add(tryCatch);
            return block;
        }

        public CodegenBlock DeclareVarWCast<T>(
            string var,
            string rhsName)
        {
            return DeclareVarWCast(typeof(T), var, rhsName);
        }

        public CodegenBlock DeclareVarWCast(
            Type clazz,
            string var,
            string rhsName)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVarWCast(clazz, var, rhsName));
            return this;
        }

        public CodegenBlock DeclareVar<T>(
            string var,
            CodegenExpression initializer)
        {
            return DeclareVar(typeof(T), var, initializer);
        }

        public CodegenBlock DeclareVar(
            Type clazz,
            string var,
            CodegenExpression initializer)
        {
            if (initializer == null) {
                throw new ArgumentNullException(nameof(initializer));
            }

            CheckClosed();
#if DEBUG && STACKTRACE
            IncludeMinStack(stackFrame => 
                stackFrame.HasMethod() && 
                stackFrame.GetMethod().Name != "DeclareVar");
#endif
            _statements.Add(new CodegenStatementDeclareVar(clazz, var, initializer));
            return this;
        }

        public CodegenBlock DeclareVar(
            string typeName,
            string var,
            CodegenExpression initializer)
        {
            if (initializer == null) {
                throw new ArgumentNullException(nameof(initializer));
            }

            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVar(typeName, var, initializer));
            return this;
        }

#if DEBUG && STACKTRACE
        private CodegenBlock IncludeMinStack(Predicate<StackFrame> stackFramePredicate)
        {
            var stackTrace = new StackTrace(true);
            var stackFrames = stackTrace.GetFrames();

            var stackFrameStart = 1;
            while (stackFrameStart < stackFrames.Length && !stackFramePredicate.Invoke(stackFrames[stackFrameStart])) {
                stackFrameStart++;
            }

            var stackFrameCount = Math.Min(stackFrames.Length, stackFrameStart + 3);
            for (var ii = stackFrameStart; ii < stackFrameCount; ii++) {
                var comment = string.Format(
                    "#{3}: File: {0}, Line: {1}, Method: {2}",
                    stackFrames[ii].GetFileName(),
                    stackFrames[ii].GetFileLineNumber(),
                    stackFrames[ii].GetMethod().Name,
                    ii);
                _statements.Add(new CodegenStatementCommentFullLine(comment));
            }

            return this;
        }
#endif

        public CodegenBlock DeclareVarNoInit<T>(
            string var)
        {
            return DeclareVarNoInit(typeof(T), var);
        }

        public CodegenBlock DeclareVarNoInit(
            Type clazz,
            string var)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVar(clazz, var, null));
            return this;
        }


        public CodegenBlock DeclareVarNull<T>(
            string var)
        {
            return DeclareVarNull(typeof(T), var);
        }

        public CodegenBlock DeclareVarNull(
            Type clazz,
            string var)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVarNull(clazz, var));
            return this;
        }

        public CodegenBlock SuperCtor(params CodegenExpression[] parameters)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementBaseCtor(parameters));
            return this;
        }

        public CodegenBlock AssignRef(
            string @ref,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignNamed(Ref(@ref), assignment));
            return this;
        }

        public CodegenBlock AssignMember(
            string @ref,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignNamed(Member(@ref), assignment));
            return this;
        }

        public CodegenBlock AssignRef(
            CodegenExpression @ref,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignRef(@ref, assignment));
            return this;
        }

        public CodegenBlock BreakLoop()
        {
            CheckClosed();
            _statements.Add(CodegenStatementBreakLoop.INSTANCE);
            return this;
        }

        public CodegenBlock AssignArrayElement(
            string @ref,
            CodegenExpression index,
            CodegenExpression assignment)
        {
            return AssignArrayElement(Ref(@ref), index, assignment);
        }

        public CodegenBlock AssignArrayElement2Dim(
            string @ref,
            CodegenExpression indexOne,
            CodegenExpression indexTwo,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignArrayElement2Dim(Ref(@ref), indexOne, indexTwo, assignment));
            return this;
        }

        public CodegenBlock AssignArrayElement(
            CodegenExpression @ref,
            CodegenExpression index,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignArrayElement(@ref, index, assignment));
            return this;
        }

        public CodegenBlock ExprDotMethod(
            CodegenExpression expression,
            string method,
            params CodegenExpression[] @params)
        {
            Expression(new CodegenExpressionExprDotMethod(expression, method, @params));
            return this;
        }

        public CodegenBlock StaticMethod(
            Type clazz,
            string method,
            params CodegenExpression[] @params)
        {
            Expression(new CodegenExpressionStaticMethod(clazz, method, @params));
            return this;
        }

        public CodegenBlock StaticMethod(
            string className,
            string method,
            params CodegenExpression[] @params)
        {
            Expression(new CodegenExpressionStaticMethod(className, method, @params));
            return this;
        }

        public CodegenBlock LocalMethod(
            CodegenMethod methodNode,
            params CodegenExpression[] parameters)
        {
            Expression(new CodegenExpressionLocalMethod(methodNode, Arrays.AsList(parameters)));
            return this;
        }

        public CodegenBlock IfRefNullReturnFalse(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfRefNullReturnFalse(@ref));
            return this;
        }

        public CodegenBlock IfRefNotTypeReturnConst(
            string @ref,
            Type type,
            object constant)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfRefNotTypeReturnConst(@ref, type, constant));
            return this;
        }

        public CodegenBlock IfRefNullReturnNull(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfNullReturnNull(Ref(@ref)));
            return this;
        }

        public CodegenBlock IfNullReturnNull(CodegenExpression @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfNullReturnNull(@ref));
            return this;
        }

        public CodegenBlock IfNullThrowException(
            CodegenExpression @ref,
            CodegenExpression exceptionGenerator)
        {
            return IfNull(@ref).BlockThrow(exceptionGenerator);
        }

        public CodegenBlock IfNullThrowException(
            CodegenExpression @ref,
            string message)
        {
            return IfNullThrowException(@ref, NewInstance<EPException>(Constant(message)));
        }

        public CodegenBlock IfNullThrowException(CodegenExpressionRef @ref)
        {
            return IfNullThrowException(@ref, "Null value encountered - not expected or allowed");
        }

        public CodegenBlock BlockReturn(CodegenExpression expression)
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementReturnExpression(expression));

            return _parentWBlock.Parent;
        }

        public CodegenBlock BlockReturnNoValue()
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(CodegenStatementReturnNoValue.INSTANCE);
            return _parentWBlock.Parent;
        }

        public CodegenBlock TypeReference(Type type)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementTypeReference(type));
            return this;
        }

        public CodegenBlock TypeReference<T>()
        {
            return TypeReference(typeof(T));
        }

        public CodegenStatementTryCatch TryReturn(CodegenExpression expression)
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            if (!(_parentWBlock is CodegenStatementTryCatch)) {
                throw new IllegalStateException("Codeblock parent is not try-catch");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementReturnExpression(expression));
            return (CodegenStatementTryCatch) _parentWBlock;
        }

        public CodegenStatementTryCatch TryEnd()
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            if (!(_parentWBlock is CodegenStatementTryCatch)) {
                throw new IllegalStateException("Codeblock parent is not try-catch");
            }

            _closed = true;
            return (CodegenStatementTryCatch) _parentWBlock;
        }

        public CodegenBlock BlockThrow(CodegenExpression expression)
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementThrow(expression));
            return _parentWBlock.Parent;
        }

        public CodegenBlock BlockEnd()
        {
            if (_parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            _closed = true;
            return _parentWBlock.Parent;
        }

        public CodegenMethod MethodThrowUnsupported()
        {
            if (_parentMethodNode == null) {
                throw new IllegalStateException("No method parent, use 'blockReturn...' instead");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementThrow(NewInstance(typeof(UnsupportedOperationException))));
            return _parentMethodNode;
        }

        public CodegenMethod MethodReturn(CodegenExpression expression)
        {
            if (_parentMethodNode == null) {
                throw new IllegalStateException("No method parent, use 'blockReturn...' instead");
            }

            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementReturnExpression(expression));
            return _parentMethodNode;
        }

        public CodegenMethod MethodEnd()
        {
            if (_parentMethodNode == null) {
                throw new IllegalStateException("No method node parent, use 'params blockReturn[] instead");
            }

            CheckClosed();
            _closed = true;
            return _parentMethodNode;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            foreach (var statement in _statements) {
                indent.Indent(builder, level);
                statement.Render(builder, isInnerClass, level, indent);
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            foreach (var statement in _statements) {
                statement.MergeClasses(classes);
            }
        }

        private void CheckClosed()
        {
            if (_closed) {
                throw new IllegalStateException("Code block already closed");
            }
        }

        public CodegenBlock IfElseIf(CodegenExpression condition)
        {
            CheckClosed();
            _closed = true;
            if (_parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(_parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            var ifBuilder = (CodegenStatementIf) _parentWBlock;
            return ifBuilder.AddElseIf(condition);
        }

        public CodegenBlock IfElse()
        {
            _closed = true;
            if (_parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(_parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            var ifBuilder = (CodegenStatementIf) _parentWBlock;
            return ifBuilder.AddElse();
        }

        public void IfReturn(CodegenExpression result)
        {
            CheckClosed();
            _closed = true;
            if (_parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(_parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            _statements.Add(new CodegenStatementReturnExpression(result));
        }

        public CodegenBlock BlockContinue()
        {
            CheckClosed();
            _closed = true;
            if (_parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            _statements.Add(CodegenStatementContinue.INSTANCE);
            return _parentWBlock.Parent;
        }

        public CodegenBlock WhileLoop(CodegenExpression expression)
        {
            return WhileOrDoLoop(expression, true);
        }

        public void ReturnMethodOrBlock(CodegenExpression expression)
        {
            if (_parentMethodNode != null) {
                MethodReturn(expression);
            }
            else {
                BlockReturn(expression);
            }
        }

        public CodegenBlock[] SwitchBlockOfLength(
            CodegenExpression switchExpression,
            int length,
            bool blocksReturnValues)
        {
            var expressions = new CodegenExpression[length];
            for (var i = 0; i < length; i++) {
                expressions[i] = Constant(i);
            }

            return SwitchBlockExpressions(switchExpression, expressions, blocksReturnValues, true).Blocks;
        }

        public CodegenBlock[] SwitchBlockOptions(
            CodegenExpression switchExpression,
            int[] options,
            bool blocksReturnValues)
        {
            var expressions = new CodegenExpression[options.Length];
            for (var i = 0; i < expressions.Length; i++) {
                expressions[i] = Constant(options[i]);
            }

            return SwitchBlockExpressions(switchExpression, expressions, blocksReturnValues, true).Blocks;
        }

        public CodegenStatementSwitch SwitchBlockExpressions(
            CodegenExpression switchExpression,
            IList<CodegenExpression> expressions,
            bool blocksReturnValues,
            bool withDefaultUnsupported)
        {
            return SwitchBlockExpressions(
                switchExpression,
                expressions.ToArray(),
                blocksReturnValues,
                withDefaultUnsupported);
        }

        public CodegenStatementSwitch SwitchBlockExpressions(
            CodegenExpression switchExpression,
            CodegenExpression[] expressions,
            bool blocksReturnValues,
            bool withDefaultUnsupported)
        {
            CheckClosed();
            var switchStmt = new CodegenStatementSwitch(
                this,
                switchExpression,
                expressions,
                blocksReturnValues,
                withDefaultUnsupported);
            _statements.Add(switchStmt);
            return switchStmt;
        }

        public CodegenBlock Apply(Consumer<CodegenBlock> consumer)
        {
            CheckClosed();
            consumer.Invoke(this);
            return this;
        }

        public CodegenBlock ApplyTri<T>(
            TriConsumer<CodegenMethod, CodegenBlock, T> consumer,
            CodegenMethod methodNode,
            T symbols)
            where T : CodegenSymbolProvider
        {
            CheckClosed();
            consumer.Accept(methodNode, this, symbols);
            return this;
        }

        public CodegenBlock AssignCompound(
            CodegenExpression expression,
            String @operator,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignCompound(expression, @operator, assignment));
            return this;
        }

        public CodegenBlock AssignCompound(
            CodegenExpressionRef expressionRef,
            string @operator,
            CodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignCompound(expressionRef, @operator, assignment));
            return this;
        }

        public CodegenBlock AssignCompound(
            string @ref,
            string @operator,
            CodegenExpression assignment)
        {
            return AssignCompound(Ref(@ref), @operator, assignment);
        }

        public CodegenBlock DebugStack()
        {
#if DEBUG && STACKTRACE
            var stackTrace = new StackTrace(true);
            CheckClosed();
            var stackFrames = stackTrace.GetFrames();
            var stackFrameCount = Math.Min(stackFrames.Length, 5);
            for (var ii = 1; ii < stackFrameCount; ii++) {
                var comment = string.Format(
                    "#{3}: File: {0}, Line: {1}, Method: {2}",
                    stackFrames[ii].GetFileName(),
                    stackFrames[ii].GetFileLineNumber(),
                    stackFrames[ii].GetMethod().Name,
                    ii);
                _statements.Add(new CodegenStatementCommentFullLine(comment));
            }
#endif

            return this;
        }

        public CodegenBlock CommentFullLine(string comment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementCommentFullLine(comment));
            return this;
        }

        private CodegenBlock WhileOrDoLoop(
            CodegenExpression expression,
            bool isWhile)
        {
            CheckClosed();
            var whileStmt = new CodegenStatementWhileOrDo(this, expression, isWhile);
            var block = new CodegenBlock(whileStmt);
            whileStmt.Block = block;
            _statements.Add(whileStmt);
            return block;
        }

        public CodegenBlock SetProperty(
            CodegenExpression @ref,
            string propertyName,
            CodegenExpression value)
        {
            CheckClosed();
            AssignRef(GetProperty(@ref, propertyName), value);
            return this;
        }

        public CodegenBlock Lambda(
            Supplier<CodegenNamedParam[]> argNamesProvider,
            Consumer<CodegenBlock> bodyProvider)
        {
            CheckClosed();
            _statements.Add(
                new CodegenExpressionLambda(this)
                    .WithParams(argNamesProvider.Invoke())
                    .WithBody(bodyProvider));
            return this;
        }

        public BlockSyntax CodegenSyntax()
        {
            throw new NotImplementedException();
        }

        public CodegenBlock Debug(
            string formatString,
            params CodegenExpression[] @params)
        {
            var passThroughParams = new CodegenExpression[@params.Length + 1];
            passThroughParams[0] = Constant(formatString);
            Array.Copy(@params, 0, passThroughParams, 1, @params.Length);
            // Console.WriteLine -
            return StaticMethod(typeof(CompatExtensions), "Debug", passThroughParams);
        }
        
        public bool HasInstanceAccess(Func<CodegenMethod, bool> permittedMethods) {
            var consumer = new InstanceAccessConsumer(permittedMethods);
            foreach (var statement in _statements) {
                statement.TraverseExpressions(consumer.Accept);
                if (consumer.hasInstanceAccess) {
                    return true;
                }
            }
            return false;
        }

        public void TraverseExpressions(Consumer<CodegenExpression> consumer) {
            foreach (var statement in _statements) {
                statement.TraverseExpressions(consumer);
            }
        }

        private class InstanceAccessConsumer
        {
            internal readonly Func<CodegenMethod, bool> permittedMethod;
            internal bool hasInstanceAccess = false;

            public InstanceAccessConsumer(Func<CodegenMethod, bool> permittedMethod)
            {
                this.permittedMethod = permittedMethod;
            }

            public void Accept(CodegenExpression codegenExpression)
            {
                if (codegenExpression is CodegenExpressionMember) {
                    hasInstanceAccess = true;
                    return;
                }

                if (codegenExpression is CodegenExpressionLocalMethod) {
                    var localMethod = (CodegenExpressionLocalMethod) codegenExpression;
                    if (!permittedMethod.Invoke(localMethod.MethodNode)) {
                        hasInstanceAccess = true;
                        return;
                    }
                }

                codegenExpression.TraverseExpressions(this.Accept);
            }
        }
    }
} // end of namespace