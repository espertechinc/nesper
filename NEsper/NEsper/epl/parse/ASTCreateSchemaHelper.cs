///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
	public class ASTCreateSchemaHelper
    {
	    public static CreateSchemaDesc WalkCreateSchema(EsperEPL2GrammarParser.CreateSchemaExprContext ctx)
        {
	        var assignedType = AssignedType.NONE;
	        if (ctx.keyword != null) {
	            assignedType = AssignedTypeExtensions.ParseKeyword(ctx.keyword.Text);
	        }
	        return GetSchemaDesc(ctx.createSchemaDef(), assignedType);
	    }

	    private static CreateSchemaDesc GetSchemaDesc(EsperEPL2GrammarParser.CreateSchemaDefContext ctx, AssignedType assignedType)
        {
	        var schemaName = ctx.name.Text;
	        var columnTypes = GetColTypeList(ctx.createColumnList());

	        // get model-after types (could be multiple for variants)
	        ISet<string> typeNames = new LinkedHashSet<string>();
	        if (ctx.variantList() != null) {
	            IList<EsperEPL2GrammarParser.VariantListElementContext> variantCtxs = ctx.variantList().variantListElement();
	            foreach (var variantCtx in variantCtxs) {
	                typeNames.Add(variantCtx.GetText().UnmaskTypeName());
	            }
	        }

	        // get inherited and start timestamp and end timestamps
	        string startTimestamp = null;
	        string endTimestamp = null;
	        ISet<string> inherited = new LinkedHashSet<string>();
	        ISet<string> copyFrom = new LinkedHashSet<string>();
	        if (ctx.createSchemaQual() != null) {
	            IList<EsperEPL2GrammarParser.CreateSchemaQualContext> qualCtxs = ctx.createSchemaQual();
	            foreach (var qualCtx in qualCtxs) {
	                var qualName = qualCtx.i.Text.ToLower();
	                var cols = ASTUtil.GetIdentList(qualCtx.columnList());
	                var qualNameLower = qualName.ToLower();
	                switch (qualNameLower)
	                {
	                    case "inherits":
	                        inherited.AddAll(cols);
	                        continue;
	                    case "starttimestamp":
	                        startTimestamp = cols[0];
	                        continue;
	                    case "endtimestamp":
	                        endTimestamp = cols[0];
	                        continue;
	                    case "copyfrom":
	                        copyFrom.AddAll(cols);
	                        continue;
	                }
	                throw new EPException("Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered '" + qualName + "'");
	            }
	        }

	        return new CreateSchemaDesc(schemaName, typeNames, columnTypes, inherited, assignedType, startTimestamp, endTimestamp, copyFrom);
	    }

	    public static IList<ColumnDesc> GetColTypeList(EsperEPL2GrammarParser.CreateColumnListContext ctx)
	    {
	        if (ctx == null) {
	            return Collections.GetEmptyList<ColumnDesc>();
	        }
	        IList<ColumnDesc> result = new List<ColumnDesc>(ctx.createColumnListElement().Length);
	        foreach (var colctx in ctx.createColumnListElement()) {
	            IList<EsperEPL2GrammarParser.ClassIdentifierContext> idents = colctx.classIdentifier();
	            var name = ASTUtil.UnescapeClassIdent(idents[0]);

                string type;
                if (colctx.VALUE_NULL() != null)
                {
                    type = null;
                }
                else
                {
                    type = ASTUtil.UnescapeClassIdent(idents[1]);
                } 
                
                var array = colctx.b != null;
	            var primitiveArray = ValidateIsPrimitiveArray(colctx.p);
	            result.Add(new ColumnDesc(name, type, array, primitiveArray));
	        }
	        return result;
	    }

	    public static bool ValidateIsPrimitiveArray(IToken p)
        {
	        if (p != null) {
	            if (!p.Text.ToLower().Equals("primitive")) {
	                throw ASTWalkException.From("Column type keyword '" + p.Text + "' not recognized, expecting '[primitive]'");
	            }
	            return true;
	        }
	        return false;
	    }
	}
} // end of namespace
