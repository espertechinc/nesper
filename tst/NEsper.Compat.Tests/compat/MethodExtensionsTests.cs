using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class MethodExtensionsTests
    {
        [Test]
        public void ExtensionMethodOfAssemblyLoadedAfterFirstLookupIsFound()
        {
            // Make sure the process-wide lookup has run before the assembly below exists.
            Assert.That(typeof(string).GetExtensionMethods("NoSuchExtensionMethod"), Is.Empty);

            var first = DefineLevelAssembly("First");
            AssertFindsOwnExtension(first);

            var second = DefineLevelAssembly("Second");
            AssertFindsOwnExtension(second);
            AssertFindsOwnExtension(first);
        }

        [Test]
        public void LookupDoesNotRetainCollectibleAssembly()
        {
            var reference = DefineAndLookUp();
            for (var i = 0; i < 10 && reference.IsAlive; i++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Assert.That(reference.IsAlive, Is.False, "collectible assembly was retained after lookup");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference DefineAndLookUp()
        {
            var levelType = DefineLevelAssembly("Collectible");
            AssertFindsOwnExtension(levelType);
            Assert.That(MethodExtensions.GetExtensionMethods().Any(m => m.Module.Assembly == levelType.Assembly), Is.False);
            return new WeakReference(levelType.Assembly);
        }

        private static void AssertFindsOwnExtension(Type levelType)
        {
            var methods = levelType.GetExtensionMethods("GetLevelCode").ToList();
            Assert.That(methods, Has.Count.EqualTo(1));
            Assert.That(methods[0].DeclaringType?.Assembly, Is.SameAs(levelType.Assembly));
            Assert.That(methods[0].Invoke(null, new[] { Enum.ToObject(levelType, 2) }), Is.EqualTo(2));
        }

        /// <summary>
        /// Defines, in a new collectible assembly, an enum MyLevel and a static class with the extension
        /// method int GetLevelCode(this MyLevel value), and returns the enum type.
        /// </summary>
        private static Type DefineLevelAssembly(string name)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("MethodExtensionsTests" + name + Guid.NewGuid().ToString("N")),
                AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Main");

            var enumBuilder = moduleBuilder.DefineEnum("MyLevel", TypeAttributes.Public, typeof(int));
            enumBuilder.DefineLiteral("LOW", 1);
            enumBuilder.DefineLiteral("MEDIUM", 2);
            enumBuilder.DefineLiteral("HIGH", 3);
            var levelType = enumBuilder.CreateType();

            var extensionAttribute = new CustomAttributeBuilder(
                typeof(ExtensionAttribute).GetConstructor(Type.EmptyTypes)!,
                Array.Empty<object>());
            var typeBuilder = moduleBuilder.DefineType(
                "MyLevelExtensions",
                TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
            typeBuilder.SetCustomAttribute(extensionAttribute);
            var methodBuilder = typeBuilder.DefineMethod(
                "GetLevelCode",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(int),
                new[] { levelType });
            methodBuilder.SetCustomAttribute(extensionAttribute);
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            typeBuilder.CreateType();

            return levelType;
        }
    }
}
