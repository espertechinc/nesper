///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.IO;
using System.Numerics;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs.cast;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the CAST(expression, type) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCastNode : ExprNodeBase
    {
        private readonly string _classIdentifier;
        private Type _targetType;
        private bool _isConstant;
        [NonSerialized] private ExprEvaluator _exprEvaluator;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="classIdentifier">the the name of the type to cast to</param>
        public ExprCastNode(string classIdentifier)
        {
            _classIdentifier = classIdentifier;
        }

        public static void HandleParseException(string formatString, string date, Exception ex)
        {
            throw new EPException(
                "Exception parsing date '" + date + "' format '" + formatString + "': " + ex.Message, ex);
        }

        public static void HandleParseISOException(string formatString, string date, Exception ex)
        {
            if (ex is ScheduleParameterException)
            {
                throw new EPException("Exception parsing iso8601 date '" + date + "': " + ex.Message, ex);
            }
            else
            {
                HandleParseException(formatString, date, ex);
            }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return _exprEvaluator; }
        }

        /// <summary>
        /// Returns the name of the type of cast to.
        /// </summary>
        /// <value>type name</value>
        public string ClassIdentifier
        {
            get { return _classIdentifier; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count == 0 || ChildNodes.Count > 2)
            {
                throw new ExprValidationException("Cast function node must have one or two child expressions");
            }

            var valueEvaluator = ChildNodes[0].ExprEvaluator;
            var fromType = valueEvaluator.ReturnType;

            // determine date format parameter
            var namedParams =
                ExprNodeUtility.GetNamedExpressionsHandleDups(ChildNodes);
            ExprNodeUtility.ValidateNamed(namedParams, new string[] { "dateformat" });
            var dateFormatParameter = namedParams.Get("dateformat");
            if (dateFormatParameter != null)
            {
                ExprNodeUtility.ValidateNamedExpectType(
                    dateFormatParameter, new Type[]{ typeof (string) });
            }

            // identify target type
            // try the primitive names including "string"
            _targetType = TypeHelper.GetPrimitiveTypeForName(_classIdentifier.Trim()).GetBoxedType();

            SimpleTypeCaster caster;
            bool numeric;
            CasterParserComputer casterParserComputer = null;

            var classIdentifierInvariant = _classIdentifier.Trim().ToLowerInvariant();
            if (dateFormatParameter != null)
            {
                if (fromType != typeof (string))
                {
                    throw new ExprValidationException(
                        string.Format("Use of the '{0}' named parameter requires a string-type input", dateFormatParameter.ParameterName));
                }

                if (_targetType == null)
                {
                    try
                    {
                        _targetType = TypeHelper.GetClassForName(
                            _classIdentifier.Trim(), validationContext.EngineImportService.GetClassForNameProvider());
                    }
                    catch (TypeLoadException)
                    {
                        // expected
                    }
                }

                // dynamic or static date format
                numeric = false;
                caster = null;


                StringToDateTimeBaseComputer dateTimeComputer;

                if (_targetType == typeof (DateTime) ||
                    _targetType == typeof (DateTime?) ||
                    classIdentifierInvariant.Equals("date"))
                {
                    _targetType = typeof (DateTime);
                    
                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            dateTimeComputer = new StringToDateTimeWStaticISOFormatComputer(validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseISOException;
                        }
                        else
                        {
                            dateTimeComputer = new StringToDateTimeWStaticFormatComputer(desc.StaticDateFormat);
                            dateTimeComputer.HandleParseException += HandleParseException;
                        }
                    }
                    else
                    {
                        dateTimeComputer = new StringToDateTimeWDynamicFormatComputer(desc.DynamicDateFormat);
                        dateTimeComputer.HandleParseException += HandleParseException;
                    }
                }
                else if (_targetType == typeof (DateTimeOffset) ||
                         _targetType == typeof (DateTimeOffset?) ||
                         classIdentifierInvariant.Equals("dto") ||
                         classIdentifierInvariant.Equals("datetimeoffset"))
                {
                    _targetType = typeof(DateTimeOffset);

                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            dateTimeComputer = new StringToDtoWStaticISOFormatComputer(validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseISOException;
                        }
                        else
                        {
                            dateTimeComputer = new StringToDtoWStaticFormatComputer(desc.StaticDateFormat, validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseException;
                        }
                    }
                    else
                    {
                        dateTimeComputer = new StringToDtoWDynamicFormatComputer(desc.DynamicDateFormat, validationContext.EngineImportService.TimeZone);
                        dateTimeComputer.HandleParseException += HandleParseException;
                    }
                }
                else if (_targetType == typeof (DateTimeEx) ||
                         classIdentifierInvariant.Equals("dtx") ||
                         classIdentifierInvariant.Equals("datetimeex") ||
                         classIdentifierInvariant.Equals("calendar"))
                {
                    _targetType = typeof(DateTimeEx);

                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            dateTimeComputer = new StringToDtxWStaticISOFormatComputer(validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseISOException;
                        }
                        else
                        {
                            dateTimeComputer = new StringToDtxWStaticFormatComputer(desc.StaticDateFormat, validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseException;
                        }
                    }
                    else
                    {
                        dateTimeComputer = new StringToDtxWDynamicFormatComputer(desc.DynamicDateFormat, validationContext.EngineImportService.TimeZone);
                        dateTimeComputer.HandleParseException += HandleParseException;
                    }
                }
                else if (_targetType == typeof (long) || _targetType == typeof(long?))
                {
                    _targetType = typeof (long);

                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            dateTimeComputer = new StringToDateTimeLongWStaticISOFormatComputer(validationContext.EngineImportService.TimeZone);
                            dateTimeComputer.HandleParseException += HandleParseISOException;
                        }
                        else
                        {
                            dateTimeComputer = new StringToDateTimeLongWStaticFormatComputer(desc.StaticDateFormat);
                            dateTimeComputer.HandleParseException += HandleParseException;
                        }
                    }
                    else
                    {
                        dateTimeComputer = new StringToDateTimeLongWDynamicFormatComputer(desc.DynamicDateFormat);
                        dateTimeComputer.HandleParseException += HandleParseException;
                    }
                }
                else
                {
                    throw new ExprValidationException(
                        "Use of the '" + dateFormatParameter.ParameterName +
                        "' named parameter requires a target type of long or datetime");
                }

                casterParserComputer = dateTimeComputer;
            }
            else if (_targetType != null)
            {
                _targetType = _targetType.GetBoxedType();
                caster = SimpleTypeCasterFactory.GetCaster(fromType, _targetType);
                numeric = _targetType.IsNumeric();
            }
            else if (String.Equals(classIdentifierInvariant, "bigint", StringComparison.InvariantCultureIgnoreCase) ||
                     String.Equals(classIdentifierInvariant, "biginteger", StringComparison.InvariantCultureIgnoreCase))
            {
                _targetType = typeof (BigInteger);
                caster = SimpleTypeCasterFactory.GetCaster(fromType, _targetType);
                numeric = true;
            }
            else if (classIdentifierInvariant.Equals("decimal".ToLowerInvariant()))
            {
                _targetType = typeof (decimal);
                caster = SimpleTypeCasterFactory.GetCaster(fromType, _targetType);
                numeric = true;
            }
            else
            {
                try
                {
                    _targetType = TypeHelper.GetClassForName(
                        _classIdentifier.Trim(), validationContext.EngineImportService.GetClassForNameProvider());
                }
                catch (TypeLoadException e)
                {
                    throw new ExprValidationException(
                        "Type as listed in cast function by name '" + _classIdentifier + "' cannot be loaded", e);
                }
                numeric = _targetType.IsNumeric();
                caster = numeric
                    ? SimpleTypeCasterFactory.GetCaster(fromType, _targetType)
                    : SimpleTypeCasterFactory.GetCaster(_targetType);
            }

            // assign a computer unless already assigned
            if (casterParserComputer == null)
            {
                // to-string
                if (_targetType == typeof (string))
                {
                    casterParserComputer = new StringXFormComputer();
                }
                else if (fromType == typeof (string))
                {
                    // parse
                    var parser = SimpleTypeParserFactory.GetParser(_targetType.GetBoxedType());
                    casterParserComputer = new StringParserComputer(parser);
                }
                else if (numeric)
                {
                    // numeric cast with check
                    casterParserComputer = new NumericCasterComputer(caster);
                }
                else
                {
                    // non-numeric cast
                    casterParserComputer = new NonnumericCasterComputer(caster);
                }
            }

            // determine constant or not
            Object theConstant = null;
            if (ChildNodes[0].IsConstantResult)
            {
                _isConstant = casterParserComputer.IsConstantForConstInput;
                if (_isConstant)
                {
                    var evaluateParams = new EvaluateParams(null, true, validationContext.ExprEvaluatorContext);
                    var @in = valueEvaluator.Evaluate(evaluateParams);
                    theConstant = @in == null ? null : casterParserComputer.Compute(@in, evaluateParams);
                }
            }

            // determine evaluator
            if (_isConstant)
            {
                _exprEvaluator = new ExprCastNodeConstEval(this, theConstant);
            }
            else
            {
                _exprEvaluator = new ExprCastNodeNonConstEval(this, valueEvaluator, casterParserComputer);
            }
            return null;
        }

        public override bool IsConstantResult
        {
            get { return _isConstant; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            writer.Write("cast(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(",");
            writer.Write(_classIdentifier);
            for (var i = 1; i < ChildNodes.Count; i++) {
                writer.Write(",");
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
            var other = node as ExprCastNode;
            if (other == null)
            {
                return false;
            }

            return other._classIdentifier.Equals(_classIdentifier);
        }

        /// <summary>
        /// Validates the date format.
        /// </summary>
        /// <param name="dateFormatParameter">The date format parameter.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns></returns>

        private ExprCastNodeDateDesc ValidateDateFormat(
            ExprNamedParameterNode dateFormatParameter,
            ExprValidationContext validationContext)
        {
            string staticDateFormat = null;
            ExprEvaluator dynamicDateFormat = null;
            var iso8601Format = false;

            if (!dateFormatParameter.ChildNodes[0].IsConstantResult)
            {
                dynamicDateFormat = dateFormatParameter.ChildNodes[0].ExprEvaluator;
            }
            else
            {
                staticDateFormat = (string) dateFormatParameter.ChildNodes[0].ExprEvaluator.Evaluate(
                    new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
                if (staticDateFormat.ToLowerInvariant().Trim().Equals("iso"))
                {
                    iso8601Format = true;
                }
                else
                {
                    try
                    {
                        DateTime dateTimeTemp;
                        DateTime.TryParseExact("", staticDateFormat, null, DateTimeStyles.None, out dateTimeTemp);
                        //new SimpleDateFormat(staticDateFormat);
                    }
                    catch (Exception ex)
                    {
                        throw new ExprValidationException(
                            "Invalid date format '" + staticDateFormat + "': " + ex.Message, ex);
                    }
                }
            }
            return new ExprCastNodeDateDesc(staticDateFormat, dynamicDateFormat, iso8601Format);
        }
    }
} // end of namespace
