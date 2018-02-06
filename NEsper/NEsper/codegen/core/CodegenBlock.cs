///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.codegen.model.statement;
using com.espertech.esper.compat;

namespace com.espertech.esper.codegen.core
{
    public class CodegenBlock : ICodegenBlock
    {
        private readonly ICodegenMethod _parentMethod;
        private readonly CodegenStatementWBlockBase _parentWBlock;
        private bool _closed;
        protected List<ICodegenStatement> _statements = new List<ICodegenStatement>();

        public CodegenBlock(ICodegenMethod parentMethod)
        {
            this._parentMethod = parentMethod;
            this._parentWBlock = null;
        }

        public CodegenBlock(CodegenStatementWBlockBase parentWBlock)
        {
            this._parentWBlock = parentWBlock;
            this._parentMethod = null;
        }

        public IList<ICodegenStatement> Statements => _statements;

        public ICodegenBlock Expression(ICodegenExpression expression)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExpression(expression));
            return this;
        }

        public ICodegenBlock IfConditionReturnConst(ICodegenExpression condition, Object constant)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfConditionReturnConst(condition, constant));
            return this;
        }

        public ICodegenBlock IfNotInstanceOf(string name, Type clazz)
        {
            return IfInstanceOf(name, clazz, true);
        }

        public ICodegenBlock IfInstanceOf(string name, Type clazz)
        {
            return IfInstanceOf(name, clazz, false);
        }

        private ICodegenBlock IfInstanceOf(string name, Type clazz, bool not)
        {
            CheckClosed();
            var ifStmt = new CodegenStatementIf(this);
            ICodegenExpression condition = !not ?
                CodegenExpressionBuilder.InstanceOf(CodegenExpressionBuilder.Ref(name), clazz) :
                CodegenExpressionBuilder.NotInstanceOf(CodegenExpressionBuilder.Ref(name), clazz);
            var block = new CodegenBlock(ifStmt);
            ifStmt.Add(condition, block);
            _statements.Add(ifStmt);
            return block;
        }

        public ICodegenBlock ForLoopInt(string name, ICodegenExpression upperLimit)
        {
            CheckClosed();
            var forStmt = new CodegenStatementForInt(this, name, upperLimit);
            var block = new CodegenBlock(forStmt);
            forStmt.Block = block;
            _statements.Add(forStmt);
            return block;
        }

        public ICodegenBlock BlockElseIf(ICodegenExpression condition)
        {
            if (_parentMethod != null)
            {
                throw new IllegalStateException("Else-If in a method-level block?");
            }
            if (!(_parentWBlock is CodegenStatementIf))
            {
                throw new IllegalStateException("Else_if in a non-if block?");
            }
            CodegenStatementIf ifStmt = (CodegenStatementIf)_parentWBlock;
            CheckClosed();
            _closed = true;
            var block = new CodegenBlock(_parentWBlock);
            ifStmt.Add(condition, block);
            return block;
        }

        public ICodegenBlock BlockElse()
        {
            if (_parentMethod != null)
            {
                throw new IllegalStateException("Else in a method-level block?");
            }
            if (!(_parentWBlock is CodegenStatementIf))
            {
                throw new IllegalStateException("Else in a non-if block?");
            }
            CodegenStatementIf ifStmt = (CodegenStatementIf)_parentWBlock;
            if (ifStmt.OptionalElse != null)
            {
                throw new IllegalStateException("Else already present");
            }
            CheckClosed();
            _closed = true;
            var block = new CodegenBlock(_parentWBlock);
            ifStmt.OptionalElse = block;
            return block;
        }

        public ICodegenBlock DeclareVarWCast(Type clazz, string var, string rhsName)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVarWCast(clazz, var, rhsName));
            return this;
        }

        public ICodegenBlock DeclareVar(Type clazz, string var, ICodegenExpression initializer)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVar(clazz, var, initializer));
            return this;
        }

        public ICodegenBlock DeclareVarNull(Type clazz, string var)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVarNull(clazz, var));
            return this;
        }

        public ICodegenBlock AssignRef(string @ref, ICodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignRef(@ref, assignment));
            return this;
        }

        public ICodegenBlock AssignArrayElement(string @ref, ICodegenExpression index, ICodegenExpression assignment)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementAssignArrayElement(@ref, index, assignment));
            return this;
        }

        public ICodegenBlock ExprDotMethod(ICodegenExpression expression, string method, params ICodegenExpression[] parameters)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementExprDotMethod(expression, method, parameters));
            return this;
        }

        public ICodegenBlock IfRefNullReturnFalse(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfRefNullReturnFalse(@ref));
            return this;
        }

        public ICodegenBlock IfRefNotTypeReturnConst(string @ref, Type type, Object constant)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfRefNotTypeReturnConst(@ref, type, constant));
            return this;
        }

        public ICodegenBlock IfRefNullReturnNull(string @ref)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementIfRefNullReturnNull(@ref));
            return this;
        }

        public ICodegenBlock DeclareVarEventPerStreamUnd(Type clazz, int streamNum)
        {
            CheckClosed();
            _statements.Add(new CodegenStatementDeclareVarEventPerStreamUnd(clazz, streamNum));
            return this;
        }

        public ICodegenBlock BlockReturn(ICodegenExpression expression)
        {
            if (_parentWBlock == null)
            {
                throw new IllegalStateException("No codeblock parent, use 'params ('methodReturn',)[] instead");
            }
            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementReturnExpression(expression));
            return _parentWBlock.Parent;
        }

        public ICodegenBlock BlockEnd()
        {
            if (_parentWBlock == null)
            {
                throw new IllegalStateException("No codeblock parent, use 'params ('methodReturn',)[] instead");
            }
            CheckClosed();
            _closed = true;
            return _parentWBlock.Parent;
        }

        public string MethodReturn(ICodegenExpression expression)
        {
            if (_parentMethod == null)
            {
                throw new IllegalStateException("No method parent, use 'params ('blockReturn',)[] instead");
            }
            CheckClosed();
            _closed = true;
            _statements.Add(new CodegenStatementReturnExpression(expression));
            return _parentMethod.MethodName;
        }

        public void Render(TextWriter textWriter)
        {
            foreach (ICodegenStatement statement in _statements)
            {
                statement.Render(textWriter);
            }
        }

        internal void MergeClasses(ICollection<Type> classes)
        {
            foreach (ICodegenStatement statement in _statements)
            {
                statement.MergeClasses(classes);
            }
        }

        private void CheckClosed()
        {
            if (_closed)
            {
                throw new IllegalStateException("Code block already closed");
            }
        }
    }
} // end of namespace