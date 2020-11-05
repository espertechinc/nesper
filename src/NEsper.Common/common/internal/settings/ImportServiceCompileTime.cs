///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.settings
{
    public interface ImportServiceCompileTime : ImportService
    {
        bool IsSortUsingCollator { get; }
        MathContext DefaultMathContext { get; }
        AdvancedIndexFactoryProvider ResolveAdvancedIndexProvider(string indexTypeName);

        MethodInfo ResolveMethodOverloadChecked(
            Type clazz,
            string methodName);

        MethodInfo ResolveMethodOverloadChecked(
            string className,
            string methodName,
            ExtensionClass classpathExtension);

        Type ResolveAnnotation(string className);

        Pair<Type, ImportSingleRowDesc> ResolveSingleRow(
            string name,
            ExtensionSingleRow classpathExtensionSingleRow);

        MethodInfo ResolveNonStaticMethodOverloadChecked(
            Type clazz,
            string methodName);

        MethodInfo ResolveMethod(
            Type clazz,
            string methodName,
            Type[] paramTypes,
            bool[] allowEventBeanType);

        Type ResolveEnumMethod(string name);

        AggregationFunctionForge ResolveAggregationFunction(
            string functionName,
            ExtensionAggregationFunction extension);

        Pair<ConfigurationCompilerPlugInAggregationMultiFunction, Type> ResolveAggregationMultiFunction(
            string name,
            ExtensionAggregationMultiFunction classpathExtensionAggregationMultiFunction);

        ExprNode ResolveAggExtendedBuiltin(
            string name,
            bool isDistinct);

        Type ResolveDateTimeMethod(string name);
        ExprNode ResolveSingleRowExtendedBuiltin(string name);

        void AddImport(Import importName);
    }
} // end of namespace