///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenStackGenerator
    {
        public static void RecursiveBuildStack(
            CodegenProperty propertyNode,
            string name,
            CodegenClassMethods methods,
            CodegenClassProperties properties)
        {
            if (propertyNode.OptionalSymbolProvider == null) {
                throw new ArgumentException("Property node does not have symbol provider");
            }

            var currentSymbols = new Dictionary<string, Type>();
            propertyNode.OptionalSymbolProvider.Provide(currentSymbols);

            var property = new CodegenPropertyWGraph(
                name,
                propertyNode.ReturnType,
                propertyNode.ReturnTypeName,
                propertyNode.AdditionalDebugInfo,
                propertyNode.GetterBlock,
                propertyNode.SetterBlock,
                true,
                propertyNode.IsOverride);

            propertyNode.AssignedProperty = property;
            properties.PublicProperties.Add(property);

            foreach (var child in propertyNode.Children) {
                RecursiveAdd(
                    child,
                    currentSymbols,
                    methods,
                    properties,
                    propertyNode.IsStatic);
            }
        }

        public static void RecursiveBuildStack(
            CodegenMethod methodNode,
            string name,
            CodegenClassMethods methods,
            CodegenClassProperties properties)
        {
            if (methodNode.OptionalSymbolProvider == null) {
                throw new ArgumentException("Method node does not have symbol provider");
            }

            IDictionary<string, Type> currentSymbols = new Dictionary<string, Type>();
            methodNode.OptionalSymbolProvider.Provide(currentSymbols);

            if (!(methodNode is CodegenCtor)) {
                var footprint = new CodegenMethodFootprint(
                    methodNode.ReturnType,
                    methodNode.ReturnTypeName,
                    methodNode.LocalParams,
                    methodNode.AdditionalDebugInfo);
                var method = new CodegenMethodWGraph(
                    name,
                    footprint,
                    methodNode.Block,
                    true,
                    methodNode.IsOverride,
                    methodNode.IsStatic);

                methodNode.AssignedMethod = method;
                methods.PublicMethods.Add(method);
            }

            foreach (var child in methodNode.Children) {
                RecursiveAdd(
                    child,
                    currentSymbols,
                    methods,
                    properties,
                    methodNode.IsStatic);
            }
        }

        private static void RecursiveAdd(
            CodegenMethod methodNode,
            IDictionary<string, Type> currentSymbols,
            CodegenClassMethods classMethods,
            CodegenClassProperties classProperties,
            bool isStatic)
        {
            ISet<string> namesPassed = GetNamesPassed(methodNode);
            methodNode.DeepParameters = namesPassed;

            IList<CodegenNamedParam> paramset = new List<CodegenNamedParam>(
                namesPassed.Count + methodNode.LocalParams.Count);

            // add local params
            foreach (var named in methodNode.LocalParams) {
                paramset.Add(named);
            }

            // add pass-thru for those methods that do not have their own scope
            if (methodNode.OptionalSymbolProvider == null) {
                foreach (var nameX in namesPassed) {
                    var symbolType = currentSymbols.Get(nameX);
                    if (symbolType == null) {
                        throw new IllegalStateException(
                            "Failed to find named parameter '" +
                            nameX +
                            "' for method from " +
                            methodNode.AdditionalDebugInfo);
                    }

                    paramset.Add(new CodegenNamedParam(symbolType, nameX));
                }
            }
            else {
                currentSymbols = new Dictionary<string, Type>();
                methodNode.OptionalSymbolProvider.Provide(currentSymbols);
            }

            var name = "M" + classMethods.PrivateMethods.Count;
            var footprint = new CodegenMethodFootprint(
                methodNode.ReturnType,
                methodNode.ReturnTypeName,
                paramset,
                methodNode.AdditionalDebugInfo);
            var method = new CodegenMethodWGraph(
                name,
                footprint,
                methodNode.Block,
                false,
                methodNode.IsOverride,
                isStatic);

            methodNode.AssignedMethod = method;
            classMethods.PrivateMethods.Add(method);

            foreach (var child in methodNode.Children) {
                RecursiveAdd(
                    child,
                    currentSymbols,
                    classMethods,
                    classProperties,
                    isStatic);
            }
        }

        private static void RecursiveAdd(
            CodegenProperty propertyNode,
            IDictionary<string, Type> currentSymbols,
            CodegenClassMethods classMethods,
            CodegenClassProperties classProperties,
            bool isStatic)
        {
            // add pass-thru for those methods that do not have their own scope
            if (propertyNode.OptionalSymbolProvider != null) {
                currentSymbols = new Dictionary<string, Type>();
                propertyNode.OptionalSymbolProvider.Provide(currentSymbols);
            }

            var name = "AutoProp" + classProperties.PrivateProperties.Count;
            var propertyWGraph = new CodegenPropertyWGraph(
                name,
                propertyNode.ReturnType,
                propertyNode.ReturnTypeName,
                propertyNode.AdditionalDebugInfo,
                propertyNode.GetterBlock,
                propertyNode.SetterBlock,
                false,
                false);

            propertyNode.AssignedProperty = propertyWGraph;
            classProperties.PrivateProperties.Add(propertyWGraph);

            foreach (var child in propertyNode.Children) {
                RecursiveAdd(
                    child,
                    currentSymbols,
                    classMethods,
                    classProperties,
                    isStatic);
            }
        }

        private static ISet<string> GetNamesPassed(CodegenMethod node)
        {
            var names = new SortedSet<string>();
            RecursiveGetNamesPassed(node, names);
            return names;
        }

        private static void RecursiveGetNamesPassed(
            CodegenMethod node,
            ISet<string> names)
        {
            if (node.OptionalSymbolProvider != null) {
                return;
            }

            foreach (var @ref in node.Environment) {
                names.Add(@ref.Ref);
            }

            foreach (var child in node.Children) {
                RecursiveGetNamesPassed(child, names);
            }
        }

        public static void MakeSetter(
            string className,
            string memberName,
            IList<CodegenTypedParam> members,
            CodegenClassMethods methods,
            CodegenClassScope classScope,
            CodegenClassProperties properties)
        {
            members.Add(new CodegenTypedParam(className, memberName));

            var method = CodegenMethod.MakeMethod(
                    typeof(void),
                    typeof(CodegenStackGenerator),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(className, "p");
            method.Block.AssignRef(memberName, Ref("p"));
            var setterMethodName = "Set" + memberName.Substring(0, 1).ToUpperInvariant() + memberName.Substring(1);
            RecursiveBuildStack(method, setterMethodName, methods, properties);
        }
    }
} // end of namespace