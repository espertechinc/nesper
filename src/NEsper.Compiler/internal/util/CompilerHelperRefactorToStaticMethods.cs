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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerHelperRefactorToStaticMethods
	{
		public const int MAX_METHODS_PER_CLASS_MINIMUM = 1000;

		public static void RefactorMethods(
			IList<CodegenClass> classes,
			int maxMethodsPerClass)
		{
			foreach (var clazz in classes) {
				RefactorMethodsClass(clazz, maxMethodsPerClass);
			}
		}

		private static void RefactorMethodsClass(
			CodegenClass clazz,
			int maxMethodsPerClass)
		{
			var inners = clazz.InnerClasses.ToArray();
			foreach (var inner in inners) {
				RefactorMethodsInnerClass(clazz, inner, maxMethodsPerClass);
			}
		}

		private static void RefactorMethodsInnerClass(
			CodegenClass clazz,
			CodegenInnerClass inner,
			int maxMethodsPerClass)
		{
			if (maxMethodsPerClass < MAX_METHODS_PER_CLASS_MINIMUM) {
				throw new EPException(
					"Invalid value for maximum number of methods per class, expected a minimum of " +
					MAX_METHODS_PER_CLASS_MINIMUM +
					" but received " +
					maxMethodsPerClass);
			}

			var size = inner.Methods.Count;
			if (size <= maxMethodsPerClass) {
				return;
			}

			// collect static methods bottom-up
			ISet<CodegenMethodWGraph> collectedStaticMethods = new HashSet<CodegenMethodWGraph>();
			Func<CodegenMethod, bool> permittedMethods = method => collectedStaticMethods.Contains(method.AssignedMethod);
			foreach (var publicMethod in inner.Methods.PublicMethods) {
				RecursiveBottomUpCollectStatic(publicMethod.Originator, collectedStaticMethods, permittedMethods);
			}

			// collect static methods from private methods preserving the order they appear in
			IList<CodegenMethodWGraph> staticMethods = new List<CodegenMethodWGraph>();
			var count = -1;
			foreach (var privateMethod in inner.Methods.PrivateMethods) {
				count++;
				if (count < maxMethodsPerClass) {
					continue;
				}

				if (collectedStaticMethods.Contains(privateMethod)) {
					staticMethods.Add(privateMethod);
				}
			}

			if (staticMethods.IsEmpty()) {
				return;
			}

			// assign to buckets
			var statics = CollectionUtil.Subdivide(staticMethods, maxMethodsPerClass);

			// for each bucket
			for (var i = 0; i < statics.Count; i++) {
				var bucket = statics[i];

				// new inner class
				var className = inner.ClassName + "util" + i;
				var properties = new CodegenClassProperties();
				var methods = new CodegenClassMethods();
				methods.PrivateMethods.AddAll(bucket);
				foreach (CodegenMethodWGraph privateMethod in bucket) {
					privateMethod.WithStatic();
				}

				var utilClass = new CodegenInnerClass(
					className, null, EmptyList<CodegenTypedParam>.Instance, methods, properties);
				clazz.AddInnerClass(utilClass);

				// repoint
				foreach (var privateMethod in bucket) {
					privateMethod.Originator.AssignedProviderClassName = className;
				}

				// remove private methods from inner class
				inner.Methods.PrivateMethods.RemoveAll(bucket);
			}
		}

		private static void RecursiveBottomUpCollectStatic(
			CodegenMethod method,
			ISet<CodegenMethodWGraph> collected,
			Func<CodegenMethod, bool> permittedMethods)
		{
			foreach (var child in method.Children) {
				RecursiveBottomUpCollectStatic(child, collected, permittedMethods);
			}

			foreach (var child in method.Children) {
				if (!collected.Contains(child.AssignedMethod)) {
					return;
				}
			}

			if (!method.Block.HasInstanceAccess(permittedMethods)) {
				collected.Add(method.AssignedMethod);
			}
		}
	}
} // end of namespace
