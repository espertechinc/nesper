///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTTableHelper
    {
        public static IList<CreateTableColumn> GetColumns(
            IList<EsperEPL2GrammarParser.CreateTableColumnContext> ctxs,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            StatementSpecMapEnv mapEnv)
        {
            IList<CreateTableColumn> cols = new List<CreateTableColumn>(ctxs.Count);
            foreach (var colctx in ctxs)
            {
                cols.Add(GetColumn(colctx, astExprNodeMap, mapEnv));
            }

            return cols;
        }

        private static CreateTableColumn GetColumn(
            EsperEPL2GrammarParser.CreateTableColumnContext ctx,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            StatementSpecMapEnv mapEnv)
        {
            var columnName = ctx.n.Text;

            ExprNode optExpression = null;
            if (ctx.builtinFunc() != null || ctx.chainable() != null)
            {
                optExpression = ASTExprHelper.ExprCollectSubNodes(ctx, 0, astExprNodeMap)[0];
            }

            var optType = ASTClassIdentifierHelper.Walk(ctx.classIdentifierWithDimensions());

            var primaryKey = false;
            if (ctx.p != null)
            {
                if (!string.Equals(ctx.p.Text, "primary", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw ASTWalkException.From("Invalid keyword '" + ctx.p.Text + "' encountered, expected 'primary key'");
                }

                if (!string.Equals(ctx.k.Text, "key", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw ASTWalkException.From("Invalid keyword '" + ctx.k.Text + "' encountered, expected 'primary key'");
                }

                primaryKey = true;
            }

            IList<AnnotationDesc> annots = new EmptyList<AnnotationDesc>();
            if (ctx.annotationEnum() != null)
            {
                annots = new List<AnnotationDesc>(ctx.annotationEnum().Length);
                foreach (EsperEPL2GrammarParser.AnnotationEnumContext anctx in ctx.annotationEnum())
                {
                    annots.Add(ASTAnnotationHelper.Walk(anctx, mapEnv.ImportService));
                }
            }

            if (ctx.typeExpressionAnnotation() != null)
            {
                if (annots.IsEmpty())
                {
                    annots = new List<AnnotationDesc>();
                }

                foreach (EsperEPL2GrammarParser.TypeExpressionAnnotationContext anno in ctx.typeExpressionAnnotation())
                {
                    annots.Add(new AnnotationDesc(anno.n.Text, anno.v.Text));
                }
            }

            return new CreateTableColumn(columnName, optExpression, optType, annots, primaryKey);
        }
    }
} // end of namespace