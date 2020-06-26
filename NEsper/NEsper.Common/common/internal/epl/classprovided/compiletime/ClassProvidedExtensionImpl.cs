///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.singlerowfunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedExtensionImpl : ClassProvidedExtension
    {
        private readonly IDictionary<string, byte[]> bytes = new LinkedHashMap<string, byte[]>();
        private readonly IList<Type> classes = new List<Type>();
        private readonly ClassProvidedCompileTimeResolver resolver;

        private IDictionary<string, Pair<Type, ExtensionAggregationFunctionAttribute>> aggregationFunctionExtensions =
            new EmptyDictionary<string, Pair<Type, ExtensionAggregationFunctionAttribute>>();

        private IDictionary<string, Pair<Type, string[]>> aggregationMultiFunctionExtensions =
            new EmptyDictionary<string, Pair<Type, string[]>>();

        private IDictionary<string, Pair<Type, ExtensionSingleRowFunctionAttribute>> singleRowFunctionExtensions =
            new EmptyDictionary<string, Pair<Type, ExtensionSingleRowFunctionAttribute>>();

        public ClassProvidedExtensionImpl(ClassProvidedCompileTimeResolver resolver)
        {
            this.resolver = resolver;
        }

        public void Add(
            IList<Type> classes,
            IDictionary<string, byte[]> bytes)
        {
            this.classes.AddAll(classes);
            this.bytes.PutAll(bytes); // duplicate class names checked at compile-time

            try {
                EPTypeHelper.TraverseAnnotations<ExtensionSingleRowFunctionAttribute>(
                    classes,
                    (
                        clazz,
                        annotation) => {
                        if (singleRowFunctionExtensions.IsEmpty()) {
                            singleRowFunctionExtensions = new Dictionary<string, Pair<Type, ExtensionSingleRowFunctionAttribute>>();
                        }

                        if (singleRowFunctionExtensions.ContainsKey(annotation.Name)) {
                            throw new EPException("The plug-in single-row function '" + annotation.Name + "' occurs multiple times");
                        }

                        singleRowFunctionExtensions.Put(annotation.Name, new Pair<Type, ExtensionSingleRowFunctionAttribute>(clazz, annotation));
                    });

                EPTypeHelper.TraverseAnnotations<ExtensionAggregationFunctionAttribute>(
                    classes,
                    (
                        clazz,
                        annotation) => {
                        if (aggregationFunctionExtensions.IsEmpty()) {
                            aggregationFunctionExtensions = new Dictionary<string, Pair<Type, ExtensionAggregationFunctionAttribute>>();
                        }

                        if (aggregationFunctionExtensions.ContainsKey(annotation.Name)) {
                            throw new EPException("The plug-in aggregation function '" + annotation.Name + "' occurs multiple times");
                        }

                        aggregationFunctionExtensions.Put(annotation.Name, new Pair<Type, ExtensionAggregationFunctionAttribute>(clazz, annotation));
                    });

                EPTypeHelper.TraverseAnnotations<ExtensionAggregationMultiFunctionAttribute>(
                    classes,
                    (
                        clazz,
                        annotation) => {
                        if (aggregationMultiFunctionExtensions.IsEmpty()) {
                            aggregationMultiFunctionExtensions = new Dictionary<string, Pair<Type, string[]>>();
                        }

                        var names = annotation.Names.Split(',');
                        var namesDeduplicated = new HashSet<string>();
                        foreach (var nameWithSpaces in names) {
                            var name = nameWithSpaces.Trim();
                            namesDeduplicated.Add(name);
                        }

                        var namesArray = namesDeduplicated.ToArray();

                        foreach (var name in namesDeduplicated) {
                            if (aggregationMultiFunctionExtensions.ContainsKey(name)) {
                                throw new EPException("The plug-in aggregation multi-function '" + name + "' occurs multiple times");
                            }

                            aggregationMultiFunctionExtensions.Put(name, new Pair<Type, string[]>(clazz, namesArray));
                        }
                    });
            }
            catch (EPException ex) {
                throw new ExprValidationException(ex.Message, ex);
            }
        }

        public Type FindClassByName(string className)
        {
            // check inlined classes
            foreach (var clazz in classes) {
                if (clazz.Name == className) {
                    return clazz;
                }
            }

            // check same-module (create inlined_class) or path classes
            var provided = resolver.ResolveClass(className);
            if (provided != null) {
                foreach (var clazz in provided.ClassesMayNull) {
                    if (clazz.Name == className) {
                        return clazz;
                    }
                }
            }

            return null;
        }

        public Pair<Type, ImportSingleRowDesc> ResolveSingleRow(string name)
        {
            // check local
            var pair = singleRowFunctionExtensions.Get(name);
            if (pair != null) {
                return new Pair<Type, ImportSingleRowDesc>(
                    pair.First,
                    new ImportSingleRowDesc(pair.First, pair.Second));
            }

            // check same-module (create inlined_class) or path classes
            return resolver.ResolveSingleRow(name);
        }

        public Type ResolveAggregationFunction(string name)
        {
            // check local
            var pair = aggregationFunctionExtensions.Get(name);
            if (pair != null) {
                return pair.First;
            }

            // check same-module (create inlined_class) or path classes
            return resolver.ResolveAggregationFunction(name);
        }

        public Pair<Type, string[]> ResolveAggregationMultiFunction(string name)
        {
            // check local
            var pair = aggregationMultiFunctionExtensions.Get(name);
            if (pair != null) {
                return pair;
            }

            // check same-module (create inlined_class) or path classes
            return resolver.ResolveAggregationMultiFunction(name);
        }

        public IDictionary<string, byte[]> GetBytes()
        {
            return bytes;
        }

        public bool IsLocalInlinedClass(Type declaringClass)
        {
            return classes.Any(clazz => declaringClass == clazz);
        }
    }
} // end of namespace