using System.Text.Json;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Serialization.Json
{
    /// <summary>
    /// <see cref="Utf8JsonWriter"/> extensions.
    /// </summary>
    public static class Utf8JsonWriterExtensions
    {
        /// <summary>
        /// Writes an object while respecting <see cref="JsonSerializerOptions.IgnoreNullValues"/>.
        /// </summary>
        public static void WriteObject(this Utf8JsonWriter writer, string propertyName, object? value, JsonSerializerOptions options)
            => WriteObject(writer, propertyName, value, options.IgnoreNullValues, options);

        /// <summary>
        /// Writes an object while respecting <paramref name="ignoreNullValues"/>.
        /// </summary>
        public static void WriteObject(this Utf8JsonWriter writer, string propertyName, object? value, bool ignoreNullValues, JsonSerializerOptions options)
        {
            Guard.IsNotNull(writer, nameof(writer));
            Guard.IsNotNullOrWhiteSpace(propertyName, nameof(propertyName));
            Guard.IsNotNull(options, nameof(options));

            if (value is null && ignoreNullValues)
            {
                return;
            }

            writer.WritePropertyName(propertyName, options);
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        /// <summary>
        /// Writes an object while respecting <see cref="JsonSerializerOptions.IgnoreNullValues"/>.
        /// </summary>
        public static void WriteObjectValue(this Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            Guard.IsNotNull(writer, nameof(writer));
            Guard.IsNotNull(options, nameof(options));

            if (value is null && options.IgnoreNullValues)
            {
                return;
            }

            if (value is null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        /// <summary>
        /// Writes the property name (as a JSON string) as the first part of a name/value pair
        /// of a JSON object. The output name will respect <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>.
        /// </summary>
        public static void WritePropertyName(this Utf8JsonWriter writer, string propertyName, JsonSerializerOptions options)
        {
            Guard.IsNotNull(writer, nameof(writer));
            Guard.IsNotNull(propertyName, nameof(propertyName));
            Guard.IsNotNull(options, nameof(options));

            string serializedName = options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
            writer.WritePropertyName(serializedName);
        }
    }
}
