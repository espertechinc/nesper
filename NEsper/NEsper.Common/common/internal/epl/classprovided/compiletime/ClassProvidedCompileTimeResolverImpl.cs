///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.singlerowfunc;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using java.util.function;
namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
	public class ClassProvidedCompileTimeResolverImpl : ClassProvidedCompileTimeResolver {
	    private readonly string moduleName;
	    private readonly ISet<string> moduleUses;
	    private readonly ClassProvidedCompileTimeRegistry locals;
	    private readonly PathRegistry<string, ClassProvided> path;
	    private readonly ModuleDependenciesCompileTime moduleDependencies;
	    private readonly bool isFireAndForget;

	    public ClassProvidedCompileTimeResolverImpl(string moduleName, ISet<string> moduleUses, ClassProvidedCompileTimeRegistry locals, PathRegistry<string, ClassProvided> path, ModuleDependenciesCompileTime moduleDependencies, bool isFireAndForget) {
	        this.moduleName = moduleName;
	        this.moduleUses = moduleUses;
	        this.locals = locals;
	        this.path = path;
	        this.moduleDependencies = moduleDependencies;
	        this.isFireAndForget = isFireAndForget;
	    }

	    public ClassProvided ResolveClass(string name) {
	        // try self-originated protected types first
	        ClassProvided localExpr = locals.Classes.Get(name);
	        if (localExpr != null) {
	            return localExpr;
	        }
	        try {
	            Pair<ClassProvided, string> expression = path.GetAnyModuleExpectSingle(name, moduleUses);
	            if (expression != null) {
	                if (!isFireAndForget && !NameAccessModifier.Visible(expression.First.Visibility, expression.First.ModuleName, moduleName)) {
	                    return null;
	                }

	                moduleDependencies.AddPathClass(name, expression.Second);
	                return expression.First;
	            }
	        } catch (PathException e) {
	            throw CompileTimeResolver.MakePathAmbiguous(PathRegistryObjectType.CLASSPROVIDED, name, e);
	        }
	        return null;
	    }

	    public Pair<Type, ClasspathImportSingleRowDesc> ResolveSingleRow(string name) {
	        Pair<Type, ExtensionSingleRowFunctionAttribute> pair = ResolveFromLocalAndPath(name, locals, path, typeof(ExtensionSingleRowFunctionAttribute), "single-row function", moduleUses, moduleDependencies, anno -> Collections.Singleton(anno.Name()));
	        return pair == null ? null : new Pair<>(pair.First, new ClasspathImportSingleRowDesc(pair.First, pair.Second));
	    }

	    public Type ResolveAggregationFunction(string name) {
	        Pair<Type, ExtensionAggregationFunction> pair = ResolveFromLocalAndPath(name, locals, path, typeof(ExtensionAggregationFunction), "aggregation function", moduleUses, moduleDependencies, anno -> Collections.Singleton(anno.Name()));
	        return pair == null ? null : pair.First;
	    }

	    public Pair<Type, string[]> ResolveAggregationMultiFunction(string name) {
	        Function<ExtensionAggregationMultiFunction, ISet<string>> nameProvision = anno -> {
	            ISet<string> names = new HashSet<>(2);
	            string[] split = anno.Names().Split(",");
	            foreach (string nameprovided in split) {
	                names.Add(nameprovided.Trim());
	            }
	            return names;
	        };
	        Pair<Type, ExtensionAggregationMultiFunction> pair = ResolveFromLocalAndPath(name, locals, path, typeof(ExtensionAggregationMultiFunction), "aggregation multi-function", moduleUses, moduleDependencies, nameProvision);
	        return pair == null ? null : new Pair<>(pair.First, pair.Second.Names().Split(","));
	    }

	    public bool IsEmpty() {
	        return path.IsEmpty() && locals.Classes.IsEmpty();
	    }

	    public void AddTo(IDictionary<string, byte[]> additionalClasses) {
	        path.Traverse(cp -> additionalClasses.PutAll(cp.Bytes));
	    }

	    public void RemoveFrom(IDictionary<string, byte[]> moduleBytes) {
	        Consumer<ClassProvided> classProvidedByteCodeRemover = item -> {
	            foreach (KeyValuePair<string, byte[]> entry in item.Bytes.EntrySet()) {
	                moduleBytes.Remove(entry.Key);
	            }
	        };
	        path.Traverse(classProvidedByteCodeRemover);
	    }

	    private static <T> Pair<Type, T> ResolveFromLocalAndPath(string soughtName, ClassProvidedCompileTimeRegistry locals, PathRegistry<string, ClassProvided> path, Type<T> annotationType, string objectName, ISet<string> moduleUses, ModuleDependenciesCompileTime moduleDependencies, Function<T, ISet<string>> namesProvider) {
	        if (locals.Classes.IsEmpty() && path.IsEmpty()) {
	            return null;
	        }

	        try {
	            // try resolve from local
	            Pair<Type, T> localPair = ResolveFromLocal(soughtName, locals, annotationType, objectName, namesProvider);
	            if (localPair != null) {
	                return localPair;
	            }

	            // try resolve from path, using module-uses when necessary
	            return ResolveFromPath(soughtName, path, annotationType, objectName, moduleUses, moduleDependencies, namesProvider);
	        } catch (ExprValidationException ex) {
	            throw new EPException(ex.Message, ex);
	        }
	    }

	    private static <T> Pair<Type, T> ResolveFromLocal(string soughtName, ClassProvidedCompileTimeRegistry locals, Type annotationType, string objectName, Function<T, ISet<string>> namesProvider) {
	        IList<Pair<Type, T>> foundLocal = new List<>(2);
	        foreach (KeyValuePair<string, ClassProvided> entry in locals.Classes.EntrySet()) {
	            TypeHelper.TraverseAnnotations(entry.Value.ClassesMayNull, annotationType, (clazz, annotation) -> {
	                T t = (T) annotation;
	                ISet<string> names = namesProvider.Apply(t);
	                foreach (string name in names) {
	                    if (soughtName.Equals(name)) {
	                        foundLocal.Add(new Pair<>(clazz, t));
	                    }
	                }
	            });
	        }
	        if (foundLocal.Count > 1) {
	            throw GetDuplicateSingleRow(soughtName, objectName);
	        }
	        if (foundLocal.Count == 1) {
	            return foundLocal.Get(0);
	        }
	        return null;
	    }

	    private static <T> Pair<Type, T> ResolveFromPath(string soughtName, PathRegistry<string, ClassProvided> path, Type annotationType, string objectName, ISet<string> moduleUses, ModuleDependenciesCompileTime moduleDependencies, Function<T, ISet<string>> namesProvider) {
	        IList<PathFunc<T>> foundPath = new List<>(2);
	        path.TraverseWithModule((moduleName, classProvided) -> {
	            TypeHelper.TraverseAnnotations(classProvided.ClassesMayNull, annotationType, (clazz, annotation) -> {
	                T t = (T) annotation;
	                ISet<string> names = namesProvider.Apply(t);
	                foreach (string name in names) {
	                    if (soughtName.Equals(name)) {
	                        foundPath.Add(new PathFunc<T>(moduleName, clazz, t));
	                    }
	                }
	            });
	        });

	        PathFunc<T> foundPathFunc;
	        if (foundPath.IsEmpty()) {
	            return null;
	        } else if (foundPath.Count == 1) {
	            foundPathFunc = foundPath.Get(0);
	        } else {
	            if (moduleUses == null || moduleUses.IsEmpty()) {
	                throw GetDuplicateSingleRow(soughtName, objectName);
	            }
	            IList<PathFunc<T>> matchesUses = new List<>(2);
	            foreach (PathFunc<T> func in foundPath) {
	                if (moduleUses.Contains(func.optionalModuleName)) {
	                    matchesUses.Add(func);
	                }
	            }
	            if (matchesUses.Count > 1) {
	                throw GetDuplicateSingleRow(soughtName, objectName);
	            }
	            if (matchesUses.IsEmpty()) {
	                return null;
	            }
	            foundPathFunc = matchesUses.Get(0);
	        }

	        moduleDependencies.AddPathClass(foundPathFunc.Clazz.Name, foundPathFunc.OptionalModuleName);
	        return new Pair<>(foundPathFunc.Clazz, foundPathFunc.annotation);
	    }

	    private static ExprValidationException GetDuplicateSingleRow(string name, string objectName) {
	        return new ExprValidationException("The plug-in " + objectName + " '" + name + "' occurs multiple times");
	    }

	    private static class PathFunc<T> {
	        private readonly string optionalModuleName;
	        private readonly Type clazz;
	        private readonly T annotation;

	        public PathFunc(string optionalModuleName, Type clazz, T annotation) {
	            this.optionalModuleName = optionalModuleName;
	            this.clazz = clazz;
	            this.annotation = annotation;
	        }

	        public string GetOptionalModuleName() {
	            return optionalModuleName;
	        }

	        public Type GetClazz() {
	            return clazz;
	        }

	        public T GetAnnotation() {
	            return annotation;
	        }
	    }
	}
} // end of namespace
