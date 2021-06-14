///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    /// Represents the INSTANCEOF(a,b,...) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprInstanceofNode : ExprNodeBase
    {
        private readonly string[] classIdentifiers;

        [NonSerialized] private ExprInstanceofNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="classIdentifiers">is a list of type names to check type for</param>
        public ExprInstanceofNode(string[] classIdentifiers)
        {
            this.classIdentifiers = classIdentifiers;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (this.ChildNodes.Length != 1) {
                throw new ExprValidationException(
                    "Instanceof node must have 1 child expression node supplying the expression to test");
            }

            if ((classIdentifiers == null) || (classIdentifiers.Length == 0)) {
                throw new ExprValidationException(
                    "Instanceof node must have 1 or more class identifiers to verify type against");
            }

            ISet<Type> classList = GetClassSet(classIdentifiers, validationContext.ImportService);
            Type[] classes;
            lock (this) {
                classes = classList.ToArray();
            }

            forge = new ExprInstanceofNodeForge(this, classes);
            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("instanceof(");
            this.ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(",");

            string delimiter = "";
            for (int i = 0; i < classIdentifiers.Length; i++) {
                writer.Write(delimiter);
                writer.Write(classIdentifiers[i]);
                delimiter = ",";
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprInstanceofNode)) {
                return false;
            }

            var other = (ExprInstanceofNode) node;
            if (Collections.AreEqual(other.classIdentifiers, classIdentifiers)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the list of class names or types to check instance of.
        /// </summary>
        /// <returns>class names</returns>
        public string[] ClassIdentifiers {
            get => classIdentifiers;
        }

        private ISet<Type> GetClassSet(
            string[] classIdentifiers,
            ImportServiceCompileTime importService)
        {
            ISet<Type> classList = new HashSet<Type>();
            foreach (string className in classIdentifiers) {
                Type clazz;

                // try the primitive names including "string"
                clazz = TypeHelper.GetPrimitiveTypeForName(className.Trim());
                if (clazz != null) {
                    classList.Add(clazz);
                    classList.Add(clazz.GetBoxedType());
                    continue;
                }

                // try to look up the class, not a primitive type name
                try {
                    clazz = TypeHelper.GetClassForName(className.Trim(), importService.ClassForNameProvider);
                }
                catch (TypeLoadException e) {
                    throw new ExprValidationException(
                        "Class as listed in instanceof function by name '" + className + "' cannot be loaded",
                        e);
                }

                // Add primitive and boxed types, or type itself if not built-in
                classList.Add(clazz.GetPrimitiveType());
                classList.Add(clazz.GetBoxedType());
            }

            return classList;
        }
    }
} // end of namespace