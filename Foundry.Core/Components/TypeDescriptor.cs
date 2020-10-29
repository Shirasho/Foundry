using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Components
{
    /// <summary>
    /// A descriptor for a type.
    /// </summary>
    public class TypeDescriptor : IEquatable<TypeDescriptor>
    {
        private static readonly Dictionary<Type, TypeDescriptorCache> Cache = new Dictionary<Type, TypeDescriptorCache>();
        private static readonly Dictionary<Type, AttributeDescriptorCache[]> AttributeCache = new Dictionary<Type, AttributeDescriptorCache[]>();

        /// <summary>
        /// The type this descriptor is for.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The public properties of this type.
        /// </summary>
        public IImmutableList<PropertyDescriptor> Properties => Info.Properties;

        /// <summary>
        /// The public fields of this type.
        /// </summary>
        public IImmutableList<FieldDescriptor> Fields => Info.Fields;

        private readonly TypeDescriptorCache Info;
        private readonly AttributeDescriptorCache[] Attributes;

        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/></exception>
        /// <exception cref="TypeLoadException">A custom attribute type could not be loaded.</exception>
        /// <exception cref="InvalidCastException">An element in the sequence cannot be cast to type <see cref="Attribute"/>.</exception>
        public TypeDescriptor(Type type)
        {
            Guard.IsNotNull(type, nameof(type));

            Type = type;
            if (Cache.TryGetValue(Type, out var cache))
            {
                Info = cache;
            }
            else
            {
                lock (Cache)
                {
                    if (Cache.TryGetValue(Type, out cache))
                    {
                        Info = cache;
                    }
                    else
                    {
                        Info = new TypeDescriptorCache
                        {
                            Properties = Type.GetProperties().Select(p => new PropertyDescriptor(p, this)).ToImmutableArray(),
                            Fields = Type.GetFields().Select(f => new FieldDescriptor(f, this)).ToImmutableArray()
                        };

                        Cache.Add(Type, Info);
                    }
                }
            }

            if (AttributeCache.TryGetValue(Type, out var attributeCache))
            {
                Attributes = attributeCache;
            }
            else
            {
                lock (AttributeCache)
                {
                    if (AttributeCache.TryGetValue(Type, out attributeCache))
                    {
                        Attributes = attributeCache;
                    }
                    else
                    {
                        //TODO: Static lambda C#9
                        var implemented = Type.GetCustomAttributes(false).Cast<Attribute>().Select(a => new AttributeDescriptorCache(a, false)).ToList();
                        var inherited = Type.GetCustomAttributes(true).Cast<Attribute>().Select(a => new AttributeDescriptorCache(a, true)).ToList();

                        implemented.AddRangeUnique(inherited);

                        Attributes = implemented.ToArray();
                        AttributeCache.Add(Type, Attributes);
                    }
                }
            }
        }

        /// <summary>
        /// Whether the type has the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute.</typeparam>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        public bool HasAttribute<TAttribute>(bool inherit = false)
            where TAttribute : Attribute => HasAttribute(typeof(TAttribute), inherit);

        /// <summary>
        /// Whether the type has the specified attribute.
        /// </summary>
        /// <param name="attributeType">The type of attribute.</param>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/> type.</exception>
        public bool HasAttribute(Type attributeType, bool inherit = false)
        {
            Guard.IsNotNull(attributeType, nameof(attributeType));
            GuardEx.IsTypeOf<Attribute>(attributeType, nameof(attributeType));

            foreach (var attribute in Attributes)
            {
                if (!inherit && attribute.IsInherited)
                {
                    continue;
                }

                if (attribute.Attribute.GetType() == attributeType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Obtains the specified attribute from the type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        public TAttribute? GetAttribute<TAttribute>(bool inherit = false)
            where TAttribute : Attribute => GetAttribute(typeof(TAttribute), inherit) as TAttribute;

        /// <summary>
        /// Obtains the specified attribute from the type.
        /// </summary>
        /// <param name="attributeType">The type of attribute.</param>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/> type.</exception>
        public Attribute? GetAttribute(Type attributeType, bool inherit = false)
        {
            Guard.IsNotNull(attributeType, nameof(attributeType));
            GuardEx.IsTypeOf<Attribute>(attributeType, nameof(attributeType));

            foreach (var attribute in Attributes)
            {
                if (!inherit && attribute.IsInherited)
                {
                    continue;
                }

                if (attribute.Attribute.GetType() == attributeType)
                {
                    return attribute.Attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtains all of the specified attribute from the type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attributes to get.</typeparam>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        /// <exception cref="InvalidCastException">An element in the sequence cannot be cast to type <typeparamref name="TAttribute"/>.</exception>
        public IEnumerable<TAttribute> GetAttributes<TAttribute>(bool inherit = false)
            where TAttribute : Attribute => GetAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();

        /// <summary>
        /// Obtains all of the specified attribute from the type.
        /// </summary>
        /// <param name="attributeType">The type of attribute.</param>
        /// <param name="inherit">Whether to look for inherited attributes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="attributeType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/> type.</exception>
        public IEnumerable<Attribute> GetAttributes(Type attributeType, bool inherit = false)
        {
            Guard.IsNotNull(attributeType, nameof(attributeType));
            GuardEx.IsTypeOf<Attribute>(attributeType, nameof(attributeType));

            foreach (var attribute in Attributes)
            {
                if (!inherit && attribute.IsInherited)
                {
                    continue;
                }

                if (attribute.Attribute.GetType() == attributeType)
                {
                    yield return attribute.Attribute;
                }
            }
        }

        /// <inheritdoc />
        public bool Equals(TypeDescriptor? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Type == other.Type;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is TypeDescriptor other && Equals(other);
        }

        public static bool operator ==(TypeDescriptor left, TypeDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypeDescriptor left, TypeDescriptor right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // We do not want to return the same hash code as Type, otherwise we
            // will get collisions in a dictionary where the key is of type
            // System.Object.
            return HashCode.Combine(Type, typeof(TypeDescriptor));
        }

        private class TypeDescriptorCache
        {
            public IImmutableList<PropertyDescriptor> Properties { get; set; } = null!;
            public IImmutableList<FieldDescriptor> Fields { get; set; } = null!;
        }

        private class AttributeDescriptorCache : IEquatable<AttributeDescriptorCache>
        {
            public Attribute Attribute { get; }
            public bool IsInherited { get; }

            public AttributeDescriptorCache(Attribute attribute, bool isInherited)
            {
                Attribute = attribute;
                IsInherited = isInherited;
            }

            /// <inheritdoc />
            public bool Equals(AttributeDescriptorCache? other)
            {
                if (other is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Attribute.Equals(other.Attribute);
            }

            /// <inheritdoc />
            public override bool Equals(object? obj)
            {
                if (obj is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType() && Equals((AttributeDescriptorCache)obj);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return HashCode.Combine(Attribute, IsInherited);
            }

            public static bool operator ==(AttributeDescriptorCache left, AttributeDescriptorCache right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(AttributeDescriptorCache left, AttributeDescriptorCache right)
            {
                return !Equals(left, right);
            }
        }
    }

    /// <summary>
    /// Checks the equality of two <see cref="TypeDescriptor"/>s.
    /// </summary>
    public class TypeDescriptorEqualityComparer : IEqualityComparer<TypeDescriptor>
    {
        /// <inheritdoc />
        public bool Equals(TypeDescriptor x, TypeDescriptor y)
        {
            return x == y;
        }

        /// <inheritdoc />
        public int GetHashCode(TypeDescriptor obj)
        {
            return obj.GetHashCode();
        }
    }
}
