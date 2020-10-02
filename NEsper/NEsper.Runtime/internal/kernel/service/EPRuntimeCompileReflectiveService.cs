///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using Module = com.espertech.esper.common.client.module.Module;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPRuntimeCompileReflectiveService
	{
		private const string CLASSNAME_COMPILER_ARGUMENTS = "com.espertech.esper.compiler.client.CompilerArguments";
		private const string CLASSNAME_COMPILER_PATH = "com.espertech.esper.compiler.client.CompilerPath";
		private const string CLASSNAME_COMPILER_PROVIDER = "com.espertech.esper.compiler.client.EPCompilerProvider";
		private const string CLASSNAME_COMPILER = "com.espertech.esper.compiler.internal.util.EPCompilerSPI";
		private const string CLASSNAME_COMPILER_EXPRESSIONS = "com.espertech.esper.compiler.internal.util.EPCompilerSPIExpression";

		private bool _available;
		private string _message;
		private ConstructorInfo _compilerArgsCtor;
		private MethodInfo _compilerArgsGetPath;
		private MethodInfo _compilerPathAdd;
		private MethodInfo _compilerProviderGetCompiler;
		private MethodInfo _compileFireAndForget;
		private MethodInfo _compileModuleString;
		private MethodInfo _compileModuleObject;
		private MethodInfo _expressionCompiler;
		private MethodInfo _compileValidate;
		private MethodInfo _eplToModel;
		private MethodInfo _parseModule;

		public EPRuntimeCompileReflectiveService()
		{
			_available = Init();
		}

		public bool IsCompilerAvailable => _available;

		public EPCompiled ReflectiveCompile(
			string epl,
			Configuration configuration,
			EPCompilerPathable optionalPathable)
		{
			CompileMethod method = (
				compiler,
				args) => _compileModuleString.Invoke(compiler, new[] { epl, args });
			return CompileInternal(method, configuration, optionalPathable);
		}

		public EPCompiled ReflectiveCompile(
			Module module,
			Configuration configuration,
			EPCompilerPathable optionalPathable)
		{
			CompileMethod method = (
				compiler,
				args) => _compileModuleObject.Invoke(compiler, new[] { module, args });
			return CompileInternal(method, configuration, optionalPathable);
		}

		public EPCompiled ReflectiveCompileFireAndForget(
			string epl,
			Configuration configuration,
			EPCompilerPathable optionalPathable)
		{
			CompileMethod method = (
				compiler,
				args) => _compileFireAndForget.Invoke(compiler, new[] { epl, args });
			return CompileInternal(method, configuration, optionalPathable);
		}

		public EPStatementObjectModel ReflectiveEPLToModel(
			string epl,
			Configuration configuration)
		{
			object compiler = GetCompiler();

			// Same as: compiler.eplToModel(epl, Configuration configuration) throws EPCompileException;
			try {
				return (EPStatementObjectModel) _eplToModel.Invoke(compiler, new object[] { epl, configuration });
			}
			catch (Exception ex) {
				throw new EPException("Failed to invoke epl-to-model: " + ex.Message, ex);
			}
		}

		public Module ReflectiveParseModule(string epl)
		{
			object compiler = GetCompiler();

			// Same as: compiler.parseModule(epl);
			try {
				return (Module) _parseModule.Invoke(compiler, new object[] { epl });
			}
			catch (Exception ex) {
				throw new EPException("Failed to invoke parse-module: " + ex.Message, ex);
			}
		}

		public ExprNode ReflectiveCompileExpression(
			string epl,
			EventType[] eventTypes,
			string[] streamNames,
			Configuration configuration)
		{
			object compiler = GetCompiler();

			// Same as: EPCompilerSPIExpression exprCompiler = compiler.expressionCompiler(configuration);
			object exprCompiler;
			try {
				exprCompiler = _expressionCompiler.Invoke(compiler, new object[] { configuration });
			}
			catch (Exception ex) {
				throw new EPException("Failed to invoke expression-compiler-method of compiler path: " + ex.Message, ex);
			}

			// Same as: exprCompiler.compileValidate(epl, eventTypes, streamNames)
			try {
				return (ExprNode) _compileValidate.Invoke(exprCompiler, new object[] { epl, eventTypes, streamNames });
			}
			catch (Exception ex) {
				throw new EPException(ex.Message, ex);
			}
		}

		private EPCompiled CompileInternal(
			CompileMethod compileMethod,
			Configuration configuration,
			EPCompilerPathable optionalPathable)
		{
			object compiler = GetCompiler();

			// Same as: CompilerArguments args = new CompilerArguments(configuration);
			object compilerArguments;
			try {
				compilerArguments = _compilerArgsCtor.Invoke(new object[] {configuration});
			}
			catch (Exception ex) {
				throw new EPException("Failed to instantiate compiler arguments: " + ex.Message, ex);
			}

			// Same as: CompilerPath path = args.getPath()
			object path;
			try {
				path = _compilerArgsGetPath.Invoke(compilerArguments, new object[0]);
			}
			catch (Exception ex) {
				throw new EPException("Failed to instantiate compiler arguments: " + ex.Message, ex);
			}

			// Same as: path.add(runtime.getRuntimePath());
			if (optionalPathable != null) {
				try {
					_compilerPathAdd.Invoke(path, new object[] { optionalPathable });
				}
				catch (Exception ex) {
					throw new EPException("Failed to invoke add-method of compiler path: " + ex.Message, ex);
				}
			}

			try {
				return (EPCompiled) compileMethod.Invoke(compiler, compilerArguments);
			}
			catch (TargetException ex) {
				throw new EPException("Failed to compile: " + ex.InnerException.Message, ex.InnerException);
			}
			catch (TargetInvocationException ex) {
				throw new EPException("Failed to compile: " + ex.InnerException.Message, ex.InnerException);
			}
			catch (Exception ex) {
				throw new EPException("Failed to invoke compile method of compiler: " + ex.Message, ex);
			}
		}

		private bool Init()
		{
			var compilerArgsClass = FindClassByName(CLASSNAME_COMPILER_ARGUMENTS);
			if (compilerArgsClass == null) {
				return false;
			}

			var compilerPathClass = FindClassByName(CLASSNAME_COMPILER_PATH);
			if (compilerPathClass == null) {
				return false;
			}

			var compilerProvider = FindClassByName(CLASSNAME_COMPILER_PROVIDER);
			if (compilerProvider == null) {
				return false;
			}

			var compiler = FindClassByName(CLASSNAME_COMPILER);
			if (compiler == null) {
				return false;
			}

			var compilerExpressions = FindClassByName(CLASSNAME_COMPILER_EXPRESSIONS);
			if (compilerExpressions == null) {
				return false;
			}

			_compilerArgsCtor = FindConstructor(compilerArgsClass, typeof(Configuration));
			if (_compilerArgsCtor == null) {
				return false;
			}

			_compilerArgsGetPath = FindMethod(compilerArgsClass, "GetPath");
			if (_compilerArgsGetPath == null) {
				return false;
			}

			_compilerPathAdd = FindMethod(compilerPathClass, "Add", typeof(EPCompilerPathable));
			if (_compilerPathAdd == null) {
				return false;
			}

			_compilerProviderGetCompiler = FindMethod(compilerProvider, "GetCompiler");
			if (_compilerProviderGetCompiler == null) {
				return false;
			}

			_compileModuleString = FindMethod(compiler, "Compile", typeof(string), compilerArgsClass);
			if (_compileModuleString == null) {
				return false;
			}

			_compileModuleObject = FindMethod(compiler, "Compile", typeof(Module), compilerArgsClass);
			if (_compileModuleObject == null) {
				return false;
			}

			_expressionCompiler = FindMethod(compiler, "ExpressionCompiler", typeof(Configuration));
			if (_expressionCompiler == null) {
				return false;
			}

			_compileValidate = FindMethod(compilerExpressions, "CompileValidate", typeof(string), typeof(EventType[]), typeof(string[]));
			if (_compileValidate == null) {
				return false;
			}

			_eplToModel = FindMethod(compiler, "EplToModel", typeof(string), typeof(Configuration));
			if (_eplToModel == null) {
				return false;
			}

			_parseModule = FindMethod(compiler, "ParseModule", typeof(string));
			if (_parseModule == null) {
				return false;
			}

			_compileFireAndForget = FindMethod(compiler, "CompileQuery", typeof(string), compilerArgsClass);
			return _compileFireAndForget != null;
		}

		private Type FindClassByName(string className)
		{
			try {
				return TypeHelper.ResolveType(className, true);
			}
			catch (TypeLoadException ex) {
				_message = "Failed to find class " + className + ": " + ex.Message;
			}

			return null;
		}

		private ConstructorInfo FindConstructor(
			Type clazz,
			params Type[] args)
		{
			var constructor = clazz.GetConstructor(args);
			if (constructor == null) {
				_message = "Failed to find constructor of class " +
				          clazz.Name +
				          " taking parameters " +
				          TypeHelper.GetParameterAsString(args);
			}

			return constructor;
		}

		private MethodInfo FindMethod(
			Type clazz,
			string name,
			params Type[] args)
		{
			var method = clazz.GetMethod(name, args);
			if (method == null) {
				var baseType = clazz.BaseType;
				if (baseType != null) {
					method = FindMethod(baseType, name, args);
				}

				if (method == null) {
					method = clazz.GetInterfaces()
						.Select(_ => FindMethod(_, name, args))
						.FirstOrDefault(_ => _ != null);
				}
				
				if (method == null) {
					_message = "Failed to find method '" +
					           name +
					           "' of class " +
					           clazz.Name +
					           " taking parameters " +
					           TypeHelper.GetParameterAsString(args);

					return null;
				}
			}

			return method;
		}

		internal delegate object CompileMethod(
			object compiler,
			object compilerArguments);

		private object GetCompiler()
		{
			if (!_available) {
				throw new EPException(_message);
			}

			// Same as: EPCompiler compiler = EPCompilerProvider.getCompiler()
			try {
				return _compilerProviderGetCompiler.Invoke(null, new object[0]);
			}
			catch (Exception ex) {
				throw new EPException("Failed to invoke getCompiler-method of compiler provider: " + ex.Message, ex);
			}
		}
	}
} // end of namespace
