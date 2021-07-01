///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTGraphHelper
    {
        public static CreateDataFlowDesc WalkCreateDataFlow(
            EsperEPL2GrammarParser.CreateDataflowContext ctx,
            IDictionary<ITree, object> astGraphNodeMap,
            ImportServiceCompileTime importService)
        {
            var graphName = ctx.name.Text;

            IList<GraphOperatorSpec> ops = new List<GraphOperatorSpec>();
            IList<CreateSchemaDesc> schemas = new List<CreateSchemaDesc>();

            IList<EsperEPL2GrammarParser.GopContext> gopctxs = ctx.gopList().gop();
            foreach (var gopctx in gopctxs)
            {
                if (gopctx.createSchemaExpr() != null)
                {
                    schemas.Add(ASTCreateSchemaHelper.WalkCreateSchema(gopctx.createSchemaExpr()));
                }
                else
                {
                    ops.Add(ParseOp(gopctx, astGraphNodeMap, importService));
                }
            }

            return new CreateDataFlowDesc(graphName, ops, schemas);
        }

        private static GraphOperatorSpec ParseOp(
            EsperEPL2GrammarParser.GopContext ctx,
            IDictionary<ITree, object> astGraphNodeMap,
            ImportServiceCompileTime importService)
        {
            var operatorName = ctx.opName != null ? ctx.opName.Text : ctx.s.Text;

            var input = new GraphOperatorInput();
            if (ctx.gopParams() != null)
            {
                ParseParams(ctx.gopParams(), input);
            }

            var output = new GraphOperatorOutput();
            if (ctx.gopOut() != null)
            {
                ParseOutput(ctx.gopOut(), output);
            }

            GraphOperatorDetail detail = null;
            if (ctx.gopDetail() != null)
            {
                IDictionary<string, object> configs = new LinkedHashMap<string, object>();
                IList<EsperEPL2GrammarParser.GopConfigContext> cfgctxs = ctx.gopDetail().gopConfig();
                foreach (var cfgctx in cfgctxs)
                {
                    string name;
                    object value = astGraphNodeMap.Delete(cfgctx);
                    if (cfgctx.n != null)
                    {
                        name = cfgctx.n.Text;
                    }
                    else
                    {
                        name = "select";
                    }

                    configs.Put(name, value);
                }

                detail = new GraphOperatorDetail(configs);
            }

            IList<AnnotationDesc> annotations;
            if (ctx.annotationEnum() != null)
            {
                IList<EsperEPL2GrammarParser.AnnotationEnumContext> annoctxs = ctx.annotationEnum();
                annotations = new List<AnnotationDesc>();
                foreach (EsperEPL2GrammarParser.AnnotationEnumContext annoctx in annoctxs)
                {
                    annotations.Add(ASTAnnotationHelper.Walk(annoctx, importService));
                }
            }
            else
            {
                annotations = new EmptyList<AnnotationDesc>();
            }

            return new GraphOperatorSpec(operatorName, input, output, detail, annotations);
        }

        private static void ParseParams(
            EsperEPL2GrammarParser.GopParamsContext ctx,
            GraphOperatorInput input)
        {
            if (ctx.gopParamsItemList() == null)
            {
                return;
            }

            IList<EsperEPL2GrammarParser.GopParamsItemContext> items = ctx.gopParamsItemList().gopParamsItem();
            foreach (var item in items)
            {
                var streamNames = ParseParamsStreamNames(item);
                string aliasName = item.gopParamsItemAs() != null ? item.gopParamsItemAs().a.Text : null;
                input.StreamNamesAndAliases.Add(new GraphOperatorInputNamesAlias(streamNames, aliasName));
            }
        }

        private static string[] ParseParamsStreamNames(EsperEPL2GrammarParser.GopParamsItemContext item)
        {
            IList<string> paramNames = new List<string>(1);
            if (item.gopParamsItemMany() != null)
            {
                foreach (EsperEPL2GrammarParser.ClassIdentifierContext ctx in item.gopParamsItemMany().classIdentifier())
                {
                    paramNames.Add(ctx.GetText());
                }
            }
            else
            {
                paramNames.Add(ASTUtil.UnescapeClassIdent(item.classIdentifier()));
            }

            return paramNames.ToArray();
        }

        private static void ParseOutput(
            EsperEPL2GrammarParser.GopOutContext ctx,
            GraphOperatorOutput output)
        {
            if (ctx == null)
            {
                return;
            }

            IList<EsperEPL2GrammarParser.GopOutItemContext> items = ctx.gopOutItem();
            foreach (var item in items)
            {
                string streamName = item.n.GetText();

                IList<GraphOperatorOutputItemType> types = new List<GraphOperatorOutputItemType>();
                if (item.gopOutTypeList() != null)
                {
                    foreach (EsperEPL2GrammarParser.GopOutTypeParamContext pctx in item.gopOutTypeList().gopOutTypeParam())
                    {
                        var type = ParseType(pctx);
                        types.Add(type);
                    }
                }

                output.Items.Add(new GraphOperatorOutputItem(streamName, types));
            }
        }

        private static GraphOperatorOutputItemType ParseType(EsperEPL2GrammarParser.GopOutTypeParamContext ctx)
        {
            if (ctx.q != null)
            {
                return new GraphOperatorOutputItemType(true, null, null);
            }

            var className = ASTUtil.UnescapeClassIdent(ctx.gopOutTypeItem().classIdentifier());
            IList<GraphOperatorOutputItemType> typeParams = new List<GraphOperatorOutputItemType>();
            if (ctx.gopOutTypeItem().gopOutTypeList() != null)
            {
                IList<EsperEPL2GrammarParser.GopOutTypeParamContext> pctxs = ctx.gopOutTypeItem().gopOutTypeList().gopOutTypeParam();
                foreach (var pctx in pctxs)
                {
                    var type = ParseType(pctx);
                    typeParams.Add(type);
                }
            }

            return new GraphOperatorOutputItemType(false, className, typeParams);
        }
    }
} // end of namespace