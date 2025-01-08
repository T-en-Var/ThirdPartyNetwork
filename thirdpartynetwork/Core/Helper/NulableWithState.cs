using System.Text.Json;
using System.Text.Json.Serialization;

namespace thirdpartynetwork.Helper
{
    public struct NullableWithState<T> where T : class
    {
        private T? _value;
        private bool _isSet;

        [JsonIgnore]  // Ignore during serialization
        public T? Value
        {
            get => _value;
            init
            {
                _value = value;
                _isSet = true;
            }
        }

        [JsonIgnore]  // Ignore IsSet during serialization
        public bool IsSet => _isSet;

        // Custom serialization for when we need to treat it as a simple value
        public static implicit operator T?(NullableWithState<T> state) => state._value;
        public static implicit operator NullableWithState<T>(T? value) => new() { _value = value, _isSet = value != null };
    }

    public class NullableWithStateConverter<T> : JsonConverter<NullableWithState<T>> where T : class
    {
        public override NullableWithState<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialize the value directly from JSON (T could be any class type)
            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return new NullableWithState<T> { Value = value };
        }

        public override void Write(Utf8JsonWriter writer, NullableWithState<T> value, JsonSerializerOptions options)
        {
            // Serialize the value only (not the internal structure)
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }

    public class NullableWithStateConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Check if the type is a NullableWithState<T> (where T is any class type)
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(NullableWithState<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // Create a generic converter based on the type
            var innerType = typeToConvert.GetGenericArguments()[0]; // Get the generic type argument (T)
            var converterType = typeof(NullableWithStateConverter<>).MakeGenericType(innerType); // Create the correct converter
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }
}
