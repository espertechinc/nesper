///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashUtil
    {
        public static void ValidateContextDesc(
            string contextName,
            ContextSpecHash hashedSpec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (hashedSpec.Items.IsEmpty()) {
                throw new ExprValidationException("Empty list of hash items");
            }

            foreach (var item in hashedSpec.Items) {
                if (item.Function.Parameters.IsEmpty()) {
                    throw new ExprValidationException(
                        "For context '" +
                        contextName +
                        "' expected one or more parameters to the hash function, but found no parameter list");
                }

                // determine type of hash to use
                var hashFuncName = item.Function.Name;
                var hashFunction = HashFunctionEnumExtensions.Determine(contextName, hashFuncName);
                Pair<Type, ImportSingleRowDesc> hashSingleRowFunction = null;
                if (hashFunction == null) {
                    try {
                        hashSingleRowFunction = services.ImportServiceCompileTime.ResolveSingleRow(hashFuncName);
                    }
                    catch (Exception) {
                        // expected
                    }

                    if (hashSingleRowFunction == null) {
                        throw new ExprValidationException(
                            "For context '" +
                            contextName +
                            "' expected a hash function that is any of {" +
                            HashFunctionEnumExtensions.GetStringList() +
                            "} or a plug-in single-row function or script but received '" +
                            hashFuncName +
                            "'");
                    }
                }

                // get first parameter
                var paramExpr = item.Function.Parameters[0];
                var paramType = paramExpr.Forge.EvaluationType;
                EventPropertyValueGetterForge getter;

                if (hashFunction == HashFunctionEnum.CONSISTENT_HASH_CRC32) {
                    if (item.Function.Parameters.Count > 1 || paramType != typeof(string)) {
                        getter = new ContextControllerHashedGetterCRC32SerializedForge(
                            item.Function.Parameters,
                            hashedSpec.Granularity);
                    }
                    else {
                        getter = new ContextControllerHashedGetterCRC32SingleForge(paramExpr, hashedSpec.Granularity);
                    }
                }
                else if (hashFunction == HashFunctionEnum.HASH_CODE) {
                    if (item.Function.Parameters.Count > 1) {
                        getter = new ContextControllerHashedGetterHashMultiple(
                            item.Function.Parameters,
                            hashedSpec.Granularity);
                    }
                    else {
                        getter = new ContextControllerHashedGetterHashSingleForge(paramExpr, hashedSpec.Granularity);
                    }
                }
                else if (hashSingleRowFunction != null) {
                    getter = new ContextControllerHashedGetterSingleRowForge(
                        hashSingleRowFunction,
                        item.Function.Parameters,
                        hashedSpec.Granularity,
                        item.FilterSpecCompiled.FilterForEventType,
                        statementRawInfo,
                        services);
                }
                else {
                    throw new ArgumentException("Unrecognized hash code function '" + hashFuncName + "'");
                }

                // create and register expression
                var expression = item.Function.Name +
                                 "(" +
                                 ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(paramExpr) +
                                 ")";
                var lookupable = new ExprFilterSpecLookupableForge(expression, getter, typeof(int), true);
                item.Lookupable = lookupable;
            }
        }

        public static ContextControllerHashSvc MakeService(
            ContextControllerHashFactory factory,
            ContextManagerRealization realization)
        {
            var factories = realization.ContextManager.ContextDefinition.ControllerFactories;
            var preallocate = factory.HashSpec.IsPreallocate;
            if (factories.Length == 1) {
                return new ContextControllerHashSvcLevelOne(preallocate);
            }

            return new ContextControllerHashSvcLevelAny(preallocate);
        }
    }
} // end of namespace