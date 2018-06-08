///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.espertech.esper.codegen.compile;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.core
{
    public class CodegenContext : ICodegenContext
    {
        private readonly IList<ICodegenMember> _members = new List<ICodegenMember>();
        private readonly IList<ICodegenMethod> _methods = new List<ICodegenMethod>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CodegenContext"/> class.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        public CodegenContext(ICodegenCompiler compiler)
        {
            Compiler = compiler;
        }

        public ICodegenCompiler Compiler { get; set; }

        public void AddMember(string memberName, Type clazz, Object @object)
        {
            _members.Add(new CodegenMember(memberName, clazz, @object));
        }

        public void AddMember(string memberName, Type clazz, Type optionalTypeParam, Object @object)
        {
            _members.Add(new CodegenMember(memberName, clazz, optionalTypeParam, @object));
        }

        public void AddMember(ICodegenMember entry)
        {
            _members.Add(entry);
        }

        public ICodegenMember MakeMember(Type clazz, Object @object)
        {
            string memberName = CodeGenerationIDGenerator.GenerateMember();
            return new CodegenMember(memberName, clazz, @object);
        }

        public ICodegenMember MakeAddMember(Type clazz, Object @object)
        {
            ICodegenMember member = MakeMember(clazz, @object);
            _members.Add(member);
            return member;
        }

        public ICodegenMember MakeMember(Type clazz, Type optionalTypeParam, Object @object)
        {
            string memberName = CodeGenerationIDGenerator.GenerateMember();
            return new CodegenMember(memberName, clazz, optionalTypeParam, @object);
        }

        public ICodegenBlock AddMethod(Type returnType, Type paramType, string paramName, Type generator)
        {
            string methodName = CodeGenerationIDGenerator.GenerateMethod();
            var method = new CodegenMethod(
                returnType, methodName, Collections.SingletonList(new CodegenNamedParam(paramType, paramName)),
                GetGeneratorDetail(generator));
            _methods.Add(method);
            return method.Statements;
        }

        public ICodegenBlock AddMethod(Type returnType, Type generator)
        {
            string methodName = CodeGenerationIDGenerator.GenerateMethod();
            var method = new CodegenMethod(
                returnType, methodName, Collections.GetEmptyList<CodegenNamedParam>(), GetGeneratorDetail(generator));
            _methods.Add(method);
            return method.Statements;
        }

        public IList<ICodegenMember> Members => _members;

        public IList<ICodegenMethod> Methods => _methods;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugEnabled { get; set; }

        private string GetGeneratorDetail(Type generator)
        {
            if (IsDebugEnabled)
            {
                var stackTrace = new StackTrace();
                var stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 3);
                var fileName = stackFrame.GetFileName();
                var lineNumber = stackFrame.GetFileLineNumber();
                var methodName = stackFrame.GetMethod().Name;

                return typeof(CodegenContext).FullName + " --- " + fileName + "." + methodName + "():" + lineNumber;
            }

            return typeof(CodegenContext).Name;
        }

        public ICodegenMethod CreateMethod(
            Type returnType, 
            string methodName,
            IList<CodegenNamedParam> paramList, 
            string optionalComment)
        {
            return new CodegenMethod(returnType, methodName, paramList, optionalComment);
        }

        public ICodegenClass CreateClass(
            string @namespace, 
            string className, 
            Type interfaceImplemented, 
            IList<ICodegenMember> members, 
            IList<ICodegenMethod> publicMethods,
            IList<ICodegenMethod> privateMethods)
        {
            return new CodegenClass(
                @namespace,
                className,
                interfaceImplemented,
                members,
                publicMethods,
                privateMethods
            );
        }
    }
} // end of namespace
