///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
	public class ExprNodeUtilityResolve {
	    public static ExprNodeUtilMethodDesc ResolveMethodAllowWildcardAndStream(string className,
	                                                                             Type optionalClass,
	                                                                             string methodName,
	                                                                             IList<ExprNode> parameters,
	                                                                             bool allowWildcard,
	                                                                             EventType wildcardType,
	                                                                             ExprNodeUtilResolveExceptionHandler exceptionHandler,
	                                                                             string functionName,
	                                                                             StatementRawInfo statementRawInfo,
	                                                                             StatementCompileTimeServices services) {
	        Type[] paramTypes = new Type[parameters.Count];
	        ExprForge[] childForges = new ExprForge[parameters.Count];
	        int count = 0;
	        bool[] allowEventBeanType = new bool[parameters.Count];
	        bool[] allowEventBeanCollType = new bool[parameters.Count];
	        ExprForge[] childEvalsEventBeanReturnTypesForges = new ExprForge[parameters.Count];
	        bool allConstants = true;
	        foreach (ExprNode childNode in parameters) {
	            if (!EnumMethodEnum.IsEnumerationMethod(methodName) && childNode is ExprLambdaGoesNode) {
	                throw new ExprValidationException("Unexpected lambda-expression encountered as parameter to UDF or static method '" + methodName + "'");
	            }
	            if (childNode is ExprWildcard) {
	                if (wildcardType == null || !allowWildcard) {
	                    throw new ExprValidationException("Failed to resolve wildcard parameter to a given event type");
	                }
	                childForges[count] = new ExprEvalStreamNumUnd(0, wildcardType.UnderlyingType);
	                childEvalsEventBeanReturnTypesForges[count] = new ExprEvalStreamNumEvent(0);
	                paramTypes[count] = wildcardType.UnderlyingType;
	                allowEventBeanType[count] = true;
	                allConstants = false;
	                count++;
	                continue;
	            }
	            if (childNode is ExprStreamUnderlyingNode) {
	                ExprStreamUnderlyingNode und = (ExprStreamUnderlyingNode) childNode;
	                TableMetaData tableMetadata = services.TableCompileTimeResolver.ResolveTableFromEventType(und.EventType);
	                if (tableMetadata == null) {
	                    childForges[count] = childNode.Forge;
	                    childEvalsEventBeanReturnTypesForges[count] = new ExprEvalStreamNumEvent(und.StreamId);
	                } else {
	                    childForges[count] = new ExprEvalStreamTable(und.StreamId, und.EventType.UnderlyingType, tableMetadata);
	                    childEvalsEventBeanReturnTypesForges[count] = new ExprEvalStreamNumEventTable(und.StreamId, tableMetadata);
	                }
	                paramTypes[count] = childForges[count].EvaluationType;
	                allowEventBeanType[count] = true;
	                allConstants = false;
	                count++;
	                continue;
	            }
	            if (childNode.Forge is ExprEnumerationForge) {
	                ExprEnumerationForge enumeration = (ExprEnumerationForge) childNode.Forge;
	                EventType eventType = enumeration.GetEventTypeSingle(statementRawInfo, services);
	                childForges[count] = childNode.Forge;
	                paramTypes[count] = childForges[count].EvaluationType;
	                allConstants = false;
	                if (eventType != null) {
	                    childEvalsEventBeanReturnTypesForges[count] = new ExprEvalStreamNumEnumSingleForge(enumeration);
	                    allowEventBeanType[count] = true;
	                    count++;
	                    continue;
	                }
	                EventType eventTypeColl = enumeration.GetEventTypeCollection(statementRawInfo, services);
	                if (eventTypeColl != null) {
	                    childEvalsEventBeanReturnTypesForges[count] = new ExprEvalStreamNumEnumCollForge(enumeration);
	                    allowEventBeanCollType[count] = true;
	                    count++;
	                    continue;
	                }
	            }

	            paramTypes[count] = childNode.Forge.EvaluationType;
	            childForges[count] = childNode.Forge;
	            count++;
	            if (!(childNode.Forge.ForgeConstantType.IsCompileTimeConstant)) {
	                allConstants = false;
	            }
	        }

	        // Try to resolve the method
	        Method method;
	        try {
	            if (optionalClass != null) {
	                method = services.ImportServiceCompileTime.ResolveMethod(optionalClass, methodName, paramTypes, allowEventBeanType);
	            } else {
	                method = services.ImportServiceCompileTime.ResolveMethodOverloadChecked(className, methodName, paramTypes, allowEventBeanType, allowEventBeanCollType);
	            }
	        } catch (Exception e) {
	            throw exceptionHandler.Handle(e);
	        }

	        // rewrite those evaluator that should return the event itself
	        if (CollectionUtil.IsAnySet(allowEventBeanType)) {
	            for (int i = 0; i < parameters.Count; i++) {
	                if (allowEventBeanType[i] && method.ParameterTypes[i] == typeof(EventBean)) {
	                    childForges[i] = childEvalsEventBeanReturnTypesForges[i];
	                }
	            }
	        }

	        // rewrite those evaluators that should return the event collection
	        if (CollectionUtil.IsAnySet(allowEventBeanCollType)) {
	            for (int i = 0; i < parameters.Count; i++) {
	                if (allowEventBeanCollType[i] && method.ParameterTypes[i] == typeof(ICollection<object>)) {
	                    childForges[i] = childEvalsEventBeanReturnTypesForges[i];
	                }
	            }
	        }

	        // add an evaluator if the method expects a context object
	        if (!method.IsVarArgs && method.ParameterTypes.Length > 0 &&
	                method.ParameterTypes[method.ParameterTypes.Length - 1] == typeof(EPLMethodInvocationContext)) {
	            ExprEvalMethodContext node = new ExprEvalMethodContext(functionName);
	            childForges = (ExprForge[]) CollectionUtil.ArrayExpandAddSingle(childForges, node);
	        }

	        // handle varargs
	        if (method.IsVarArgs) {
	            // handle context parameter
	            int numMethodParams = method.ParameterTypes.Length;
	            if (numMethodParams > 1 && method.ParameterTypes[numMethodParams - 2] == typeof(EPLMethodInvocationContext)) {
	                ExprForge[] rewrittenForges = new ExprForge[childForges.Length + 1];
	                Array.Copy(childForges, 0, rewrittenForges, 0, numMethodParams - 2);
	                ExprEvalMethodContext node = new ExprEvalMethodContext(functionName);
	                rewrittenForges[numMethodParams - 2] = node;
	                Array.Copy(childForges, numMethodParams - 2, rewrittenForges, numMethodParams - 1, childForges.Length - (numMethodParams - 2));
	                childForges = rewrittenForges;
	            }

	            Pair<ExprForge[], ExprEvaluator[]> pair = ExprNodeUtilityMake.MakeVarargArrayEval(method, childForges);
	            childForges = pair.First;
	        }

	        return new ExprNodeUtilMethodDesc(allConstants, childForges, method);
	    }
	}
} // end of namespace