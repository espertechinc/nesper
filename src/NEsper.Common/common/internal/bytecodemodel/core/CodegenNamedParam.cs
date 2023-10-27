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
using com.espertech.esper.common.@internal.bytecodemodel.util;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenNamedParam
    {
        public Type Type { get; }

        public string TypeName { get; }

        public string Name { get; }

        public bool HasOutputModifier { get; set; }

        public CodegenNamedParam(
            Type type,
            string name)
        {
            HasOutputModifier = false;
            Type = type ?? throw new ArgumentException("Invalid null type");
            TypeName = null;
            Name = name;
        }

        public CodegenNamedParam(
            string typeName,
            string name)
        {
            HasOutputModifier = false;
            Type = null;
            TypeName = typeName ?? throw new ArgumentException("Invalid null type");
            Name = name;
        }

        public CodegenNamedParam(
            Type type,
            CodegenExpressionRef name)
            : this(type, name.Ref)
        {
        }

        public void Render(
            StringBuilder builder)
        {
            if (HasOutputModifier) {
                builder.Append("out ");
            }

            if (Type != null) {
                AppendClassName(builder, Type);
            }
            else {
                builder.Append(TypeName.CodeInclusionTypeName());
            }

            builder.Append(" ").Append(Name);
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne,
            string nameOne)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(2);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(2);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree)
        {
            IList<CodegenNamedParam> result = new List<CodegenNamedParam>(3);
            result.Add(new CodegenNamedParam(typeOne, nameOne));
            result.Add(new CodegenNamedParam(typeTwo, nameTwo));
            result.Add(new CodegenNamedParam(typeThree, nameThree));
            return result;
        }

        public static IList<CodegenNamedParam> From(
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree,
            Type typeFour,
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
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree,
            Type typeFour,
            string nameFour,
            Type typeFive,
            string nameFive)
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
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree,
            Type typeFour,
            string nameFour,
            Type typeFive,
            string nameFive,
            Type typeSix,
            string nameSix)
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
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree,
            Type typeFour,
            string nameFour,
            Type typeFive,
            string nameFive,
            Type typeSix,
            string nameSix,
            Type typeSeven,
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
            Type typeOne,
            string nameOne,
            Type typeTwo,
            string nameTwo,
            Type typeThree,
            string nameThree,
            Type typeFour,
            string nameFour,
            Type typeFive,
            string nameFive,
            Type typeSix,
            string nameSix,
            Type typeSeven,
            string nameSeven,
            Type typeEight,
            string nameEight)
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
            StringBuilder builder,
            IList<CodegenNamedParam> @params)
        {
            var delimiter = "";
            foreach (var param in @params) {
                builder.Append(delimiter);
                param.Render(builder);
                delimiter = ",";
            }
        }

        public void MergeClasses(ISet<Type> classes)
        {
            if (Type != null) {
                classes.AddToSet(Type);
            }
        }

        public static void Render(
            StringBuilder builder,
            IDictionary<Type, string> imports,
            IList<CodegenNamedParam> @params)
        {
            var delimiter = "";
            foreach (var param in @params) {
                builder.Append(delimiter);
                param.Render(builder);
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

            var param = (CodegenNamedParam)o;

            if (!Type.Equals(param.Type)) {
                return false;
            }

            return Name.Equals(param.Name);
        }

        public override int GetHashCode()
        {
            var result = Type.GetHashCode();
            result = 31 * result + Name.GetHashCode();
            return result;
        }

        public ParameterSyntax CodegenSyntaxAsParameter()
        {
            throw new NotSupportedException();
        }

        public CodegenNamedParam WithOutputModifier()
        {
            HasOutputModifier = true;
            return this;
        }
    }
} // end of namespace