///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    public class ExpressionCopier
    {
        private readonly ContextCompileTimeDescriptor contextCompileTimeDescriptor;
        private readonly StatementCompileTimeServices services;
        private readonly StatementSpecRaw statementSpecRaw;
        private readonly ExprNodeSubselectDeclaredDotVisitor visitor;

        public ExpressionCopier(
            StatementSpecRaw statementSpecRaw,
            ContextCompileTimeDescriptor contextCompileTimeDescriptor,
            StatementCompileTimeServices services,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            this.statementSpecRaw = statementSpecRaw;
            this.contextCompileTimeDescriptor = contextCompileTimeDescriptor;
            this.services = services;
            this.visitor = visitor;
        }

        public ExprNode Copy(ExprNode exprNode)
        {
            var expression = StatementSpecMapper.Unmap(exprNode);
            var mapEnv = services.StatementSpecMapEnv;
            var mapContext = new StatementSpecMapContext(contextCompileTimeDescriptor, mapEnv);
            var copy = StatementSpecMapper.MapExpression(expression, mapContext);

            statementSpecRaw.TableExpressions.AddAll(mapContext.TableExpressions);
            copy.Accept(visitor);

            return copy;
        }
    }
} // end of namespace