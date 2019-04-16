///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Visibility modifiers for EPL objects.
    /// </summary>
    public class NameAccessModifier
    {
        /// <summary>
        /// Transient is used for non-visible objects that are only visible for the purpose of statement-internal processing.
        /// </summary>
        public static readonly NameAccessModifier TRANSIENT =
            new NameAccessModifier(false, true);

        /// <summary>
        /// Private is used for objects that may be used with the same module.
        /// </summary>
        public static readonly NameAccessModifier PRIVATE =
            new NameAccessModifier(true, true);

        /// <summary>
        /// Protected is used for objects that may be used with the modules of the same module name.
        /// </summary>
        public static readonly NameAccessModifier PROTECTED =
            new NameAccessModifier(true, false);

        /// <summary>
        /// Public is used for objects that may be used by other modules irrespective of module names.
        /// </summary>
        public static readonly NameAccessModifier PUBLIC =
            new NameAccessModifier(true, false);

        /// <summary>
        /// Preconfigured is used for objects that are preconfigured by configuration.
        /// </summary>
        public static readonly NameAccessModifier PRECONFIGURED =
            new NameAccessModifier(false, false);

        private readonly bool _isAccessModifier;
        private readonly bool _privateOrTransient;

        NameAccessModifier(
            bool isAccessModifier,
            bool privateOrTransient)
        {
            this._isAccessModifier = isAccessModifier;
            this._privateOrTransient = privateOrTransient;
        }

        /// <summary>
        /// Returns indicator whether the object is visible.
        /// <para />Always false if the object is private or transient.
        /// <para />Always true if the object is public or preconfigured.
        /// <para />For protected the module name must match
        /// </summary>
        /// <param name="objectVisibility">object visibility</param>
        /// <param name="objectModuleName">object module name</param>
        /// <param name="importModuleName">my module name</param>
        /// <returns>indicator</returns>
        public static bool Visible(
            NameAccessModifier objectVisibility,
            string objectModuleName,
            string importModuleName)
        {
            if (objectVisibility.IsPrivateOrTransient) {
                return false;
            }

            if (objectVisibility == NameAccessModifier.PROTECTED) {
                return CompareModuleName(objectModuleName, importModuleName);
            }

            return true;
        }

        /// <summary>
        /// Returns true if the modifier can be used by modules i.e. returns true for private, protected and public.
        /// Returns false for preconfigured since preconfigured is reserved for configured objects.
        /// Returns false for transient as transient is reserved for internal use
        /// </summary>
        /// <value>indicator</value>
        public bool IsModuleProvidedAccessModifier {
            get { return _isAccessModifier; }
        }

        /// <summary>
        /// Returns true for a public and protected and false for all others
        /// </summary>
        /// <value>indicator</value>
        public bool IsNonPrivateNonTransient {
            get { return !_privateOrTransient && this != PRECONFIGURED; }
        }

        /// <summary>
        /// Returns true for a private and transient and false for all others
        /// </summary>
        /// <value>indicator</value>
        public bool IsPrivateOrTransient {
            get { return _privateOrTransient; }
        }

        private static bool CompareModuleName(
            string objectModuleName,
            string importModuleName)
        {
            if (objectModuleName == null && importModuleName == null) {
                return true;
            }

            if (objectModuleName != null && importModuleName != null) {
                return objectModuleName.Equals(importModuleName);
            }

            return false;
        }
    }
} // end of namespace