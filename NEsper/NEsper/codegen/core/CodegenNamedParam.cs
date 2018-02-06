///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.codegen.core
{
    public class CodegenNamedParam
    {
        private readonly string _name;
        private readonly Type _type;

        public CodegenNamedParam(Type type, string name)
        {
            _type = type;
            _name = name;
        }

        public string Name => _name;

        public Type ParamType => _type;

        public void Render(TextWriter textWriter)
        {
            CodeGenerationHelper.AppendClassName(textWriter, _type, null);
            textWriter.Write(" ");
            textWriter.Write(_name);
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne,
            Type typeTwo, string nameTwo,
            Type typeThree, string nameThree,
            Type typeFour, string nameFour)
        {
            var result = new List<CodegenNamedParam>(4);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            return result;
        }

        public static IList<CodegenNamedParam> From(Type typeOne, string nameOne)
        {
            return Collections.SingletonList(new CodegenNamedParam(typeOne, nameOne));
        }

        public static void Render(TextWriter textWriter, IList<CodegenNamedParam> @params)
        {
            var delimiter = "";
            foreach (var param in @params)
            {
                textWriter.Write(delimiter);
                param.Render(textWriter);
                delimiter = ",";
            }
        }

        public void MergeClasses(ICollection<Type> classes)
        {
            classes.Add(_type);
        }
    }
} // end of namespace