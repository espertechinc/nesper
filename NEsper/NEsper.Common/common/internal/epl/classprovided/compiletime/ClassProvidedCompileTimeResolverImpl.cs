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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
	public class ClassProvidedCompileTimeResolverImpl : ClassProvidedCompileTimeResolver
	{
		private readonly string moduleName;
		private readonly ICollection<string> moduleUses;
		private readonly ClassProvidedCompileTimeRegistry locals;
		private readonly PathRegistry<string, ClassProvided> path;
		private readonly ModuleDependenciesCompileTime moduleDependencies;
		private readonly bool isFireAndForget;

		public ClassProvidedCompileTimeResolverImpl(
			string moduleName,
			ICollection<string> moduleUses,
			ClassProvidedCompileTimeRegistry locals,
			PathRegistry<string, ClassProvided> path,
			ModuleDependenciesCompileTime moduleDependencies,
			bool isFireAndForget)
		{
			this.moduleName = moduleName;
			this.moduleUses = moduleUses;
			this.locals = locals;
			this.path = path;
			this.moduleDependencies = moduleDependencies;
			this.isFireAndForget = isFireAndForget;
		}

		public ClassProvided ResolveClass(string name)
		{
			// try self-originated protected types first
			var localExpr = locals.Classes.Get(name);
			if (localExpr != null) {
				return localExpr;
			}

			try {
				var expression = path.GetAnyModuleExpectSingle(name, moduleUses);
				if (expression != null) {
					if (!isFireAndForget && !NameAccessModifierExtensions.Visible(expression.First.Visibility, expression.First.ModuleName, moduleName)) {
						return null;
					}

					moduleDependencies.AddPathClass(name, expression.Second);
					return expression.First;
				}
			}
			catch (PathException e) {
				throw CompileTimeResolver.MakePathAmbiguous(PathRegistryObjectType.CLASSPROVIDED, name, e);
			}

			return null;
		}

		public Pair<Type, ImportSingleRowDesc> ResolveSingleRow(string name)
		{
			Pair<Type, ExtensionSingleRowFunctionAttribute> pair = ResolveFromLocalAndPath<ExtensionSingleRowFunctionAttribute>(
				name,
				locals,
				path,
				"single-row function",
				moduleUses,
				moduleDependencies,
				anno => Collections.SingletonSet(anno.Name));
			return pair == null ? null : new Pair<Type, ImportSingleRowDesc>(
				pair.First, new ImportSingleRowDesc(pair.First, pair.Second));
		}

		public Type ResolveAggregationFunction(string name)
		{
			Pair<Type, ExtensionAggregationFunctionAttribute> pair = ResolveFromLocalAndPath<ExtensionAggregationFunctionAttribute>(
				name,
				locals,
				path,
				"aggregation function",
				moduleUses,
				moduleDependencies,
				anno => Collections.SingletonSet(anno.GetName()));
			return pair == null ? null : pair.First;
		}

		public Pair<Type, string[]> ResolveAggregationMultiFunction(string name)
		{
			Func<ExtensionAggregationMultiFunctionAttribute, ISet<string>> nameProvision = anno => {
				ISet<string> names = new HashSet<string>();
				string[] split = anno.Names.SplitCsv();
				foreach (var nameprovided in split) {
					names.Add(nameprovided.Trim());
				}

				return names;
			};
			var pair = ResolveFromLocalAndPath(
				name,
				locals,
				path,
				"aggregation multi-function",
				moduleUses,
				moduleDependencies,
				nameProvision);
			return pair == null ? null : new Pair<Type, string[]>(pair.First, pair.Second.Names.SplitCsv());
		}

		public bool IsEmpty()
		{
			return path.IsEmpty() && locals.Classes.IsEmpty();
		}

		public void AddTo(IDictionary<string, byte[]> additionalClasses)
		{
			path.Traverse(cp => additionalClasses.PutAll(cp.Bytes));
		}

		public void RemoveFrom(IDictionary<string, byte[]> moduleBytes)
		{
			Consumer<ClassProvided> classProvidedByteCodeRemover = item => {
				foreach (var entry in item.Bytes) {
					moduleBytes.Remove(entry.Key);
				}
			};
			path.Traverse(classProvidedByteCodeRemover);
		}

		internal static Pair<Type, T> ResolveFromLocalAndPath<T>(
			string soughtName,
			ClassProvidedCompileTimeRegistry locals,
			PathRegistry<string, ClassProvided> path,
			string objectName,
			ICollection<string> moduleUses,
			ModuleDependenciesCompileTime moduleDependencies,
			Func<T, ISet<string>> namesProvider)
			where T : Attribute
		{
			if (locals.Classes.IsEmpty() && path.IsEmpty()) {
				return null;
			}

			var annotationType = typeof(T);
			try {
				// try resolve from local
				var localPair = ResolveFromLocal(soughtName, locals, annotationType, objectName, namesProvider);
				if (localPair != null) {
					return localPair;
				}

				// try resolve from path, using module-uses when necessary
				return ResolveFromPath(soughtName, path, annotationType, objectName, moduleUses, moduleDependencies, namesProvider);
			}
			catch (ExprValidationException ex) {
				throw new EPException(ex.Message, ex);
			}
		}

		private static Pair<Type, T> ResolveFromLocal<T>(
			string soughtName,
			ClassProvidedCompileTimeRegistry locals,
			Type annotationType,
			string objectName,
			Func<T, ISet<string>> namesProvider)
			where T : Attribute
		{
			var foundLocal = new List<Pair<Type, T>>();
			foreach (var entry in locals.Classes) {
				EPTypeHelper.TraverseAnnotations<T>(
					entry.Value.ClassesMayNull,
					(clazz, annotation) => {
						var t = (T) annotation;
						var names = namesProvider.Invoke(t);
						foreach (var name in names) {
							if (soughtName.Equals(name)) {
								foundLocal.Add(new Pair<Type, T>(clazz, t));
							}
						}
					});
			}

			if (foundLocal.Count > 1) {
				throw GetDuplicateSingleRow(soughtName, objectName);
			}

			if (foundLocal.Count == 1) {
				return foundLocal[0];
			}

			return null;
		}

		private static Pair<Type, T> ResolveFromPath<T>(
			string soughtName,
			PathRegistry<string, ClassProvided> path,
			Type annotationType,
			string objectName,
			ICollection<string> moduleUses,
			ModuleDependenciesCompileTime moduleDependencies,
			Func<T, ISet<string>> namesProvider)
			where T : Attribute
		{
			// TBD: Verify that annotationType is derived from T
			if (!typeof(T).IsAssignableFrom(annotationType)) {
				throw new ArgumentException("cannot assign annotationType from " + typeof(T).FullName);
			}
		
			IList<PathFunc<T>> foundPath = new List<PathFunc<T>>();
			path.TraverseWithModule((moduleName, classProvided) => {
				EPTypeHelper.TraverseAnnotations<T>(
					classProvided.ClassesMayNull,
					(
						clazz,
						annotation) => {
						var t = annotation;
						var names = namesProvider.Invoke(t);
						foreach (var name in names) {
							if (soughtName.Equals(name)) {
								foundPath.Add(new PathFunc<T>(moduleName, clazz, t));
							}
						}
					});
				});

			PathFunc<T> foundPathFunc;
			if (foundPath.IsEmpty()) {
				return null;
			}
			else if (foundPath.Count == 1) {
				foundPathFunc = foundPath[0];
			}
			else {
				if (moduleUses == null || moduleUses.IsEmpty()) {
					throw GetDuplicateSingleRow(soughtName, objectName);
				}

				IList<PathFunc<T>> matchesUses = new List<PathFunc<T>>(2);
				foreach (var func in foundPath) {
					if (moduleUses.Contains(func.OptionalModuleName)) {
						matchesUses.Add(func);
					}
				}

				if (matchesUses.Count > 1) {
					throw GetDuplicateSingleRow(soughtName, objectName);
				}

				if (matchesUses.IsEmpty()) {
					return null;
				}

				foundPathFunc = matchesUses[0];
			}

			moduleDependencies.AddPathClass(foundPathFunc.Clazz.Name, foundPathFunc.OptionalModuleName);
			return new Pair<Type, T>(foundPathFunc.Clazz, foundPathFunc.Annotation);
		}

		private static ExprValidationException GetDuplicateSingleRow(
			string name,
			string objectName)
		{
			return new ExprValidationException("The plug-in " + objectName + " '" + name + "' occurs multiple times");
		}

		private class PathFunc<T>
		{
			public PathFunc(
				string optionalModuleName,
				Type clazz,
				T annotation)
			{
				this.OptionalModuleName = optionalModuleName;
				this.Clazz = clazz;
				this.Annotation = annotation;
			}

			public string OptionalModuleName { get; }

			public Type Clazz { get; }

			public T Annotation { get; }
		}
	}
} // end of namespace
