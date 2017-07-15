///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.funcs.cast;
using com.espertech.esper.pattern.observer;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the CAST(expression, type) function is an expression tree.
    /// </summary>
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

        public static EPException HandleParseException(string formatString, string date, Exception ex)
        {
            return new EPException(
                "Exception parsing date '" + date + "' format '" + formatString + "': " + ex.Message, ex);
        }

        public static EPException HandleParseISOException(string date, ScheduleParameterException ex)
        {
            return new EPException("Exception parsing iso8601 date '" + date + "': " + ex.Message, ex);
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
            if (ChildNodes.Length == 0 || ChildNodes.Length > 2)
            {
                throw new ExprValidationException("Cast function node must have one or two child expressions");
            }

            ExprEvaluator valueEvaluator = ChildNodes[0].ExprEvaluator;
            Type fromType = valueEvaluator.ReturnType;

            // determine date format parameter
            IDictionary<string, ExprNamedParameterNode> namedParams =
                ExprNodeUtility.GetNamedExpressionsHandleDups(ChildNodes);
            ExprNodeUtility.ValidateNamed(namedParams, new string[] { "dateformat" });
            ExprNamedParameterNode dateFormatParameter = namedParams.Get("dateformat");
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
                        _targetType = TypeHelper.GetTypeForName(
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

                if (_targetType == typeof (DateTime) ||
                    classIdentifierInvariant.Equals("date") ||
                    classIdentifierInvariant.Equals("datetime"))
                {
                    _targetType = typeof (DateTime);
                    
                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            casterParserComputer = new StringToDateTimeWStaticISOFormatComputer();
                        }
                        else
                        {
                            casterParserComputer = new StringToDateTimeWStaticFormatComputer(desc.StaticDateFormat);
                        }
                    }
                    else
                    {
                        casterParserComputer = new StringToDateTimeWDynamicFormatComputer(desc.DynamicDateFormat);
                    }
                }
                else if (_targetType == typeof (DateTimeOffset) ||
                         classIdentifierInvariant.Equals("datetimeoffset"))
                {
                    _targetType = typeof(DateTimeOffset);

                    var desc = ValidateDateFormat(dateFormatParameter, validationContext);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            casterParserComputer = new StringToDateTimeOffsetWStaticISOFormatComputer(validationContext.EngineImportService.TimeZone);
                        }
                        else
                        {
                            casterParserComputer = new StringToDateTimeOffsetWStaticFormatComputer(desc.StaticDateFormat, validationContext.EngineImportService.TimeZone);
                        }
                    }
                    else
                    {
                        casterParserComputer = new StringToDateTimeOffsetWDynamicFormatComputer(desc.DynamicDateFormat, validationContext.EngineImportService.TimeZone);
                    }
                }
                else if (_targetType == typeof (DateTimeEx) ||
                         classIdentifierInvariant.Equals("datetimeex") ||
                         classIdentifierInvariant.Equals("calendar"))
                {
                    _targetType = typeof (DateTimeEx);
                    ExprCastNodeDateDesc desc = ValidateDateFormat(dateFormatParameter, validationContext, false);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            casterParserComputer = new StringToCalendarWStaticISOFormatComputer();
                        }
                        else
                        {
                            casterParserComputer =
                                new StringToCalendarWStaticFormatComputer(
                                    desc.StaticDateFormat, validationContext.EngineImportService.TimeZone);
                        }
                    }
                    else
                    {
                        casterParserComputer = new StringToCalendarWDynamicFormatComputer(
                            desc.DynamicDateFormat, validationContext.EngineImportService.TimeZone);
                    }
                }
                else if (_targetType == typeof (long))
                {
                    _targetType = typeof (long);
                    ExprCastNodeDateDesc desc = ValidateDateFormat(dateFormatParameter, validationContext, false);
                    if (desc.StaticDateFormat != null)
                    {
                        if (desc.Iso8601Format)
                        {
                            casterParserComputer = new StringToLongWStaticISOFormatComputer();
                        }
                        else
                        {
                            casterParserComputer = new StringToLongWStaticFormatComputer(desc.StaticDateFormat);
                        }
                    }
                    else
                    {
                        casterParserComputer = new StringToLongWDynamicFormatComputer(desc.DynamicDateFormat);
                    }
                }
                else
                {
                    throw new ExprValidationException(
                        "Use of the '" + dateFormatParameter.ParameterName +
                        "' named parameter requires a target type of calendar, date, long, localdatetime, localdate, localtime or zoneddatetime");
                }
            }
            else if (_targetType != null)
            {
                _targetType = TypeHelper.GetBoxedType(_targetType);
                caster = SimpleTypeCasterFactory.GetCaster(fromType, _targetType);
                numeric = caster.IsNumericCast;
            }
            else if (classIdentifierInvariant.Equals("bigint".ToLowerInvariant()))
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
                    _targetType = TypeHelper.GetTypeForName(
                        _classIdentifier.Trim(), validationContext.EngineImportService.ClassForNameProvider);
                }
                catch (TypeLoadException e)
                {
                    throw new ExprValidationException(
                        "Type as listed in cast function by name '" + _classIdentifier + "' cannot be loaded", e);
                }
                numeric = TypeHelper.IsNumeric(_targetType);
                if (numeric)
                {
                    caster = SimpleTypeCasterFactory.GetCaster(fromType, _targetType);
                }
                else
                {
                    caster = new SimpleTypeCasterAnyType(_targetType);
                }
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
                    SimpleTypeParser parser = SimpleTypeParserFactory.GetParser(_targetType.GetBoxedType());
                    casterParserComputer = new StringParserComputer(parser);
                }
                else if (numeric)
                {
                    // numeric cast with check
                    casterParserComputer = new NumberCasterComputer(caster);
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
                    var @in = valueEvaluator.Evaluate(new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
                    theConstant = @in == null
                        ? null
                        : casterParserComputer.Compute(@in, null, true, validationContext.ExprEvaluatorContext);
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
            writer.Write("Cast(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(",");
            writer.Write(_classIdentifier);
            for (int i = 1; i < ChildNodes.Length; i++) {
                writer.Write(",");
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node) {
            if (!(node is ExprCastNode)) {
                return false;
            }
            ExprCastNode other = (ExprCastNode) node;
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
            bool iso8601Format = false;

            if (!dateFormatParameter.ChildNodes[0].IsConstantResult)
            {
                dynamicDateFormat = dateFormatParameter.ChildNodes[0].ExprEvaluator;
            }
            else
            {
                staticDateFormat = (string) dateFormatParameter.ChildNodes[0].ExprEvaluator.Evaluate(
                    null, true, validationContext.ExprEvaluatorContext);
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

        public class StringToLongWStaticFormatComputer : StringToDateLongWStaticFormat
        {
            public StringToLongWStaticFormatComputer(string dateFormat)
                : base(dateFormat)
            {
            }
    
            internal static Object ParseSafe(string dateFormat, Object input) {
                try {
                    return dateFormat.Parse(input.ToString()).Time;
                } catch (Exception e) {
                    throw HandleParseException(dateFormat, input.ToString(), e);
                }
            }
    
            public override Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
                return ParseSafe(base.DateFormat, input);
            }
        }
    
        public class StringToLongWStaticISOFormatComputer : CasterParserComputer
        {
            public Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
                try {
                    return TimerScheduleISO8601Parser.ParseDate(input.ToString()).TimeInMillis;
                } catch (ScheduleParameterException ex) {
                    throw HandleParseISOException(input.ToString(), ex);
                }
            }

            public bool IsConstantForConstInput
            {
                get { return true; }
            }
        }
    
        public class StringToCalendarWStaticFormatComputer : StringToDateLongWStaticFormat
        {
            private readonly TimeZoneInfo _timeZone;

            public StringToCalendarWStaticFormatComputer(string dateFormat, TimeZoneInfo timeZone)
                : base(dateFormat)
            {
                _timeZone = timeZone;
            }

            internal static Object Parse(string formatString, Object input, TimeZoneInfo timeZone)
            {
                try {
                    DateTimeEx cal = DateTimeEx.GetInstance(timeZone);
                    DateTimeOffset date = format.Parse(input.ToString());
                    cal.Set(date);
                    return cal;
                } catch (Exception ex) {
                    throw HandleParseException(formatString, input.ToString(), ex);
                }
            }
    
            public override Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
                return Parse(base.DateFormat, input, _timeZone);
            }
        }
    
        public class StringToCalendarWStaticISOFormatComputer : CasterParserComputer
        {
            public Object Compute(Object input, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
                try {
                    return TimerScheduleISO8601Parser.ParseDate(input.ToString());
                } catch (ScheduleParameterException ex) {
                    throw HandleParseISOException(input.ToString(), ex);
                }
            }

            public bool IsConstantForConstInput
            {
                get { return true; }
            }
        }
    
    }
} // end of namespace
