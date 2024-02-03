///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    ///     Walker to annotation stuctures.
    /// </summary>
    public class ASTAnnotationHelper
    {
        /// <summary>
        ///     Walk an annotation root name or child node (nested annotations).
        /// </summary>
        /// <param name="ctx">annotation walk node</param>
        /// <param name="importService">for imports</param>
        /// <returns>annotation descriptor</returns>
        /// <throws>ASTWalkException if the walk failed</throws>
        public static AnnotationDesc Walk(
            EsperEPL2GrammarParser.AnnotationEnumContext ctx,
            ImportServiceCompileTime importService)
        {
            var name = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            IList<Pair<string, object>> values = new List<Pair<string, object>>();
            if (ctx.elementValueEnum() != null)
            {
                var value = WalkValue(ctx.elementValueEnum(), importService);
                values.Add(new Pair<string, object>("Value", value));
            }
            else if (ctx.elementValuePairsEnum() != null)
            {
                WalkValuePairs(ctx.elementValuePairsEnum(), values, importService);
            }

            return new AnnotationDesc(name, values);
        }

        private static void WalkValuePairs(
            EsperEPL2GrammarParser.ElementValuePairsEnumContext elementValuePairsEnumContext,
            IList<Pair<string, object>> values,
            ImportServiceCompileTime importService)
        {
            foreach (var ctx in elementValuePairsEnumContext.elementValuePairEnum())
            {
                var pair = WalkValuePair(ctx, importService);
                values.Add(pair);
            }
        }

        private static object WalkValue(
            EsperEPL2GrammarParser.ElementValueEnumContext ctx,
            ImportServiceCompileTime importService)
        {
            if (ctx.elementValueArrayEnum() != null)
            {
                return WalkArray(ctx.elementValueArrayEnum(), importService);
            }

            if (ctx.annotationEnum() != null)
            {
                return Walk(ctx.annotationEnum(), importService);
            }

            if (ctx.v != null)
            {
                return ctx.v.Text;
            }

            if (ctx.classIdentifier() != null)
            {
                return WalkClassIdent(ctx.classIdentifier(), importService);
            }

            return ASTConstantHelper.Parse(ctx.constant());
        }

        private static Pair<string, object> WalkValuePair(
            EsperEPL2GrammarParser.ElementValuePairEnumContext ctx,
            ImportServiceCompileTime importService)
        {
            var name = ctx.keywordAllowedIdent().GetText();
            var value = WalkValue(ctx.elementValueEnum(), importService);
            return new Pair<string, object>(name, value);
        }

        private static object WalkClassIdent(
            EsperEPL2GrammarParser.ClassIdentifierContext ctx,
            ImportServiceCompileTime importService)
        {
            var enumValueText = ctx.GetText();
            ValueAndFieldDesc enumValueAndField;
            try
            {
                enumValueAndField = ImportCompileTimeUtil.ResolveIdentAsEnumConst(enumValueText, importService, ExtensionClassEmpty.INSTANCE, true);
            }
            catch (ExprValidationException)
            {
                throw ASTWalkException.From(
                    "Annotation value '" + enumValueText +
                    "' is not recognized as an enumeration value, please check imports or use a primitive or string type");
            }

            if (enumValueAndField != null)
            {
                return enumValueAndField.Value;
            }

            // resolve as class
            object enumValue = null;
            if (enumValueText.EndsWith(".class") && enumValueText.Length > 6)
            {
                try
                {
                    var name = enumValueText.Substring(0, enumValueText.Length - 6);
                    enumValue = importService.ResolveType(name, true, ExtensionClassEmpty.INSTANCE);
                }
                catch (ImportException)
                {
                    // expected
                }
            }

            if (enumValue != null)
            {
                return enumValue;
            }

            throw ASTWalkException.From(
                "Annotation enumeration value '" + enumValueText + "' not recognized as an enumeration class, please check imports or type used");
        }

        private static object[] WalkArray(
            EsperEPL2GrammarParser.ElementValueArrayEnumContext ctx,
            ImportServiceCompileTime importService)
        {
            IList<EsperEPL2GrammarParser.ElementValueEnumContext> elements = ctx.elementValueEnum();
            var values = new object[elements.Count];
            for (var i = 0; i < elements.Count; i++)
            {
                values[i] = WalkValue(elements[i], importService);
            }

            return values;
        }
    }
} // end of namespace