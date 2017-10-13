///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.parse
{
    public class ASTTableHelper
    {
        public static IList<CreateTableColumn> GetColumns(IList<EsperEPL2GrammarParser.CreateTableColumnContext> ctxs, IDictionary<ITree, ExprNode> astExprNodeMap, EngineImportService engineImportService)
        {
            IList<CreateTableColumn> cols = new List<CreateTableColumn>(ctxs.Count);
            foreach (EsperEPL2GrammarParser.CreateTableColumnContext colctx in ctxs)
            {
                cols.Add(GetColumn(colctx, astExprNodeMap, engineImportService));
            }
            return cols;
        }

        private static CreateTableColumn GetColumn(EsperEPL2GrammarParser.CreateTableColumnContext ctx, IDictionary<ITree, ExprNode> astExprNodeMap, EngineImportService engineImportService)
        {
            string columnName = ctx.n.Text;

            ExprNode optExpression = null;
            if (ctx.builtinFunc() != null || ctx.libFunction() != null)
            {
                optExpression = ASTExprHelper.ExprCollectSubNodes(ctx, 0, astExprNodeMap)[0];
            }

            string optTypeName = null;
            bool? optTypeIsArray = null;
            bool? optTypeIsPrimitiveArray = null;
            if (ctx.createTableColumnPlain() != null)
            {
                EsperEPL2GrammarParser.CreateTableColumnPlainContext sub = ctx.createTableColumnPlain();
                optTypeName = ASTUtil.UnescapeClassIdent(sub.classIdentifier());
                optTypeIsArray = sub.b != null;
                optTypeIsPrimitiveArray = ASTCreateSchemaHelper.ValidateIsPrimitiveArray(sub.p);
            }

            bool primaryKey = false;
            if (ctx.p != null)
            {
                if (ctx.p.Text.ToLowerInvariant() != "primary")
                {
                    throw ASTWalkException.From("Invalid keyword '" + ctx.p.Text + "' encountered, expected 'primary key'");
                }
                if (ctx.k.Text.ToLowerInvariant() != "key")
                {
                    throw ASTWalkException.From("Invalid keyword '" + ctx.k.Text + "' encountered, expected 'primary key'");
                }
                primaryKey = true;
            }

            IList<AnnotationDesc> annots = Collections.GetEmptyList<AnnotationDesc>();
            if (ctx.annotationEnum() != null)
            {
                annots = new List<AnnotationDesc>(ctx.annotationEnum().Length);
                foreach (EsperEPL2GrammarParser.AnnotationEnumContext anctx in ctx.annotationEnum())
                {
                    annots.Add(ASTAnnotationHelper.Walk(anctx, engineImportService));
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

            return new CreateTableColumn(columnName, optExpression, optTypeName, optTypeIsArray, optTypeIsPrimitiveArray, annots, primaryKey);
        }
    }
}
