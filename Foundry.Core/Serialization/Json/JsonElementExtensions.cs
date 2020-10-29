using System;
using System.Buffers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundry.IO;

namespace Foundry.Serialization.Json
{
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Returns whether this <see cref="JsonElement"/> contains a boolean token type.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns><see langword="true"/> if this element contains a boolean token type, <see langword="false"/> otherwise.</returns>
        public static bool IsBoolean(this in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.True ||
                   element.ValueKind == JsonValueKind.False;
        }

        /// <summary>
        /// Returns the boolean value from this <see cref="JsonElement"/>. If this element is not a boolean,
        /// <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="element">The element to get the boolean value from.</param>
        /// <param name="defaultValue">The value to return if <paramref name="element"/> is not a boolean token.</param>
        public static bool GetBoolean(this in JsonElement element, bool defaultValue)
        {
            if (IsBoolean(element))
            {
                return element.GetBoolean();
            }

            return defaultValue;
        }

        /// <summary>
        /// Converts this <see cref="JsonElement"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="element">The element to convert.</param>
        /// <param name="options">The serialization options.</param>
        public static T ToObject<T>(this in JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }

        /// <summary>
        /// Converts this <see cref="JsonElement"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="element">The element to convert.</param>
        public static ValueTask<T> ToObjectAsync<T>(this in JsonElement element, CancellationToken cancellationToken = default)
            => ToObjectAsync<T>(element, null, cancellationToken);


        /// <summary>
        /// Converts this <see cref="JsonElement"/> into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="element">The element to convert.</param>
        /// <param name="options">The serialization options.</param>
        public static ValueTask<T> ToObjectAsync<T>(this in JsonElement element, JsonSerializerOptions? options, CancellationToken cancellationToken = default)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            using var ms = new MemoryStream(bufferWriter.WrittenMemory);
            return JsonSerializer.DeserializeAsync<T>(ms, options, cancellationToken);
        }

        /// <summary>
        /// Converts this <see cref="JsonElement"/> into an object of type <paramref name="targetType"/>.
        /// </summary>
        /// <param name="element">The element to convert.</param>
        /// <param name="targetType">The type to convert to.</param>
        public static ValueTask<object?> ToObjectAsync(this in JsonElement element, Type targetType, CancellationToken cancellationToken = default)
            => ToObjectAsync(element, targetType, null, cancellationToken);


        /// <summary>
        /// Converts this <see cref="JsonElement"/> into an object of type <paramref name="targetType"/>.
        /// </summary>
        /// <param name="element">The element to convert.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="options">The serialization options.</param>
        public static async ValueTask<object?> ToObjectAsync(this JsonElement element, Type targetType, JsonSerializerOptions? options, CancellationToken cancellationToken = default)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            using var ms = new MemoryStream(bufferWriter.WrittenMemory);
            return await JsonSerializer.DeserializeAsync(ms, targetType, options, cancellationToken).ConfigureAwait(false);
        }
    }
}
