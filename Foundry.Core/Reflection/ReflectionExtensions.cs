using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Reflection
{
    public static class ReflectionExtensions
    {
        private static Func<FieldInfo, bool> IsConstantDelegate = new Func<FieldInfo, bool>(IsConstant);

        /// <remarks>
        /// This is protected instead of an extension to <see cref="Type"/> since we need to ensure that the fieldInfos
        /// belong to <paramref name="type"/>. At the moment I am too lazy to implement that sort of checking, and it
        /// could potentially offset any performance gains.
        /// </remarks>
        private static IEnumerable<FieldInfo> FilterTypes(Type type, Type? ofType, IEnumerable<FieldInfo> fieldInfos, EMemberVisibilityFlags memberVisibility, bool declaredOnly)
        {
            var filtered = !declaredOnly
                ? fieldInfos
                : fieldInfos.Where(p => p.DeclaringType?.AssemblyQualifiedName?.Equals(type.AssemblyQualifiedName) ?? true);

            if (ofType != null)
            {
                filtered = filtered.Where(t => t.FieldType.IsTypeOf(ofType));
            }

            return memberVisibility switch
            {
                //TODO: static lambda in C#9
                EMemberVisibilityFlags.Protected => filtered.Where(p => p.IsFamily),
                EMemberVisibilityFlags.Private => filtered.Where(p => p.IsPrivate && !p.IsFamily),
                EMemberVisibilityFlags.Public | EMemberVisibilityFlags.Protected => filtered.Where(p => !p.IsPrivate),
                EMemberVisibilityFlags.Public | EMemberVisibilityFlags.Private => filtered.Where(p => p.IsPublic || p.IsPrivate),
                EMemberVisibilityFlags.Private | EMemberVisibilityFlags.Protected => filtered.Where(p => p.IsFamily || p.IsPrivate),
                EMemberVisibilityFlags.Public => filtered.Where(p => p.IsPublic),

                EMemberVisibilityFlags.All => filtered,
                _ => filtered
            };
        }

        /// <remarks>
        /// This is protected instead of an extension to <see cref="Type"/> since we need to ensure that the fieldInfos
        /// belong to <paramref name="type"/>. At the moment I am too lazy to implement that sort of checking, and it
        /// could potentially offset any performance gains.
        /// </remarks>
        private static IEnumerable<PropertyInfo> FilterTypes(Type type, Type? ofType, IEnumerable<PropertyInfo> fieldInfos, EMemberVisibilityFlags memberVisibility, bool declaredOnly)
        {
            var filtered = !declaredOnly
                ? fieldInfos
                : fieldInfos.Where(p => p.DeclaringType?.AssemblyQualifiedName?.Equals(type.AssemblyQualifiedName) ?? true);

            if (ofType != null)
            {
                filtered = filtered.Where(t => t.PropertyType.IsTypeOf(ofType));
            }

            return memberVisibility switch
            {
                //TODO: static lambda in C#9
                EMemberVisibilityFlags.Protected => filtered.Where(p => p.GetGetMethod()?.IsFamily ?? false),
                EMemberVisibilityFlags.Private => filtered.Where(p => p.GetGetMethod().IsPrivate && !p.GetGetMethod().IsFamily),
                EMemberVisibilityFlags.Public | EMemberVisibilityFlags.Protected => filtered.Where(p => !p.GetGetMethod().IsPrivate),
                EMemberVisibilityFlags.Public | EMemberVisibilityFlags.Private => filtered.Where(p => p.GetGetMethod().IsPublic || p.GetGetMethod().IsPrivate),
                EMemberVisibilityFlags.Private | EMemberVisibilityFlags.Protected => filtered.Where(p => p.GetGetMethod().IsFamily || p.GetGetMethod().IsPrivate),
                EMemberVisibilityFlags.Public => filtered.Where(p => p.GetGetMethod().IsPublic),
                EMemberVisibilityFlags.All => filtered,
                _ => filtered
            };
        }

        /// <summary>
        /// Gets all async void methods and lambdas in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to search.</param>
        public static IEnumerable<MethodInfo> GetAsyncVoidMethods(this Assembly assembly)
        {
            Guard.IsNotNull(assembly, nameof(assembly));

            return assembly.GetLoadableTypes()
                           .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                                               BindingFlags.Instance | BindingFlags.Static |
                                                               BindingFlags.DeclaredOnly))
                           .Where(method => method.HasAttribute<AsyncStateMachineAttribute>() &&
                                            method.ReturnType == typeof(void));
        }

        /// <summary>
        /// Gets the property of an <see cref="Attribute"/> on this reference. If the attribute
        /// does not exist, the default value of <typeparamref name="TValue"/> is returned.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="expression">The transform expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [return: MaybeNull]
        public static TValue GetAttributeValue<TAttribute, TValue>(this Assembly assembly, Func<TAttribute, TValue> expression)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(assembly, nameof(assembly));
            Guard.IsNotNull(expression, nameof(expression));

            var attribute = assembly.GetCustomAttributes<TAttribute>().SingleOrDefault();
            return attribute is null ? default : expression(attribute);
        }

        /// <summary>
        /// Gets the property of an <see cref="Attribute"/> on this reference. If the attribute
        /// does not exist, the default value of <typeparamref name="TValue"/> is returned.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="expression">The transform expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/></exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [return: MaybeNull]
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> expression)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(expression, nameof(expression));

            var attribute = type.GetCustomAttributes<TAttribute>().SingleOrDefault();
            return attribute is null ? default : expression(attribute);
        }

        /// <summary>
        /// Gets the property of an <see cref="Attribute"/> on this reference. If the attribute
        /// does not exist, the default value of <typeparamref name="TValue"/> is returned.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="expression">The transform expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/></exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [return: MaybeNull]
        public static TValue GetAttributeValue<TAttribute, TValue>(this MemberInfo member, Func<TAttribute, TValue> expression)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(member, nameof(member));
            Guard.IsNotNull(expression, nameof(expression));

            var attribute = member.GetCustomAttributes<TAttribute>().SingleOrDefault();
            return attribute is null ? default : expression(attribute);
        }

        /// <summary>
        /// Returns the public constants of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants(this Type type)
        {
            return GetConstants(type, null, EMemberVisibilityFlags.Public, true);
        }

        /// <summary>
        /// Returns the public constants of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="declaredOnly">Whether to only return constants declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants(this Type type, bool declaredOnly)
        {
            return GetConstants(type, null, EMemberVisibilityFlags.Public, declaredOnly);
        }

        /// <summary>
        /// Returns the constants of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired constants. This is a flag field.</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants(this Type type, EMemberVisibilityFlags memberVisibility)
        {
            return GetConstants(type, null, memberVisibility, true);
        }

        /// <summary>
        /// Returns the constants of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ofType">Return constants only of this type.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired constants. This is a flag field.</param>
        /// <param name="declaredOnly">Whether to only return constants declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants(this Type type, Type? ofType, EMemberVisibilityFlags memberVisibility, bool declaredOnly)
        {
            Guard.IsNotNull(type, nameof(type));
            GuardEx.IsValid(memberVisibility, nameof(memberVisibility));

            var visibility = ((memberVisibility & EMemberVisibilityFlags.Public) > 0 ? BindingFlags.Public : 0) |
                             ((memberVisibility & (EMemberVisibilityFlags.Protected | EMemberVisibilityFlags.Private)) > 0 ? BindingFlags.NonPublic : 0);

            var fields = type.GetFields(visibility | BindingFlags.Static).Where(IsConstantDelegate).ToList();
            if (!declaredOnly)
            {
                var parent = type;
                while ((parent = parent.BaseType) != null && parent != typeof(object))
                {
                    fields.AddRange(parent.GetFields(visibility | BindingFlags.Static).Where(IsConstantDelegate));
                }
            }

            return FilterTypes(type, ofType, fields, memberVisibility, declaredOnly);
        }

        /// <summary>
        /// Returns the public constants of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return constants only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants<TOfType>(this Type type)
        {
            return GetConstants(type, typeof(TOfType), EMemberVisibilityFlags.Public, true);
        }

        /// <summary>
        /// Returns the public constants of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return constants only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <param name="declaredOnly">Whether to only return constants declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants<TOfType>(this Type type, bool declaredOnly)
        {
            return GetConstants(type, typeof(TOfType), EMemberVisibilityFlags.Public, declaredOnly);
        }

        /// <summary>
        /// Returns the constants of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return constants only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired constants. This is a flag field.</param>
        /// <returns>The constants of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<FieldInfo> GetConstants<TOfType>(this Type type, EMemberVisibilityFlags memberVisibility)
        {
            return GetConstants(type, typeof(TOfType), memberVisibility, true);
        }

        /// <summary>
        /// Returns the default value of a type. Runtime version of <see langword="default"/>(T).
        /// </summary>
        /// <exception cref="TargetInvocationException">The constructor being called throws an exception.</exception>
        /// <exception cref="MethodAccessException">
        ///  In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912) or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md), catch the base class exception, <see cref="MemberAccessException"></see>, instead.
        ///  The caller does not have permission to call this constructor.</exception>
        /// <exception cref="MemberAccessException">Cannot create an instance of an abstract class, or this member was invoked with a late-binding mechanism.</exception>
        /// <exception cref="MissingMethodException">
        ///  In the [.NET for Windows Store apps](http://go.microsoft.com/fwlink/?LinkID=247912) or the [Portable Class Library](~/docs/standard/cross-platform/cross-platform-development-with-the-portable-class-library.md), catch the base class exception, <see cref="MissingMemberException"></see>, instead.
        ///  No matching public constructor was found.</exception>
        /// <exception cref="TypeLoadException"><paramref name="type">type</paramref> is not a valid type.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static object? GetDefault(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            return !type.IsValueType ? null : Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns the public default (parameterless) constructor for the specified type.
        /// If the type does not have a public parameterless constructor, null is returned.
        /// </summary>
        /// <param name="type">The type to get the constructor for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            return type.GetConstructor(Type.EmptyTypes);
        }

        /// <summary>
        /// Returns the <see cref="DescriptionAttribute.Description"/> of the <see cref="DescriptionAttribute"/>
        /// decorating this enum member, if one exists.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static string? GetDescription<T>(this T enumValue)
            where T : struct, Enum
        {
            GuardEx.IsValid(enumValue, nameof(enumValue));

            return enumValue.GetMember().GetAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// Returns the <see cref="DescriptionAttribute.Description"/> of the <see cref="DescriptionAttribute"/>
        /// decorating this <see cref="MemberInfo"/>, if one exists.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/>.</param>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static string? GetDescription(this MemberInfo member)
        {
            Guard.IsNotNull(member, nameof(member));

            return member.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// Returns the <see cref="DescriptionAttribute.Description"/> of the <see cref="DescriptionAttribute"/>
        /// decorating this <see cref="Type"/>, if one exists.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <exception cref="AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static string? GetDescription(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            return type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }

        /// <summary>
        /// Obtains all extension methods for <typeparamref name="TForType"/> in the assembly.
        /// </summary>
        /// <typeparam name="TForType">The type to obtain extension methods for.</typeparam>
        /// <param name="assembly">The assembly to look in.</param>
        public static IEnumerable<MethodInfo> GetExtensionMethods<TForType>(this Assembly assembly)
            => GetExtensionMethods(assembly, typeof(TForType));

        /// <summary>
        /// Obtains all extension methods for <paramref name="forType"/> in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to look in.</param>
        /// <param name="forType">The type to obtain extension methods for.</param>
        /// <remarks>
        /// https://stackoverflow.com/a/299526
        /// </remarks>
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Assembly assembly, Type forType)
        {
            Guard.IsNotNull(assembly, nameof(assembly));
            Guard.IsNotNull(forType, nameof(forType));

            return from type in assembly.GetTypes()
                   where type.IsSealed && !type.IsGenericType && !type.IsNested
                   from method in type.GetMethods(BindingFlags.Static
                       | BindingFlags.Public | BindingFlags.NonPublic)
                   where method.IsDefined(typeof(ExtensionAttribute), false)
                   where method.GetParameters()[0].ParameterType == forType
                   select method;
        }

        /// <summary>
        /// Returns a collection of <see cref="Type"/> objects that represent the type arguments of a closed
        /// generic type or the type parameters of a generic type definition.
        /// Returns the generic arguments for this type. If no generic arguments are
        /// found on this type, the base type will be searched instead, up to <paramref name="depth"/>
        /// base types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="depth">The maximum number of base types to look at before giving up.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static IEnumerable<Type> GetGenericArguments(this Type type, int depth)
        {
            Guard.IsNotNull(type, nameof(type));

            bool searchEntireHierarchy = depth < 0;
            var genericArguments = Array.Empty<Type>();
            var currentType = type;
            while (currentType != null && genericArguments.Length == 0 && (searchEntireHierarchy || depth >= 0))
            {
                // We can't go higher than this.
                if (currentType == typeof(object))
                {
                    break;
                }

                genericArguments = currentType.GetGenericArguments();
                currentType = currentType.BaseType;
                --depth;
            }

            return genericArguments;
        }

        /// <summary>
        /// Returns a collection of <see cref="Type"/> objects that represent the type arguments of a closed
        /// generic type or the type parameters of a generic type definition.
        /// Returns the generic arguments for this type. If no generic arguments are
        /// found on this type, the base type will be searched instead.
        /// base types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="parentGenericType">The parent type to get the generic arguments of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static IEnumerable<Type> GetGenericArguments(this Type type, Type parentGenericType)
            => GetGenericArguments(type, parentGenericType, -1);

        /// <summary>
        /// Returns a collection of <see cref="Type"/> objects that represent the type arguments of a closed
        /// generic type or the type parameters of a generic type definition.
        /// Returns the generic arguments for this type. If no generic arguments are
        /// found on this type, the base type will be searched instead, up to <paramref name="depth"/>
        /// base types.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="parentGenericType">The parent type to get the generic arguments of.</param>
        /// <param name="depth">The maximum number of base types to look at before giving up.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="parentGenericType"/> is not a generic type definition.</exception>
        public static IEnumerable<Type> GetGenericArguments(this Type type, Type parentGenericType, int depth)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(parentGenericType, nameof(parentGenericType));

            if (!parentGenericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Parameter must be a generic type definition.", nameof(parentGenericType));
            }

            bool searchEntireHierarchy = depth < 0;
            var genericArguments = Array.Empty<Type>();
            var currentType = type;
            while (currentType != null && genericArguments.Length == 0 && (searchEntireHierarchy || depth >= 0))
            {
                // We can't go higher than this.
                if (currentType == typeof(object))
                {
                    break;
                }

                if (!currentType.IsGenericType ||
                    (currentType.IsGenericType && currentType.GetGenericTypeDefinition() != parentGenericType))
                {
                    currentType = currentType.BaseType;
                    --depth;
                    continue;
                }

                genericArguments = currentType.GetGenericArguments();
                break;
            }

            return genericArguments;
        }

        /// <summary>
        /// Returns the first generic type in the class hierarchy that matches
        /// the specified generic type definition.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="genericTypeDefinition">The type definition to look for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentException"><paramref name="genericTypeDefinition"/> is not a generic type definition.</exception>
        public static Type? GetGenericTypeFromDefinition(this Type type, Type genericTypeDefinition)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(genericTypeDefinition, nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"{nameof(genericTypeDefinition)} must be a generic type definition.");
            }

            var currentType = type;
            while (currentType != null)
            {
                // We can't go higher than this.
                if (currentType == typeof(object))
                {
                    break;
                }

                if (currentType.IsGenericType &&
                    currentType.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    return currentType;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Gets the loadable types from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            Guard.IsNotNull(assembly, nameof(assembly));

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(type => type != null);
            }
        }

        /// <summary>
        /// Obtains the <see cref="MemberInfo"/> of this enum value. If this enum value
        /// represents multiple flags, <see langword="null" /> is returned.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value to get the <see cref="MemberInfo"/> of.</param>
        public static MemberInfo? GetMemberInfo<T>(this T enumValue)
            where T : struct, Enum
        {
            GuardEx.IsValid(enumValue, nameof(enumValue));

            var enumType = typeof(T).GetTypeInfo();

            var possibleMembers = enumType.GetMember(enumValue.ToString());
            return possibleMembers.Length > 0 ? possibleMembers[0] : null;
        }

        /// <summary>
        /// Returns the underlying type of the <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to get the underlying type of.</param>
        /// <exception cref="ArgumentException">MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            Guard.IsNotNull(memberInfo, nameof(memberInfo));

            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                MemberTypes.Event => ((EventInfo)memberInfo).EventHandlerType,
                _ => throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", nameof(memberInfo))
            };
        }

        /// <summary>
        /// Returns <paramref name="type"/> if it is not wrapped in a <see cref="Nullable{T}"/>,
        /// else the underlying type of the <see cref="Nullable{T}"/> is returned.
        /// </summary>
        /// <param name="type">The type to get the underlying type of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static Type GetNullableUnderlyingType(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            var t = Nullable.GetUnderlyingType(type);
            return t ?? type;
        }

        /// <summary>
        /// Returns the public or non-public get accessor for this property.
        /// If not found, the property of the declaring type will be used instead
        /// if <paramref name="searchDeclaringType"/> is true.
        /// </summary>
        /// <param name="property">The property to get the getter of.</param>
        /// <param name="nonPublic">Whether to return a non-public getter if one exists.</param>
        /// <param name="searchDeclaringType">Whether to search the property of the declaring type if no getter is found on the property.</param>
        public static MethodInfo? GetGetMethod(this PropertyInfo property, bool nonPublic, bool searchDeclaringType)
        {
            Guard.IsNotNull(property, nameof(property));

            var setter = property.GetGetMethod(nonPublic);
            if (!(setter is null))
            {
                return setter;
            }

            if (searchDeclaringType)
            {
                var parent = property.DeclaringType;
                if (parent is null)
                {
                    return null;
                }

                property = parent.GetProperty(property.Name);
                return property?.GetGetMethod(nonPublic);
            }

            return null;
        }

        /// <summary>
        /// Returns the public or non-public set accessor for this property.
        /// If not found, the property of the declaring type will be used instead
        /// if <paramref name="searchDeclaringType"/> is true.
        /// </summary>
        /// <param name="property">The property to get the setter of.</param>
        /// <param name="nonPublic">Whether to return a non-public setter if one exists.</param>
        /// <param name="searchDeclaringType">Whether to search the property of the declaring type if no setter is found on the property.</param>
        public static MethodInfo? GetSetMethod(this PropertyInfo property, bool nonPublic, bool searchDeclaringType)
        {
            Guard.IsNotNull(property, nameof(property));

            var setter = property.GetSetMethod(nonPublic);
            if (!(setter is null))
            {
                return setter;
            }

            if (searchDeclaringType)
            {
                var parent = property.DeclaringType;
                if (parent is null)
                {
                    return null;
                }

                property = parent.GetProperty(property.Name);
                return property?.GetSetMethod(nonPublic);
            }

            return null;
        }

        /// <summary>
        /// Returns the public statics of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics(this Type type)
        {
            return GetStatics(type, null, EMemberVisibilityFlags.Public, true);
        }

        /// <summary>
        /// Returns the public statics of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="declaredOnly">Whether to only return statics declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics(this Type type, bool declaredOnly)
        {
            return GetStatics(type, null, EMemberVisibilityFlags.Public, declaredOnly);
        }

        /// <summary>
        /// Returns the statics of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired statics. This is a flag field.</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics(this Type type, EMemberVisibilityFlags memberVisibility)
        {
            return GetStatics(type, null, memberVisibility, true);
        }

        /// <summary>
        /// Returns the statics of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ofType">Return statics only of this type.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired statics. This is a flag field.</param>
        /// <param name="declaredOnly">Whether to only return statics declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics(this Type type, Type? ofType, EMemberVisibilityFlags memberVisibility, bool declaredOnly)
        {
            Guard.IsNotNull(type, nameof(type));
            GuardEx.IsValid(memberVisibility, nameof(memberVisibility));

            var visibility = ((memberVisibility & EMemberVisibilityFlags.Public) > 0 ? BindingFlags.Public : 0) |
                             ((memberVisibility & (EMemberVisibilityFlags.Protected | EMemberVisibilityFlags.Private)) > 0 ? BindingFlags.NonPublic : 0);

            var memberInfos = type.GetMembers(visibility | BindingFlags.Static).ToList();
            if (!declaredOnly)
            {
                var parent = type;
                while ((parent = parent.BaseType) != null && parent != typeof(object))
                {
                    memberInfos.AddRange(parent.GetFields(visibility | BindingFlags.Static));
                }
            }

            var fieldInfos = memberInfos.Where(m => m.MemberType == MemberTypes.Field).Cast<FieldInfo>();
            var propertyInfos = memberInfos.Where(m => m.MemberType == MemberTypes.Property).Cast<PropertyInfo>();

            return FilterTypes(type, ofType, fieldInfos, memberVisibility, declaredOnly).Where(f => !f.IsConstant()).Cast<MemberInfo>().Concat(FilterTypes(type, ofType, propertyInfos, memberVisibility, declaredOnly));
        }

        /// <summary>
        /// Returns the public statics of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return statics only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics<TOfType>(this Type type)
        {
            return GetStatics(type, typeof(TOfType), EMemberVisibilityFlags.Public, true);
        }

        /// <summary>
        /// Returns the public statics of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return statics only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <param name="declaredOnly">Whether to only return statics declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics<TOfType>(this Type type, bool declaredOnly)
        {
            return GetStatics(type, typeof(TOfType), EMemberVisibilityFlags.Public, declaredOnly);
        }

        /// <summary>
        /// Returns the statics of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return statics only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired statics. This is a flag field.</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics<TOfType>(this Type type, EMemberVisibilityFlags memberVisibility)
        {
            return GetStatics(type, typeof(TOfType), memberVisibility, true);
        }

        /// <summary>
        /// Returns the statics of the specified type that are of type <typeparamref name="TOfType"/>.
        /// </summary>
        /// <typeparam name="TOfType">Return statics only of this type.</typeparam>
        /// <param name="type">The type to get the members of.</param>
        /// <param name="memberVisibility">The <see cref="EMemberVisibilityFlags"/> of the desired statics. This is a flag field.</param>
        /// <param name="declaredOnly">Whether to only return statics declared in that <see cref="Type"/> (and not parent <see cref="Type"/>s).</param>
        /// <returns>The statics of a <see cref="Type"/>.</returns>
        /// <remarks>
        /// This method is extremely expensive. Cache the results where possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        public static IEnumerable<MemberInfo> GetStatics<TOfType>(this Type type, EMemberVisibilityFlags memberVisibility, bool declaredOnly)
        {
            return GetStatics(type, typeof(TOfType), memberVisibility, declaredOnly);
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of the object. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException"><paramref name="memberInfo"/> is not of a static member.</exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in index does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static object GetValue(this MemberInfo memberInfo)
        {
            return GetValue(memberInfo, null, null);
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <param name="forObject">The object to get the value from.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of <paramref name="forObject"/>. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and <paramref name="forObject"/> is <see langword="null" />. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in index does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static object GetValue(this MemberInfo memberInfo, object? forObject)
        {
            return GetValue(memberInfo, forObject, null);
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <param name="forObject">The object to get the value from.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of <paramref name="forObject"/>. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and <paramref name="forObject"/> is <see langword="null" />. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in <paramref name="index" /> does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static object GetValue(this MemberInfo memberInfo, object? forObject, object[]? index)
        {
            Guard.IsNotNull(memberInfo, nameof(memberInfo));

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fi = (FieldInfo)memberInfo;
                    if (fi.IsConstant())
                    {
                        return fi.GetRawConstantValue();
                    }
                    return fi.GetValue(forObject);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(forObject, index);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in <see langword="null"/> does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and forObject is <see langword="null" />. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of forObject. </exception>
        /// <exception cref="ArgumentNullException">memberInfo is <see langword="null"/></exception>
        public static T GetValue<T>(this MemberInfo memberInfo)
        {
            return (T)GetValue(memberInfo, null);
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <param name="forObject">The object to get the value from.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in <see langword="null"/> does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and forObject is <see langword="null" />. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of forObject. </exception>
        /// <exception cref="ArgumentNullException">memberInfo is <see langword="null"/></exception>
        public static T GetValue<T>(this MemberInfo memberInfo, object? forObject, object[]? index)
        {
            return (T)GetValue(memberInfo, forObject, index);
        }

        /// <summary>
        /// Gets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="memberInfo">The member to get the value of.</param>
        /// <param name="forObject">The object to get the value from.</param>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in <see langword="null"/> does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and forObject is <see langword="null" />. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of forObject. </exception>
        /// <exception cref="ArgumentNullException">memberInfo is <see langword="null"/></exception>
        public static T GetValue<T>(this MemberInfo memberInfo, object? forObject)
        {
            return (T)GetValue(memberInfo, forObject);
        }

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        [DebuggerHidden, DebuggerStepThrough]
        public static bool HasAttribute<TAttribute>(this Type type, bool inherit = false)
            where TAttribute : Attribute => HasAttribute(type, typeof(TAttribute), inherit);

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        [DebuggerHidden, DebuggerStepThrough]
        public static bool HasAttribute(this Type type, Type attributeType, bool inherit = false)
            => HasAttribute(type, attributeType, out var _, inherit);

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        [DebuggerHidden, DebuggerStepThrough]
        public static bool HasAttribute<TAttribute>(this Type type, [NotNullWhen(true)] out TAttribute? attribute)
            where TAttribute : Attribute => HasAttribute(type, false, out attribute);

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        [DebuggerHidden, DebuggerStepThrough]
        public static bool HasAttribute<TAttribute>(this Type type, bool inherit, [NotNullWhen(true)] out TAttribute? attribute)
            where TAttribute : Attribute
        {
            if (HasAttribute(type, typeof(TAttribute), out var attr, inherit))
            {
                attribute = (TAttribute)attr;
                return true;
            }

            attribute = default;
            return false;
        }

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this Type type, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
            => HasAttribute(type, attributeType, false, out attribute);

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this Type type, Type attributeType, bool inherit, [NotNullWhen(true)] out Attribute? attribute)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            attribute = null;

            var attributes = type.GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();
            if (attributes.Length > 0)
            {
                attribute = attributes[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributes">Contains the attributes found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttributes<TAttribute>(this Type type, [NotNullWhen(true)] out TAttribute[]? attributes)
            where TAttribute : Attribute => HasAttributes(type, false, out attributes);

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <param name="attributes">Contains the attributes found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttributes<TAttribute>(this Type type, bool inherit, [NotNullWhen(true)] out TAttribute[]? attributes)
            where TAttribute : Attribute
        {
            if (HasAttributes(type, typeof(TAttribute), inherit, out var a))
            {
                attributes = a.Cast<TAttribute>().ToArray();
                return true;
            }

            attributes = default;
            return false;
        }

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attributes">Contains the attributes found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttributes(this Type type, Type attributeType, [NotNullWhen(true)] out Attribute[]? attributes)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            attributes = type.GetCustomAttributes(attributeType).ToArray();
            if (attributes.Length < 1)
            {
                attributes = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the provided type has the specified attribute.
        /// </summary>
        /// <param name="type">The type to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <param name="attributes">Contains the attributes found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasAttributes(this Type type, Type attributeType, bool inherit, [NotNullWhen(true)] out Attribute[]? attributes)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            attributes = type.GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();
            if (attributes.Length < 1)
            {
                attributes = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the provided enum has the specified attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static bool HasAttribute<TEnum, TAttribute>(this TEnum enumValue)
            where TEnum : struct, Enum
            where TAttribute : Attribute
        {
            return HasAttribute<TEnum, TAttribute>(enumValue, out _);
        }

        /// <summary>
        /// Returns whether the provided enum has the specified attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static bool HasAttribute<TEnum>(this TEnum enumValue, Type attributeType)
            where TEnum : struct, Enum
        {
            return HasAttribute(enumValue, attributeType, out _);
        }

        /// <summary>
        /// Returns whether the provided enum has the specified attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static bool HasAttribute<TEnum, TAttribute>(this TEnum enumValue, [NotNullWhen(true)] out TAttribute? attribute)
            where TEnum : struct, Enum
            where TAttribute : Attribute
        {
            return HasAttribute(typeof(TEnum).GetMember(enumValue.ToString()).Single(), out attribute);
        }

        /// <summary>
        /// Returns whether the provided enum has the specified attribute.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static bool HasAttribute<TEnum>(this TEnum enumValue, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
            where TEnum : struct, Enum
        {
            return HasAttribute(typeof(TEnum).GetMember(enumValue.ToString()).Single(), attributeType, out attribute);
        }

        /// <summary>
        /// Returns whether the provided member has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="member">The member to check the attributes of.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/></exception>
        public static bool HasAttribute<TAttribute>(this MemberInfo member, bool inherit = false)
            where TAttribute : Attribute
        {
            return HasAttribute<TAttribute>(member, out _, inherit);
        }

        /// <summary>
        /// Returns whether the provided member has the specified attribute.
        /// </summary>
        /// <param name="member">The member to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this MemberInfo member, Type attributeType, bool inherit = false)
        {
            return HasAttribute(member, attributeType, out _, inherit);
        }

        /// <summary>
        /// Returns whether the provided member has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="member">The member to check the attributes of.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/></exception>
        public static bool HasAttribute<TAttribute>(this MemberInfo member, [NotNullWhen(true)] out TAttribute? attribute, bool inherit = false)
            where TAttribute : Attribute
        {
            if (HasAttribute(member, typeof(TAttribute), out var attr, inherit))
            {
                attribute = (TAttribute)attr;
                return true;
            }

            attribute = default;
            return false;
        }

        /// <summary>
        /// Returns whether the provided member has the specified attribute.
        /// </summary>
        /// <param name="member">The member to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <param name="inherit">Whether to search the inheritance chain for the attribute.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this MemberInfo member, Type attributeType, [NotNullWhen(true)] out Attribute? attribute, bool inherit = false)
        {
            Guard.IsNotNull(member, nameof(member));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            var attributes = member.GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();
            if (attributes.Length > 0)
            {
                attribute = attributes[0];
                return true;
            }

            attribute = default;
            return false;
        }

        /// <summary>
        /// Returns whether the provided member has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="member">The member to check the attributes of.</param>
        /// <param name="attributes">Contains the attributes found of the specified type.</param>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/></exception>
        public static bool HasAttributes<TAttribute>([NotNull] this MemberInfo member, [NotNullWhen(true)] out TAttribute[]? attributes)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(member, nameof(member));

            attributes = member.GetCustomAttributes<TAttribute>().ToArray();
            if (attributes.Length < 1)
            {
                attributes = default;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttribute<TAttribute>(this Assembly assembly)
            where TAttribute : Attribute => HasAttribute(assembly, typeof(TAttribute));

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this Assembly assembly, Type attributeType)
        {
            Guard.IsNotNull(assembly, nameof(assembly));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            return assembly.GetCustomAttributes(attributeType).Any();
        }

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttribute<TAttribute>(this Assembly assembly, [NotNullWhen(true)] out TAttribute? attribute)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(assembly, nameof(assembly));

            var attributes = assembly.GetCustomAttributes<TAttribute>().ToArray();
            if (attributes.Length > 0)
            {
                attribute = attributes[0];
                return true;
            }

            attribute = default;
            return false;
        }

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attribute">Contains the first attribute found of the specified type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttribute(this Assembly assembly, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
        {
            Guard.IsNotNull(assembly, nameof(assembly));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            var attributes = assembly.GetCustomAttributes(attributeType).ToArray();
            if (attributes.Length > 0)
            {
                attribute = attributes[0];
                return true;
            }

            attribute = null;
            return false;
        }

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check for.</typeparam>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <param name="attributes">Contains the attribute founds of the specified type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttributes<TAttribute>(this Assembly assembly, [NotNullWhen(true)] out TAttribute[]? attributes)
            where TAttribute : Attribute
        {
            Guard.IsNotNull(assembly, nameof(assembly));

            attributes = assembly.GetCustomAttributes<TAttribute>().ToArray();
            if (attributes.Length < 1)
            {
                attributes = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the provided assembly has the specified attribute.
        /// </summary>
        /// <param name="assembly">The assembly to check the attributes of.</param>
        /// <param name="attributeType">The attribute to check for.</param>
        /// <param name="attributes">Contains the attribute founds of the specified type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool HasAttributes(this Assembly assembly, Type attributeType, [NotNullWhen(true)] out Attribute[]? attributes)
        {
            Guard.IsNotNull(assembly, nameof(assembly));
            Guard.IsNotNull(attributeType, nameof(attributeType));

            attributes = assembly.GetCustomAttributes(attributeType).ToArray();
            if (attributes.Length < 1)
            {
                attributes = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the type has a default parameterless constructor.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool HasDefaultConstructor(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Returns whether the type implements the interface type of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="type">The type to check the interface declaration of.</param>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an interface type.</exception>
        public static bool ImplementsInterface<T>(this Type type)
            where T : class => ImplementsInterface(type, typeof(T));

        /// <summary>
        /// Returns whether the type implements the interface type of the type defined in
        /// <paramref name="interfaceType"/>.
        /// </summary>
        /// <param name="type">The type to check the interface declaration of.</param>
        /// <param name="interfaceType">The interface type.</param>
        /// <exception cref="ArgumentException"><paramref name="interfaceType"/> is not an interface type.</exception>
        public static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(interfaceType, nameof(interfaceType));

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"Type must be an interface type.", nameof(interfaceType));
            }

            return type.GetInterfaces().Contains(interfaceType);
        }

        /// <summary>
        /// Returns whether the given type is an anonymous class type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true" /> if the given type is an anonymous class type, <see langword="false" /> otherwise.</returns>
        /// <remarks>There is no official support for this, and the generated code could change at any time.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        /// <exception cref="TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public static bool IsAnonymousClass(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            if (!type.HasAttribute<CompilerGeneratedAttribute>() || !type.Name.StartsWith("<>") || !type.IsClass)
            {
                return false;
            }

            return TypeDescriptor.GetProperties(type).Cast<PropertyDescriptor>().All(p => p.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes));
        }

        /// <summary>
        /// Returns whether this <see cref="FieldInfo"/> represents a constant value.
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <exception cref="ArgumentNullException"><paramref name="fieldInfo"/> is <see langword="null"/></exception>
        public static bool IsConstant(this FieldInfo fieldInfo)
        {
            Guard.IsNotNull(fieldInfo, nameof(fieldInfo));

            return fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
        }

        /// <summary>
        /// Returns whether this <see cref="Assembly"/> is compiled in DEBUG mode.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/></exception>
        public static bool IsDebugAssembly(this Assembly assembly)
        {
            Guard.IsNotNull(assembly, nameof(assembly));

            try
            {
                return assembly.GetCustomAttribute<DebuggableAttribute>()?.IsJITTrackingEnabled ?? false;
            }
            catch (AmbiguousMatchException)
            {
                return true;
            }
        }

        /// <summary>
        /// Returns whether this object is the default value for its type.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to check.</param>
        public static bool IsDefaultValue<T>([AllowNull, NotNullWhen(false)] this T obj)
            => IsDefaultValue(obj, false);

        /// <summary>
        /// Returns whether this object is the default value for its type.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object to check.</param>
        /// <param name="nullableIsDefault">Whether nullable values are considered non-default.</param>
        public static bool IsDefaultValue<T>([AllowNull, NotNullWhen(false)] this T obj, bool nullableIsDefault)
        {
            // Class types are an easy null check.
            if (obj is null || Equals(obj, default(T)))
            {
                return true;
            }

            if (!typeof(T).IsValueType)
            {
                return false;
            }

            // Nullables 
            var methodType = typeof(T);
            if (methodType.IsNullable())
            {
                if (nullableIsDefault)
                {
                    return false;
                }

                object underlyingValue = typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(methodType)).GetProperty(nameof(Nullable<int>.Value)).GetGetMethod().Invoke(obj, null);
                return IsDefaultValue(underlyingValue, nullableIsDefault);
            }

            // Value types. We need them unboxed.
            // T may be an interface type which is a reference type, but the underlying type
            // is a value type. We need the real underlying type here, so we cannot reuse
            // the Type variable above.
            var argumentType = obj.GetType();
            if (argumentType.IsValueType && argumentType != methodType)
            {
                // Value types all have a default constructor. Making the objects
                // may not be cheap, but there is nothing we can do about that.
                object o = Activator.CreateInstance(argumentType);
                return obj.Equals(o);
            }

            return false;
        }

        /// <summary>
        /// Returns whether the given type is a floating point type.
        /// </summary>
        public static bool IsFloatingPointType(this Type? type)
        {
            return Number.IsFloatingPointType(type);
        }

        /// <summary>
        /// Returns whether the given type is an integer type.
        /// </summary>
        public static bool IsIntegerType(this Type? type)
        {
            return Number.IsIntegerType(type);
        }

        /// <summary>
        /// Returns whether the given type is a Microsoft type, based on the company attribute of it's declaring assembly.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true" /> if the given type is a Microsoft type, <see langword="false" /> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool IsMicrosoftType(this Type? type)
        {
            if (type is null)
            {
                return false;
            }

            var attributes = type.GetTypeInfo().Assembly.GetCustomAttributes<AssemblyCompanyAttribute>();
            return attributes.Any(x => x.Company.Equals("Microsoft Corporation"));
        }

        /// <summary>
        /// Returns whether the given type is wrapped in a <see cref="Nullable{T}"/> wrapper.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        public static bool IsNullable(this Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            return type.IsClass || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Returns whether the given type is a numeric type.
        /// </summary>
        public static bool IsNumeric(this Type? type)
        {
            return Number.IsNumericType(type);
        }

        /// <summary>
        /// Returns whether the specified method info is an override.
        /// </summary>
        /// <param name="methodInfo"></param>
        public static bool IsOverride(this MethodInfo methodInfo)
        {
            Guard.IsNotNull(methodInfo, nameof(methodInfo));

            return methodInfo.GetBaseDefinition() != methodInfo;
        }

        /// <summary>
        /// Returns whether a <see cref="PropertyInfo"/> is static by looking
        /// at the public accessors of the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public static bool IsStatic(this PropertyInfo property)
            => IsStatic(property, false);

        /// <summary>
        /// Returns whether a <see cref="PropertyInfo"/> is static.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="nonPublic">Whether to look at non-public accessors to determine whether the property is static.</param>
        public static bool IsStatic(this PropertyInfo property, bool nonPublic)
        {
            Guard.IsNotNull(property, nameof(property));

            return property.GetAccessors(nonPublic).Any(a => a.IsStatic);
        }

        /// <summary>
        /// Returns whether the given type is of the specified type.
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="type">The type to check.</param>
        /// <exception cref="ArgumentNullException">type is <see langword="null"/></exception>
        /// <exception cref="TargetInvocationException">A static initializer is invoked and throws an exception.</exception>
        public static bool IsTypeOf<T>(this Type type)
        {
            return IsTypeOf(type, typeof(T));
        }

        /// <summary>
        /// Returns whether the given type is of the specified type. <paramref name="typeOf"/>
        /// may be an open generic type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="typeOf">The type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="typeOf"/> is <see langword="null"/></exception>
        /// <exception cref="TargetInvocationException">A static initializer is invoked and throws an exception.</exception>
        public static bool IsTypeOf(this Type type, Type typeOf)
        {
            Guard.IsNotNull(type, nameof(type));
            Guard.IsNotNull(typeOf, nameof(typeOf));

            if (type == typeOf)
            {
                return true;
            }

            if (!typeOf.IsGenericTypeDefinition)
            {
                return typeOf.IsAssignableFrom(type);
            }

            var interfaceTypes = type.GetInterfaces();
            if (interfaceTypes.Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeOf))
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeOf)
            {
                return true;
            }

            var baseType = type.BaseType;
            return baseType != null && IsTypeOf(baseType, typeOf);
        }

        /// <summary>
        /// Sets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to set the value of.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of the object. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException"><paramref name="memberInfo"/> is not of a static member.</exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in index does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static void SetValue(this MemberInfo memberInfo, object? value)
        {
            SetValue(memberInfo, null, value, null);
        }

        /// <summary>
        /// Sets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to set the value of.</param>
        /// <param name="forObject">The object to set the value on.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of <paramref name="forObject"/>. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and <paramref name="forObject"/> is <see langword="null" />. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in index does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static void SetValue(this MemberInfo memberInfo, object? forObject, object? value)
        {
            SetValue(memberInfo, forObject, value, null);
        }

        /// <summary>
        /// Sets the value of the <see cref="MemberInfo"/> instance.
        /// </summary>
        /// <param name="memberInfo">The member to set the value of.</param>
        /// <param name="forObject">The object to set the value on.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentException">The method is neither declared nor inherited by the class of <paramref name="forObject"/>. </exception>
        /// <exception cref="FieldAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.The caller does not have permission to access this field. </exception>
        /// <exception cref="NotSupportedException">A field is marked literal, but the field does not have one of the accepted literal types. </exception>
        /// <exception cref="TargetException">In the .NET for Windows Store apps or the Portable Class Library, catch <see cref="T:System.Exception" /> instead.The field is non-static and <paramref name="forObject"/> is <see langword="null" />. </exception>
        /// <exception cref="TargetInvocationException">An error occurred while retrieving the property value. For example, an index value specified for an indexed property is out of range. The <see cref="P:System.Exception.InnerException" /> property indicates the reason for the error.</exception>
        /// <exception cref="MethodAccessException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.MemberAccessException" />, instead.There was an illegal attempt to access a private or protected method inside a class. </exception>
        /// <exception cref="TargetParameterCountException">The number of parameters in <paramref name="index" /> does not match the number of parameters the indexed property takes. </exception>
        /// <exception cref="InvalidOperationException"><see cref="MemberInfo.MemberType"/> is not implemented for this <see cref="MemberInfo"/> subclass.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="memberInfo"/> is <see langword="null"/></exception>
        public static void SetValue(this MemberInfo memberInfo, object? forObject, object? value, object[]? index)
        {
            Guard.IsNotNull(memberInfo, nameof(memberInfo));

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fi = (FieldInfo)memberInfo;
                    fi.SetValue(forObject, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)memberInfo).SetValue(forObject, value, index);
                    break;
                case MemberTypes.Method:
                    ((MethodInfo)memberInfo).Invoke(forObject, new[] { value });
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
