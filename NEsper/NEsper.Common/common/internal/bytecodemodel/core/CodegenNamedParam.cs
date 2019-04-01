///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenNamedParam
    {
        private readonly string typeName;

        public CodegenNamedParam(Type type, string name)
        {
            if (type == null) {
                throw new ArgumentException("Invalid null type");
            }

            Type = type;
            typeName = null;
            Name = name;
        }

        public CodegenNamedParam(string typeName, string name)
        {
            if (typeName == null) {
                throw new ArgumentException("Invalid null type");
            }

            Type = null;
            this.typeName = typeName;
            Name = name;
        }

        public CodegenNamedParam(Type type, CodegenExpressionRef name)
            : this(type, name.Ref)
        {
        }

        public Type Type { get; }

        public string Name { get; }

        public void Render(StringBuilder builder, IDictionary<Type, string> imports)
        {
            if (Type != null) {
                AppendClassName(builder, Type, null, imports);
            }
            else {
                builder.Append(typeName);
            }

            builder.Append(" ").Append(Name);
        }

        public static IList<CodegenNamedParam> From(Type typeOne, string nameOne)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(2);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            return result;
        }

        public static IList<CodegenNamedParam> From(Type typeOne, string nameOne, Type typeTwo, string nameTwo)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(2);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(3);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree, Type typeFour,
            string nameFour)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(4);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree, Type typeFour,
            string nameFour, Type typeFive, string nameFive)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(5);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            result.Add(new CodegenNamedParam(typeFive, nameFive));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree, Type typeFour,
            string nameFour, Type typeFive, string nameFive, Type typeSix, string nameSix)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(6);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            result.Add(new CodegenNamedParam(typeFive, nameFive));
            result.Add(new CodegenNamedParam(typeSix, nameSix));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree, Type typeFour,
            string nameFour, Type typeFive, string nameFive, Type typeSix, string nameSix, Type typeSeven,
            string nameSeven)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>();
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            result.Add(new CodegenNamedParam(typeFive, nameFive));
            result.Add(new CodegenNamedParam(typeSix, nameSix));
            result.Add(new CodegenNamedParam(typeSeven, nameSeven));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne, string nameOne, Type typeTwo, string nameTwo, Type typeThree, string nameThree, Type typeFour,
            string nameFour, Type typeFive, string nameFive, Type typeSix, string nameSix, Type typeSeven,
            string nameSeven, Type typeEight, string nameEight)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>();
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            result.Add(new CodegenNamedParam(typeFour, nameFour));
            result.Add(new CodegenNamedParam(typeFive, nameFive));
            result.Add(new CodegenNamedParam(typeSix, nameSix));
            result.Add(new CodegenNamedParam(typeSeven, nameSeven));
            result.Add(new CodegenNamedParam(typeEight, nameEight));
            return result;
        }

        public static void Render(
            StringBuilder builder, IList<CodegenNamedParam> @params, IDictionary<Type, string> imports)
        {
            var delimiter = "";
            foreach (var param in @params) {
                builder.Append(delimiter);
                param.Render(builder, imports);
                delimiter = ",";
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            classes.Add(Type);
        }

        public static void Render(
            StringBuilder builder, IDictionary<Type, string> imports, IList<CodegenNamedParam> @params)
        {
            var delimiter = "";
            foreach (var param in @params) {
                builder.Append(delimiter);
                param.Render(builder, imports);
                delimiter = ",";
            }
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var param = (CodegenNamedParam) o;

            if (!Type.Equals(param.Type)) {
                return false;
            }

            return Name.Equals(param.Name);
        }

        public override int GetHashCode()
        {
            int result = Type.GetHashCode();
            result = 31 * result + Name.GetHashCode();
            return result;
        }
    }
} // end of namespace