///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
	public class CalendarOpUtil {

	    protected static int? GetInt(ExprEvaluator expr, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        object result = expr.Evaluate(eventsPerStream, isNewData, context);
	        if (result == null) {
	            return null;
	        }
	        return (int?) result;
	    }

	    public static CalendarFieldEnum GetEnum(string methodName, ExprNode exprNode) {
	        string message = ValidateConstant(methodName, exprNode);
	        if (message != null) {
	            message += ", " + GetValidFieldNamesMessage();
	            throw new ExprValidationException(message);
	        }
	        string fieldname = (string) exprNode.Forge.ExprEvaluator.Evaluate(null, true, null);
	        CalendarFieldEnum fieldNum = EnumHelper.Parse<CalendarFieldEnum>(fieldname);
	        if (fieldNum == null) {
	            throw new ExprValidationException(GetMessage(methodName) + " datetime-field name '" + fieldname + "' is not recognized, " + GetValidFieldNamesMessage());
	        }
	        return fieldNum;
	    }

	    public static ReformatFormatForgeDesc ValidateGetFormatterType(EPType inputType, string methodName, ExprNode exprNode) {
	        if (!(inputType is ClassEPType)) {
	            throw new ExprValidationException(GetMessage(methodName) + " requires a datetime input value but received " + inputType);
	        }

	        if (!exprNode.Forge.ForgeConstantType.IsConstant) {
	            throw new ExprValidationException(GetMessage(methodName) + " requires a constant-value format");
	        }

	        ExprForge formatForge = exprNode.Forge;
	        Type formatType = formatForge.EvaluationType;
	        if (formatType == null) {
	            throw new ExprValidationException(GetMessage(methodName) + " invalid null format object");
	        }

	        object format = null;
	        if (formatForge.ForgeConstantType.IsCompileTimeConstant) {
	            format = ExprNodeUtilityEvaluate.EvaluateValidationTimeNoStreams(exprNode.Forge.ExprEvaluator, null, "date format");
	            if (format == null) {
	                throw new ExprValidationException(GetMessage(methodName) + " invalid null format object");
	            }
	        }

	        // handle legacy date
	        ClassEPType input = (ClassEPType) inputType;
	        if (Boxing.GetBoxedType(input.Clazz) == typeof(long?)
	            || TypeHelper.IsSubclassOrImplementsInterface(input.Clazz, typeof(DateTime))
	            || TypeHelper.IsSubclassOrImplementsInterface(input.Clazz, typeof(DateTimeOffset))
	            || TypeHelper.IsSubclassOrImplementsInterface(input.Clazz, typeof(DateTimeEx))) {

	            if (TypeHelper.IsSubclassOrImplementsInterface(formatType, typeof(DateFormat))) {
	                return new ReformatFormatForgeDesc(false, typeof(DateFormat));
	            }
	            if (TypeHelper.IsSubclassOrImplementsInterface(formatType, typeof(string))) {
	                if (format != null) {
	                    try {
	                        new SimpleDateFormat((string) format);
	                    } catch (Exception ex) {
	                        throw new ExprValidationException(GetMessage(methodName) + " invalid format string (SimpleDateFormat): " + ex.Message, ex);
	                    }
	                }
	                return new ReformatFormatForgeDesc(false, typeof(string));
	            }
	            throw GetFailedExpected(methodName, typeof(DateFormat), formatType);
	        }

	        // handle jdk8 date
	        if (TypeHelper.IsSubclassOrImplementsInterface(formatType, typeof(DateTimeFormat))) {
	            return new ReformatFormatForgeDesc(true, typeof(DateTimeFormat));
	        }
	        if (TypeHelper.IsSubclassOrImplementsInterface(formatType, typeof(string))) {
	            if (format != null) {
	                try {
	                    DateTimeFormat.For((string) format);
	                } catch (Exception ex) {
	                    throw new ExprValidationException(GetMessage(methodName) + " invalid format string (DateTimeFormatter): " + ex.Message, ex);
	                }
	            }
	            return new ReformatFormatForgeDesc(true, typeof(string));
	        }
	        throw GetFailedExpected(methodName, typeof(DateTimeFormat), formatType);
	    }

	    private static ExprValidationException GetFailedExpected(string methodName, Type expected, Type received) {
	        return new ExprValidationException(GetMessage(methodName) + " invalid format, expected string-format or " + expected.Name + " but received " + received.GetCleanName());
	    }

	    private static string ValidateConstant(string methodName, ExprNode exprNode) {
	        if (ExprNodeUtilityQuery.IsConstant(exprNode)) {
	            return null;
	        }
	        return GetMessage(methodName) + " requires a constant string-type parameter as its first parameter";
	    }

	    private static string GetMessage(string methodName) {
	        return "Date-time enumeration method '" + methodName + "'";
	    }

	    private static string GetValidFieldNamesMessage() {
	        return "valid field names are '" + CalendarFieldEnumExtensions.ValidList + "'";
	    }
	}
} // end of namespace