///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Walker to annotation stuctures.
    /// </summary>
    public class ASTAnnotationHelper
    {
        /// <summary>
        /// Walk an annotation root name or child node (nested annotations).
        /// </summary>
        /// <param name="ctx">annotation walk node</param>
        /// <param name="engineImportService">for engine imports</param>
        /// <returns>annotation descriptor</returns>
        /// <throws>ASTWalkException if the walk failed</throws>
        public static AnnotationDesc Walk(EsperEPL2GrammarParser.AnnotationEnumContext ctx, EngineImportService engineImportService) 
        {
            var name = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            IList<Pair<String, Object>> values = new List<Pair<String, Object>>();
            if (ctx.elementValueEnum() != null) {
                var value = WalkValue(ctx.elementValueEnum(), engineImportService);
                values.Add(new Pair<String, Object>("Value", value));
            }
            else if (ctx.elementValuePairsEnum() != null) {
                WalkValuePairs(ctx.elementValuePairsEnum(), values, engineImportService);
            }

            return new AnnotationDesc(name, values);
        }
    
        private static void WalkValuePairs(EsperEPL2GrammarParser.ElementValuePairsEnumContext elementValuePairsEnumContext,
                                           IList<Pair<String, Object>> values,
                                           EngineImportService engineImportService)
        {
            foreach (var ctx in elementValuePairsEnumContext.elementValuePairEnum()) {
                var pair = WalkValuePair(ctx, engineImportService);
                values.Add(pair);
            }
        }
    
        private static Object WalkValue(EsperEPL2GrammarParser.ElementValueEnumContext ctx, EngineImportService engineImportService)
        {
            if (ctx.elementValueArrayEnum() != null) {
                return WalkArray(ctx.elementValueArrayEnum(), engineImportService);
            }
            if (ctx.annotationEnum() != null) {
                return Walk(ctx.annotationEnum(), engineImportService);
            }
            else if (ctx.v != null) {
                return ctx.v.Text;
            }
            else if (ctx.classIdentifier() != null) {
                return WalkClassIdent(ctx.classIdentifier(), engineImportService);
            }
            else {
                return ASTConstantHelper.Parse(ctx.constant());
            }
        }
    
        private static Pair<String, Object> WalkValuePair(EsperEPL2GrammarParser.ElementValuePairEnumContext ctx, EngineImportService engineImportService)
        {
            var name = ctx.i.Text;
            var value = WalkValue(ctx.elementValueEnum(), engineImportService);
            return new Pair<String, Object>(name, value);
        }
    
        private static Object WalkClassIdent(EsperEPL2GrammarParser.ClassIdentifierContext ctx, EngineImportService engineImportService)
        {
            var enumValueText = ctx.GetText();
            Object enumValue;
            try {
                enumValue = TypeHelper.ResolveIdentAsEnumConst(enumValueText, null, engineImportService, true);
            }
            catch (ExprValidationException) {
                throw ASTWalkException.From("Annotation value '" + enumValueText + "' is not recognized as an enumeration value, please check imports or use a primitive or string type");
            }
            if (enumValue != null) {
                return enumValue;
            }
            throw ASTWalkException.From("Annotation enumeration value '" + enumValueText + "' not recognized as an enumeration class, please check imports or type used");
        }
    
        private static Object[] WalkArray(EsperEPL2GrammarParser.ElementValueArrayEnumContext ctx, EngineImportService engineImportService)
        {
            IList<EsperEPL2GrammarParser.ElementValueEnumContext> elements = ctx.elementValueEnum();
            var values = new Object[elements.Count];
            for (var i = 0; i < elements.Count; i++) {
                values[i] = WalkValue(elements[i], engineImportService);
            }
            return values;
        }
    }
}
