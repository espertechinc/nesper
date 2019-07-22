///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.model.statement;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenBlock
    {
        private readonly CodegenCtor parentCtor;
        private readonly CodegenMethod parentMethodNode;
        private readonly CodegenStatementWBlockBase parentWBlock;
        private bool closed;
        protected IList<CodegenStatement> statements = new List<CodegenStatement>(4);

        private CodegenBlock(
            CodegenCtor parentCtor,
            CodegenMethod parentMethodNode,
            CodegenStatementWBlockBase parentWBlock)
        {
            this.parentCtor = parentCtor;
            this.parentMethodNode = parentMethodNode;
            this.parentWBlock = parentWBlock;
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

        public CodegenBlock Expression(CodegenExpression expression)
        {
            CheckClosed();
            statements.Add(new CodegenStatementExpression(expression));
            return this;
        }

        public CodegenBlock Decrement(CodegenExpressionRef expression)
        {
            CheckClosed();
            statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Decrement(expression)));
            return this;
        }

        public CodegenBlock Decrement(string @ref)
        {
            CheckClosed();
            statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Decrement(@ref)));
            return this;
        }

        public CodegenBlock Increment(CodegenExpressionRef expression)
        {
            CheckClosed();
            statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Increment(expression)));
            return this;
        }

        public CodegenBlock Increment(string @ref)
        {
            CheckClosed();
            statements.Add(new CodegenStatementExpression(CodegenExpressionBuilder.Increment(@ref)));
            return this;
        }

        public CodegenBlock IfConditionReturnConst(
            CodegenExpression condition,
            object constant)
        {
            CheckClosed();
            statements.Add(new CodegenStatementIfConditionReturnConst(condition, constant));
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

        public CodegenBlock IfRefNull(CodegenExpressionRef @ref)
        {
            return IfCondition(EqualsNull(@ref));
        }

        public CodegenBlock IfRefNotNull(string @ref)
        {
            return IfCondition(NotEqualsNull(Ref(@ref)));
        }

        public CodegenBlock IfCondition(CodegenExpression condition)
        {
            CheckClosed();
            var builder = new CodegenStatementIf(this);
            statements.Add(builder);
            return builder.IfBlock(condition);
        }

        public CodegenBlock SynchronizedOn(CodegenExpression expression)
        {
            CheckClosed();
            var builder = new CodegenStatementSynchronized(this, expression);
            statements.Add(builder);
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
            statements.Add(forStmt);
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
            statements.Add(forStmt);
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
            statements.Add(forStmt);
            return block;
        }

        public CodegenBlock TryCatch()
        {
            CheckClosed();
            var tryCatch = new CodegenStatementTryCatch(this);
            var block = new CodegenBlock(tryCatch);
            tryCatch.Try = block;
            statements.Add(tryCatch);
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
            statements.Add(new CodegenStatementDeclareVarWCast(clazz, var, rhsName));
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
            CheckClosed();
            statements.Add(new CodegenStatementDeclareVar(clazz, var, initializer));
            return this;
        }

        public CodegenBlock DeclareVar(
            string typeName,
            string var,
            CodegenExpression initializer)
        {
            CheckClosed();
            statements.Add(new CodegenStatementDeclareVar(typeName, var, initializer));
            return this;
        }

#if false
        public CodegenBlock DeclareVar(
            Type clazz,
            Type optionalTypeVariable,
            string var,
            CodegenExpression initializer)
        {
            CheckClosed();
            statements.Add(new CodegenStatementDeclareVar(clazz, optionalTypeVariable, var, initializer));
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
            statements.Add(new CodegenStatementDeclareVar(clazz, var, null));
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
            statements.Add(new CodegenStatementDeclareVarNull(clazz, var));
            return this;
        }

        public CodegenBlock AssignRef(
            string @ref,
            CodegenExpression assignment)
        {
            CheckClosed();
            statements.Add(new CodegenStatementAssignNamed(@ref, assignment));
            return this;
        }

        public CodegenBlock AssignRef(
            CodegenExpression @ref,
            CodegenExpression assignment)
        {
            CheckClosed();
            statements.Add(new CodegenStatementAssignRef(@ref, assignment));
            return this;
        }

        public CodegenBlock BreakLoop()
        {
            CheckClosed();
            statements.Add(CodegenStatementBreakLoop.INSTANCE);
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
            statements.Add(new CodegenStatementAssignArrayElement2Dim(Ref(@ref), indexOne, indexTwo, assignment));
            return this;
        }

        public CodegenBlock AssignArrayElement(
            CodegenExpression @ref,
            CodegenExpression index,
            CodegenExpression assignment)
        {
            CheckClosed();
            statements.Add(new CodegenStatementAssignArrayElement(@ref, index, assignment));
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

        public CodegenBlock InstanceMethod(
            CodegenMethod methodNode,
            params CodegenExpression[] parameters)
        {
            Expression(new CodegenExpressionLocalMethod(methodNode, Arrays.AsList(parameters)));
            return this;
        }

        public CodegenBlock IfRefNullReturnFalse(string @ref)
        {
            CheckClosed();
            statements.Add(new CodegenStatementIfRefNullReturnFalse(@ref));
            return this;
        }

        public CodegenBlock IfRefNotTypeReturnConst(
            string @ref,
            Type type,
            object constant)
        {
            CheckClosed();
            statements.Add(new CodegenStatementIfRefNotTypeReturnConst(@ref, type, constant));
            return this;
        }

        public CodegenBlock IfRefNullReturnNull(string @ref)
        {
            CheckClosed();
            statements.Add(new CodegenStatementIfRefNullReturnNull(Ref(@ref)));
            return this;
        }

        public CodegenBlock IfRefNullReturnNull(CodegenExpressionRef @ref)
        {
            CheckClosed();
            statements.Add(new CodegenStatementIfRefNullReturnNull(@ref));
            return this;
        }

        public CodegenBlock BlockReturn(CodegenExpression expression)
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementReturnExpression(expression));
            return parentWBlock.Parent;
        }

        public CodegenBlock BlockReturnNoValue()
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(CodegenStatementReturnNoValue.INSTANCE);
            return parentWBlock.Parent;
        }

        public CodegenStatementTryCatch TryReturn(CodegenExpression expression)
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            if (!(parentWBlock is CodegenStatementTryCatch)) {
                throw new IllegalStateException("Codeblock parent is not try-catch");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementReturnExpression(expression));
            return (CodegenStatementTryCatch) parentWBlock;
        }

        public CodegenStatementTryCatch TryEnd()
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            if (!(parentWBlock is CodegenStatementTryCatch)) {
                throw new IllegalStateException("Codeblock parent is not try-catch");
            }

            closed = true;
            return (CodegenStatementTryCatch) parentWBlock;
        }

        public CodegenBlock BlockThrow(CodegenExpression expression)
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementThrow(expression));
            return parentWBlock.Parent;
        }

        public CodegenBlock BlockEnd()
        {
            if (parentWBlock == null) {
                throw new IllegalStateException("No codeblock parent, use 'params methodReturn[] instead");
            }

            CheckClosed();
            closed = true;
            return parentWBlock.Parent;
        }

        public CodegenMethod MethodThrowUnsupported()
        {
            if (parentMethodNode == null) {
                throw new IllegalStateException("No method parent, use 'blockReturn...' instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementThrow(NewInstance(typeof(UnsupportedOperationException))));
            return parentMethodNode;
        }

        public CodegenMethod MethodThrow(CodegenExpression expression)
        {
            if (parentMethodNode == null) {
                throw new IllegalStateException("No method parent, use 'blockReturn...' instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementThrow(expression));
            return parentMethodNode;
        }

        public CodegenMethod MethodReturn(CodegenExpression expression)
        {
            if (parentMethodNode == null) {
                throw new IllegalStateException("No method parent, use 'blockReturn...' instead");
            }

            CheckClosed();
            closed = true;
            statements.Add(new CodegenStatementReturnExpression(expression));
            return parentMethodNode;
        }

        public CodegenMethod MethodEnd()
        {
            if (parentMethodNode == null) {
                throw new IllegalStateException("No method node parent, use 'params blockReturn[] instead");
            }

            CheckClosed();
            closed = true;
            return parentMethodNode;
        }

        public void CtorEnd()
        {
            if (parentCtor == null) {
                throw new IllegalStateException("No ctor node parent");
            }

            CheckClosed();
            closed = true;
        }

        public void Render(
            StringBuilder builder,
            bool isInnerClass,
            int level,
            CodegenIndent indent)
        {
            foreach (var statement in statements) {
                indent.Indent(builder, level);
                statement.Render(builder, isInnerClass, level, indent);
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            foreach (var statement in statements) {
                statement.MergeClasses(classes);
            }
        }

        private void CheckClosed()
        {
            if (closed) {
                throw new IllegalStateException("Code block already closed");
            }
        }

        public CodegenBlock IfElseIf(CodegenExpression condition)
        {
            CheckClosed();
            closed = true;
            if (parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            var ifBuilder = (CodegenStatementIf) parentWBlock;
            return ifBuilder.AddElseIf(condition);
        }

        public CodegenBlock IfElse()
        {
            closed = true;
            if (parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            var ifBuilder = (CodegenStatementIf) parentWBlock;
            return ifBuilder.AddElse();
        }

        public void IfReturn(CodegenExpression result)
        {
            CheckClosed();
            closed = true;
            if (parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            if (!(parentWBlock is CodegenStatementIf)) {
                throw new IllegalStateException("If-block-end in method?");
            }

            statements.Add(new CodegenStatementReturnExpression(result));
        }

        public CodegenBlock BlockContinue()
        {
            CheckClosed();
            closed = true;
            if (parentMethodNode != null) {
                throw new IllegalStateException("If-block-end in method?");
            }

            statements.Add(CodegenStatementContinue.INSTANCE);
            return parentWBlock.Parent;
        }

        public CodegenBlock WhileLoop(CodegenExpression expression)
        {
            return WhileOrDoLoop(expression, true);
        }

        public CodegenBlock DoLoop(CodegenExpression expression)
        {
            return WhileOrDoLoop(expression, false);
        }

        public void ReturnMethodOrBlock(CodegenExpression expression)
        {
            if (parentMethodNode != null) {
                MethodReturn(expression);
            }
            else {
                BlockReturn(expression);
            }
        }

        public CodegenBlock[] SwitchBlockOfLength(
            string @ref,
            int length,
            bool withDefaultUnsupported)
        {
            CheckClosed();
            var options = new int[length];
            for (var i = 1; i < length; i++) {
                options[i] = i;
            }

            var switchStmt = new CodegenStatementSwitch(this, @ref, options, withDefaultUnsupported);
            statements.Add(switchStmt);
            return switchStmt.Blocks;
        }

        public CodegenBlock[] SwitchBlockOptions(
            string @ref,
            int[] options,
            bool withDefaultUnsupported)
        {
            CheckClosed();
            var switchStmt = new CodegenStatementSwitch(this, @ref, options, withDefaultUnsupported);
            statements.Add(switchStmt);
            return switchStmt.Blocks;
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

        public CodegenBlock ApplyConditional(
            bool flag,
            Consumer<CodegenBlock> consumer)
        {
            if (flag) {
                Apply(consumer);
            }

            return this;
        }

        public CodegenBlock AssignCompound(
            CodegenExpressionRef expressionRef,
            string @operator,
            CodegenExpression assignment)
        {
            CheckClosed();
            statements.Add(new CodegenStatementAssignCompound(expressionRef, @operator, assignment));
            return this;
        }

        public CodegenBlock AssignCompound(
            string @ref,
            string @operator,
            CodegenExpression assignment)
        {
            return AssignCompound(Ref(@ref), @operator, assignment);
        }

        public CodegenBlock CommentFullLine(string comment)
        {
            CheckClosed();
            statements.Add(new CodegenStatementCommentFullLine(comment));
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
            statements.Add(whileStmt);
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
    }
} // end of namespace