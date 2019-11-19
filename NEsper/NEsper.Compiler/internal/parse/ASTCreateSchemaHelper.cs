///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Antlr4.Runtime;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTCreateSchemaHelper
    {
        public static CreateSchemaDesc WalkCreateSchema(EsperEPL2GrammarParser.CreateSchemaExprContext ctx)
        {
            var assignedType = AssignedType.NONE;
            if (ctx.keyword != null)
            {
                assignedType = AssignedTypeExtensions.ParseKeyword(ctx.keyword.Text);
            }

            return GetSchemaDesc(ctx.createSchemaDef(), assignedType);
        }

        private static CreateSchemaDesc GetSchemaDesc(
            EsperEPL2GrammarParser.CreateSchemaDefContext ctx,
            AssignedType assignedType)
        {
            var schemaName = ctx.name.Text;
            var columnTypes = GetColTypeList(ctx.createColumnList());

            // get model-after types (could be multiple for variants)
            ISet<string> typeNames = new LinkedHashSet<string>();
            if (ctx.variantList() != null)
            {
                IList<EsperEPL2GrammarParser.VariantListElementContext> variantCtxs = ctx.variantList().variantListElement();
                foreach (var variantCtx in variantCtxs)
                {
                    typeNames.Add(variantCtx.GetText());
                }
            }

            // get inherited and start timestamp and end timestamps
            string startTimestamp = null;
            string endTimestamp = null;
            ISet<string> inherited = new LinkedHashSet<string>();
            ISet<string> copyFrom = new LinkedHashSet<string>();
            if (ctx.createSchemaQual() != null)
            {
                IList<EsperEPL2GrammarParser.CreateSchemaQualContext> qualCtxs = ctx.createSchemaQual();
                foreach (var qualCtx in qualCtxs)
                {
                    var qualName = qualCtx.i.Text.ToLowerInvariant();
                    var cols = ASTUtil.GetIdentList(qualCtx.columnList());
                    if (string.Equals(qualName, "inherits", StringComparison.InvariantCultureIgnoreCase))
                    {
                        inherited.AddAll(cols);
                        continue;
                    }

                    if (string.Equals(qualName, "starttimestamp", StringComparison.InvariantCultureIgnoreCase))
                    {
                        startTimestamp = cols[0];
                        continue;
                    }

                    if (string.Equals(qualName, "endtimestamp", StringComparison.InvariantCultureIgnoreCase))
                    {
                        endTimestamp = cols[0];
                        continue;
                    }

                    if (string.Equals(qualName, "copyfrom", StringComparison.InvariantCultureIgnoreCase))
                    {
                        copyFrom.AddAll(cols);
                        continue;
                    }

                    throw new EPException(
                        "Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered '" +
                        qualName + "'");
                }
            }

            return new CreateSchemaDesc(schemaName, typeNames, columnTypes, inherited, assignedType, startTimestamp, endTimestamp, copyFrom);
        }

        public static IList<ColumnDesc> GetColTypeList(EsperEPL2GrammarParser.CreateColumnListContext ctx)
        {
            if (ctx == null)
            {
                return new EmptyList<ColumnDesc>();
            }

            IList<ColumnDesc> result = new List<ColumnDesc>(ctx.createColumnListElement().Length);
            foreach (var colctx in ctx.createColumnListElement())
            {
                var colname = colctx.classIdentifier();
                var name = ASTUtil.UnescapeClassIdent(colname);
                var classIdent = ASTClassIdentifierHelper.Walk(colctx.classIdentifierWithDimensions());
                result.Add(new ColumnDesc(name, classIdent?.ToEPL()));
            }

            return result;
        }

        internal static bool ValidateIsPrimitiveArray(IToken p)
        {
            if (p != null)
            {
                if (!p.Text.ToLowerInvariant().Equals(ClassIdentifierWArray.PRIMITIVE_KEYWORD))
                {
                    throw ASTWalkException.From(
                        "Column type keyword '" + p.Text + "' not recognized, expecting '[" + ClassIdentifierWArray.PRIMITIVE_KEYWORD + "]'");
                }

                return true;
            }

            return false;
        }
    }
} // end of namespace