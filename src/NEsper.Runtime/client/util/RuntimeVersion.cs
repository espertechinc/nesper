///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>
    ///     Runtime version.
    /// </summary>
    public class RuntimeVersion
    {
        /// <summary>
        ///     Current runtime version.
        /// </summary>
        public const string RUNTIME_VERSION = "8.5.2";

        /// <summary>
        ///     Current runtime major version.
        /// </summary>
        public static readonly int MAJOR;

        /// <summary>
        ///     Current runtime major version.
        /// </summary>
        public static readonly int MINOR;

        /// <summary>
        ///     Current runtime patch version.
        /// </summary>
        public static readonly int PATCH;

        static RuntimeVersion()
        {
            var level = ParseVersion(RUNTIME_VERSION);
            MAJOR = level.Major;
            MINOR = level.Minor;
            PATCH = level.Patch;
        }

        /// <summary>
        ///     Compare major and minor version
        /// </summary>
        /// <param name="compilerVersion">compiler version to compare</param>
        public static void CheckVersion(string compilerVersion)
        {
            if (RUNTIME_VERSION == compilerVersion) {
                return;
            }

            MajorMinorPatch compiler;
            try {
                compiler = ParseVersion(compilerVersion);
            }
            catch (FormatException ex) {
                throw new VersionException(ex.Message, ex);
            }

            if (compiler.Major != MAJOR ||
                compiler.Minor != MINOR) {
                throw new VersionException(
                    "Major or minor version of compiler and runtime mismatch; The runtime version is " +
                    RUNTIME_VERSION +
                    " and the compiler version of the compiled unit is " +
                    compilerVersion);
            }
        }

        private static MajorMinorPatch ParseVersion(string version)
        {
            if (version == null || version.Trim().Length == 0) {
                throw new FormatException("Null or empty semantic version");
            }

            string[] split = version.Split('.');
            if (split.Length != 3) {
                throw new FormatException("Invalid semantic version '" + version + "'");
            }

            try {
                return new MajorMinorPatch(
                    Int32.Parse(split[0]), 
                    Int32.Parse(split[1]),
                    Int32.Parse(split[2]));
            }
            catch (Exception) {
                throw new FormatException("Invalid semantic version '" + version + "'");
            }
        }

        public class MajorMinorPatch
        {
            public MajorMinorPatch(
                int major,
                int minor,
                int patch)
            {
                Major = major;
                Minor = minor;
                Patch = patch;
            }

            public int Major { get; }

            public int Minor { get; }

            public int Patch { get; }
        }

        [Serializable]
        public class VersionException : Exception
        {
            public VersionException(string message) : base(message)
            {
            }

            public VersionException(
                string message,
                Exception innerException) : base(message, innerException)
            {
            }

            protected VersionException(SerializationInfo info,
                StreamingContext context) : base(info, context)
            {
            }
        }
    }
} // end of namespace