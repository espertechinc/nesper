///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the CAST(expression, type) function is an expression tree.
    /// </summary>
    public partial class ExprCastNode : ExprNodeBase
    {
        private ExprCastNodeForge forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="classIdentifierWArray">the the name of the type to cast to</param>
        public ExprCastNode(ClassIdentifierWArray classIdentifierWArray)
        {
            ClassIdentifierWArray = classIdentifierWArray;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public ClassIdentifierWArray ClassIdentifierWArray { get; }

        public bool IsConstantResult {
            get {
                CheckValidated(forge);
                return forge.IsConstant;
            }
        }

        public Type TargetType {
            get {
                CheckValidated(forge);
                return forge.EvaluationType;
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length == 0 || ChildNodes.Length > 2) {
                throw new ExprValidationException("Cast function node must have one or two child expressions");
            }

            var fromType = ChildNodes[0].Forge.EvaluationType;
            var classIdentifier = ClassIdentifierWArray.ClassIdentifier;
            var classIdentifierInvariant = classIdentifier.Trim();
            var arrayDimensions = ClassIdentifierWArray.ArrayDimensions;

            // Local function to match a class identifier
            bool MatchesClassIdentifier(string identifier)
            {
                return string.Equals(
                    classIdentifierInvariant,
                    identifier,
                    StringComparison.InvariantCultureIgnoreCase);
            }
            
            // determine date format parameter
            var namedParams =
                ExprNodeUtilityValidate.GetNamedExpressionsHandleDups(ChildNodes);
            ExprNodeUtilityValidate.ValidateNamed(namedParams, new[] {"dateformat"});
            var dateFormatParameter = namedParams.Get("dateformat");
            if (dateFormatParameter != null) {
                ExprNodeUtilityValidate.ValidateNamedExpectType(
                    dateFormatParameter,
                    new[] {
                        typeof(string), 
                        typeof(DateFormat), 
                        typeof(DateTimeFormat)
                    });
            }

            // identify target type
            // try the primitive names including "string"
            SimpleTypeCaster caster;
            var targetType = TypeHelper.GetPrimitiveTypeForName(classIdentifier.Trim());
            if (!ClassIdentifierWArray.IsArrayOfPrimitive) {
                targetType = targetType.GetBoxedType();
            }

            targetType = ApplyDimensions(targetType);

            bool numeric;
            CasterParserComputerForge casterParserComputerForge = null;
            if (dateFormatParameter != null) {
                if (fromType != typeof(string)) {
                    throw new ExprValidationException(
                        "Use of the '" +
                        dateFormatParameter.ParameterName +
                        "' named parameter requires a string-type input");
                }

                if (targetType == null) {
                    try {
                        targetType = TypeHelper.GetClassForName(
                            classIdentifier.Trim(),
                            validationContext.ImportService.ClassForNameProvider);
                        targetType = ApplyDimensions(targetType);
                    }
                    catch (TypeLoadException) {
                        // expected
                    }
                }

                // dynamic or static date format
                numeric = false;
                caster = null;

                if (targetType == typeof(DateTimeEx) ||
                    MatchesClassIdentifier("calendar") ||
                    MatchesClassIdentifier("dateTimeEx")) {
                    targetType = typeof(DateTimeEx);

                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.IsIso8601Format) {
                        casterParserComputerForge = new StringToDateTimeExIsoFormatComputer();
                    }
                    else if (desc.StaticDateFormat != null) {
                        casterParserComputerForge = new StringToDateTimExWStaticFormatComputer(
                            desc.StaticDateFormat,
                            TimeZoneInfo.Utc); // Note how code-generation does not use the default time zone
                    }
                    else {
                        casterParserComputerForge = new StringToDateTimeExWExprFormatComputer(
                            desc.DynamicDateFormat,
                            TimeZoneInfo.Utc);
                    }
                }
                else if (targetType == typeof(DateTimeOffset) ||
                         targetType == typeof(DateTimeOffset?) ||
                         MatchesClassIdentifier("dto") ||
                         MatchesClassIdentifier("datetimeoffset")) {
                    targetType = typeof(DateTimeOffset);
                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.IsIso8601Format) {
                        casterParserComputerForge = new StringToDateTimeOffsetIsoFormatComputer();
                    }
                    else if (desc.StaticDateFormat != null) {
                        casterParserComputerForge =
                            new StringToDateTimeOffsetWStaticFormatComputer(desc.StaticDateFormat);
                    }
                    else {
                        casterParserComputerForge =
                            new StringToDateTimeOffsetWExprFormatComputerForge(desc.DynamicDateFormat);
                    }
                }
                else if (targetType == typeof(DateTime) ||
                         targetType == typeof(DateTime?) ||
                         MatchesClassIdentifier("datetime")) {
                    targetType = typeof(DateTime);
                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.IsIso8601Format) {
                        casterParserComputerForge = new StringToDateTimeIsoFormatComputer();
                    }
                    else if (desc.StaticDateFormat != null) {
                        casterParserComputerForge =
                            new StringToDateTimeWStaticFormatComputer(desc.StaticDateFormat);
                    }
                    else {
                        casterParserComputerForge =
                            new StringToDateTimeWExprFormatComputerForge(desc.DynamicDateFormat);
                    }
                }
                else if (targetType == typeof(long) || targetType == typeof(long?)) {
                    targetType = typeof(long);
                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.IsIso8601Format) {
                        casterParserComputerForge = new StringToLongWStaticISOFormatComputer();
                    }
                    else if (desc.StaticDateFormat != null) {
                        casterParserComputerForge = new StringToLongWStaticFormatComputer(desc.StaticDateFormat);
                    }
                    else {
                        casterParserComputerForge = new StringToLongWExprFormatComputerForge(desc.DynamicDateFormat);
                    }
                }
                else {
                    throw new ExprValidationException(
                        "Use of the '" +
                        dateFormatParameter.ParameterName +
                        "' named parameter requires a target type of long, DateTime, DateTimeOffset or DateEx");
                }
            }
            else if (targetType != null) {
                targetType = targetType.GetBoxedType();
                caster = SimpleTypeCasterFactory.GetCaster(fromType, targetType);
                numeric = caster.IsNumericCast;
            }
            else if (MatchesClassIdentifier("bigint") || MatchesClassIdentifier("biginteger")) {
                targetType = typeof(BigInteger);
                targetType = ApplyDimensions(targetType);
                caster = SimpleTypeCasterFactory.GetCaster(fromType, targetType);
                numeric = true;
            }
            else if (MatchesClassIdentifier("decimal")) {
                targetType = typeof(decimal);
                targetType = ApplyDimensions(targetType);
                caster = SimpleTypeCasterFactory.GetCaster(fromType, targetType);
                numeric = true;
            }
            else {
                try {
                    targetType = TypeHelper.GetClassForName(
                        classIdentifier.Trim(),
                        validationContext.ImportService.ClassForNameProvider);
                }
                catch (TypeLoadException e) {
                    throw new ExprValidationException(
                        "Class as listed in cast function by name '" + classIdentifier + "' cannot be loaded",
                        e);
                }

                targetType = ApplyDimensions(targetType);
                numeric = targetType.IsNumeric();
                if (numeric) {
                    caster = SimpleTypeCasterFactory.GetCaster(fromType, targetType);
                }
                else {
                    caster = new SimpleTypeCasterAnyType(targetType);
                }
            }

            // assign a computer unless already assigned
            if (casterParserComputerForge == null) {
                // to-string
                if (targetType == typeof(string)) {
                    casterParserComputerForge = new StringXFormComputer();
                }
                else if (fromType == typeof(string) && targetType != typeof(char)) {
                    // parse
                    SimpleTypeParserSPI parser = SimpleTypeParserFactory.GetParser(targetType.GetBoxedType());
                    casterParserComputerForge = new StringParserComputer(parser);
                }
                else if (numeric) {
                    // numeric cast with check
                    casterParserComputerForge = new NumberCasterComputer(caster);
                }
                else {
                    // non-numeric cast
                    casterParserComputerForge = new NonnumericCasterComputer(caster);
                }
            }

            // determine constant or not
            object theConstant = null;
            var isConstant = false;
            if (ChildNodes[0].Forge.ForgeConstantType.IsCompileTimeConstant) {
                isConstant = casterParserComputerForge.IsConstantForConstInput;
                if (isConstant) {
                    var @in = ChildNodes[0].Forge.ExprEvaluator.Evaluate(null, true, null);
                    theConstant = @in == null
                        ? null
                        : casterParserComputerForge.EvaluatorComputer.Compute(@in, null, true, null);
                }
            }
            
            forge = new ExprCastNodeForge(this, casterParserComputerForge, targetType, isConstant, theConstant);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("cast(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(",");
            ClassIdentifierWArray.ToEPL(writer);
            for (var i = 1; i < ChildNodes.Length; i++) {
                writer.Write(",");
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }

            writer.Write(')');
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprCastNode)) {
                return false;
            }

            var other = (ExprCastNode) node;
            return other.ClassIdentifierWArray.Equals(ClassIdentifierWArray);
        }

        public static EPException HandleParseException(
            DateFormat format,
            string date,
            Exception ex)
        {
            string pattern;
            if (format is SimpleDateFormat simpleDateFormat) {
                pattern = simpleDateFormat.FormatString;
            }
            else {
                pattern = format.ToString();
            }

            return HandleParseException(pattern, date, ex);
        }

        public static EPException HandleParseException(
            string formatString,
            string date,
            Exception ex)
        {
            return new EPException(
                "Exception parsing date '" + date + "' format '" + formatString + "': " + ex.Message,
                ex);
        }

        public static EPException HandleParseISOException(
            string date,
            ScheduleParameterException ex)
        {
            return new EPException("Exception parsing iso8601 date '" + date + "': " + ex.Message, ex);
        }

        private ExprCastNodeDateDesc ValidateDateFormat(
            ExprNamedParameterNode dateFormatParameter,
            ExprValidationContext validationContext)
        {
            var iso8601Format = false;
            var formatExpr = dateFormatParameter.ChildNodes[0];
            var formatForge = formatExpr.Forge;
            var formatReturnType = formatExpr.Forge.EvaluationType;
            string staticFormatString = null;

            if (formatReturnType == typeof(string)) {
                if (formatExpr.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    staticFormatString = (string) formatForge.ExprEvaluator.Evaluate(null, true, null);
                    if (staticFormatString.ToLowerInvariant().Trim().Equals("iso")) {
                        iso8601Format = true;
                    }
                    else {
                        try {
                            DateTimeFormat.For(staticFormatString);
                        }
                        catch (EPException) {
                            throw;
                        }
                        catch (Exception ex) {
                            throw new ExprValidationException(
                                "Invalid date format '" +
                                staticFormatString +
                                "' (as obtained from DateTimeFormatter.For): " +
                                ex.Message,
                                ex);
                        }
                    }
                }
            }
            else {
                if ((typeof(DateFormat) != formatReturnType) &&
                    (!typeof(DateFormat).IsAssignableFrom(formatReturnType)) &&
                    (typeof(DateTimeFormat) != formatReturnType) &&
                    (!typeof(DateTimeFormat).IsAssignableFrom(formatReturnType))) {
                    throw GetFailedExpected(typeof(DateFormat), formatReturnType);
                }
            }

            return new ExprCastNodeDateDesc(
                iso8601Format,
                formatForge,
                staticFormatString,
                formatForge.ForgeConstantType.IsConstant);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="format">format</param>
        /// <returns>date format</returns>
        public static SimpleDateFormat StringToSimpleDateFormatSafe(object format)
        {
            if (format == null) {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }

            try {
                return new SimpleDateFormat(format.ToString());
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw new EPException("Invalid date format '" + format + "': " + ex.Message, ex);
            }
        }

        public static DateTimeFormat StringToDateTimeFormatterSafe(object format)
        {
            if (format == null) {
                throw new EPException("Null date format returned by 'dateformat' expression");
            }

            try {
                return DateTimeFormat.For(format.ToString());
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                throw new EPException("Invalid date format '" + format + "': " + ex.Message, ex);
            }
        }

        private static CodegenExpression FormatField(
            string dateFormatString,
            CodegenClassScope codegenClassScope)
        {
            return codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(DateFormat),
                NewInstance<SimpleDateFormat>(Constant(dateFormatString)));
        }

        private static CodegenExpression FormatFieldExpr(
            Type type,
            ExprForge formatExpr,
            CodegenClassScope codegenClassScope)
        {
            var formatEval = CodegenLegoMethodExpression.CodegenExpression(
                formatExpr,
                codegenClassScope.NamespaceScope.InitMethod,
                codegenClassScope,
                true);
            CodegenExpression formatInit = LocalMethod(formatEval, ConstantNull(), ConstantTrue(), ConstantNull());
            return codegenClassScope.AddDefaultFieldUnshared(true, type, formatInit);
        }

        private ExprValidationException GetFailedExpected(
            Type expected,
            Type received)
        {
            return new ExprValidationException(
                "Invalid format, expected string-format or " +
                expected.GetSimpleName() +
                " but received " +
                received.CleanName());
        }

        private Type ApplyDimensions(Type targetType)
        {
            if (targetType == null) {
                return null;
            }

            if (ClassIdentifierWArray.ArrayDimensions == 0) {
                return targetType;
            }

            return TypeHelper.GetArrayType(
                targetType,
                ClassIdentifierWArray.ArrayDimensions);
        }
    }
} // end of namespace