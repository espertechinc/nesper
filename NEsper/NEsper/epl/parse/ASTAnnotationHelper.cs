///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    /// <summary>Walker to annotation stuctures.</summary>
    public class ASTAnnotationHelper {
        /// <summary>
        /// Walk an annotation root name or child node (nested annotations).
        /// </summary>
        /// <param name="ctx">annotation walk node</param>
        /// <param name="engineImportService">for engine imports</param>
        /// <exception cref="ASTWalkException">if the walk failed</exception>
        /// <returns>annotation descriptor</returns>
        public static AnnotationDesc Walk(EsperEPL2GrammarParser.AnnotationEnumContext ctx, EngineImportService engineImportService) {
            string name = ASTUtil.UnescapeClassIdent(typeof(ctx)Identifier());
            var values = new List<Pair<string, Object>>();
            if (ctx.ElementValueEnum() != null) {
                Object value = WalkValue(ctx.ElementValueEnum(), engineImportService);
                values.Add(new Pair<string, Object>("value", value));
            } else if (ctx.ElementValuePairsEnum() != null) {
                WalkValuePairs(ctx.ElementValuePairsEnum(), values, engineImportService);
            }
    
            return new AnnotationDesc(name, values);
        }
    
        private static void WalkValuePairs(EsperEPL2GrammarParser.ElementValuePairsEnumContext elementValuePairsEnumContext,
                                           List<Pair<string, Object>> values,
                                           EngineImportService engineImportService) {
    
            foreach (EsperEPL2GrammarParser.ElementValuePairEnumContext ctx in elementValuePairsEnumContext.ElementValuePairEnum()) {
                Pair<string, Object> pair = WalkValuePair(ctx, engineImportService);
                values.Add(pair);
            }
        }
    
        private static Object WalkValue(EsperEPL2GrammarParser.ElementValueEnumContext ctx, EngineImportService engineImportService) {
            if (ctx.ElementValueArrayEnum() != null) {
                return WalkArray(ctx.ElementValueArrayEnum(), engineImportService);
            }
            if (ctx.AnnotationEnum() != null) {
                return Walk(ctx.AnnotationEnum(), engineImportService);
            } else if (ctx.v != null) {
                return Ctx.v.Text;
            } else if (typeof(ctx)Identifier() != null) {
                return WalkClassIdent(typeof(ctx)Identifier(), engineImportService);
            } else {
                return ASTConstantHelper.Parse(ctx.Constant());
            }
        }
    
        private static Pair<string, Object> WalkValuePair(EsperEPL2GrammarParser.ElementValuePairEnumContext ctx, EngineImportService engineImportService) {
            string name = ctx.KeywordAllowedIdent().Text;
            Object value = WalkValue(ctx.ElementValueEnum(), engineImportService);
            return new Pair<string, Object>(name, value);
        }
    
        private static Object WalkClassIdent(EsperEPL2GrammarParser.ClassIdentifierContext ctx, EngineImportService engineImportService) {
            string enumValueText = ctx.Text;
            Object enumValue;
            try {
                enumValue = TypeHelper.ResolveIdentAsEnumConst(enumValueText, engineImportService, true);
            } catch (ExprValidationException e) {
                throw ASTWalkException.From("Annotation value '" + enumValueText + "' is not recognized as an enumeration value, please check imports or use a primitive or string type");
            }
            if (enumValue != null) {
                return enumValue;
            }
            throw ASTWalkException.From("Annotation enumeration value '" + enumValueText + "' not recognized as an enumeration class, please check imports or type used");
        }
    
        private static Object[] WalkArray(EsperEPL2GrammarParser.ElementValueArrayEnumContext ctx, EngineImportService engineImportService) {
            List<EsperEPL2GrammarParser.ElementValueEnumContext> elements = ctx.ElementValueEnum();
            var values = new Object[elements.Count];
            for (int i = 0; i < elements.Count; i++) {
                values[i] = WalkValue(elements.Get(i), engineImportService);
            }
            return values;
        }
    }
} // end of namespace
