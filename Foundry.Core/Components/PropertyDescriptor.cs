using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Components
{
    /// <summary>
    /// An <see cref="IDescriptor"/> for a <see cref="PropertyInfo"/>.
    /// </summary>
    public class PropertyDescriptor : IEquatable<PropertyDescriptor>
    {
        private static readonly Dictionary<PropertyInfo, AttributeDescriptorCache[]> AttributeCache = new Dictionary<PropertyInfo, AttributeDescriptorCache[]>();

        /// <summary>
        /// The <see cref="Framework.TypeDescriptor"/> associated with this
        /// <see cref="PropertyDescriptor"/>.
        /// </summary>
        public TypeDescriptor TypeDescriptor { get; }

        /// <summary>
        /// The <see cref="PropertyInfo"/> this descriptor is for.
        /// </summary>
        public PropertyInfo Property { get; }

        private readonly AttributeDescriptorCache[] Attributes;

        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/></exception>
        /// <exception cref="TypeLoadException">A custom attribute type could not be loaded.</exception>
        /// <exception cref="InvalidCastException">An element in the sequence cannot be cast to type <see cref="Attribute"/>.</exception>
        public PropertyDescriptor(PropertyInfo property, TypeDescriptor type)
        {
            Guard.IsNotNull(property, nameof(property));
            Guard.IsNotNull(type, nameof(type));

            Property = property;
            TypeDescriptor = type;

            if (AttributeCache.TryGetValue(Property, out var attributeCache))
            {
                Attributes = attributeCache;
            }
            else
            {
                lock (AttributeCache)
                {
                    if (AttributeCache.TryGetValue(Property, out attributeCache))
                    {
                        Attributes = attributeCache;
                    }
                    else
                    {
                        var implemented = Attribute.GetCustomAttributes(Property, false);
                        var all = Attribute.GetCustomAttributes(Property, true);

                        var aCache = new List<AttributeDescriptorCache>(implemented.Select(a => new AttributeDescriptorCache
                        {
                            Attribute = a,
                            IsInherited = false
                        }));
                        aCache.AddRange(all.Except(implemented).Select(a => new AttributeDescriptorCache
                        {
                            Attribute = a,
                            IsInherited = true
                        }));

                        Attributes = aCache.ToArray();
                        AttributeCache.Add(Property, Attributes);
                    }
                }
            }
        }

        public PropertyDescriptor(PropertyInfo property, Type type)
            : this(property, new TypeDescriptor(type))
        {

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
        public bool Equals(PropertyDescriptor? other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other) || Property.Equals(other.Property);
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

            return obj.GetType() == GetType() && Equals((PropertyDescriptor)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => Property.GetHashCode();

        public static bool operator ==(PropertyDescriptor? left, PropertyDescriptor? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyDescriptor? left, PropertyDescriptor? right)
        {
            return !Equals(left, right);
        }

        private class AttributeDescriptorCache
        {
            public Attribute Attribute { get; set; } = null!;
            public bool IsInherited { get; set; }
        }
    }

    /// <summary>
    /// Checks the equality of two <see cref="PropertyDescriptor"/>s.
    /// </summary>

    public class PropertyDescriptorEqualityComparer : IEqualityComparer<PropertyDescriptor>
    {
        /// <inheritdoc />
        public bool Equals(PropertyDescriptor x, PropertyDescriptor y)
        {
            return x == y;
        }

        /// <inheritdoc />
        public int GetHashCode(PropertyDescriptor obj)
        {
            return obj.GetHashCode();
        }
    }
}
