using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace thirdpartynetwork.Helper
{
    [JsonConverter(typeof(OmmitableConverterFactory))]
    public class Ommitable<T>
    {
        public T? Value { get; }
        public bool IsSet { get; }

        // Constructor for setting the value explicitly
        public Ommitable(T? value = default)
        {
            Value = value;
            IsSet = value != null || Nullable.GetUnderlyingType(typeof(T)) != null; // Handle nullability
        }

        // Default constructor for when the value is not provided
        public Ommitable() : this(default) { }
    }

    public class OmmitableConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Ommitable<>);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            System.Diagnostics.Debug.WriteLine("Here 1!");

            var innerType = typeToConvert.GetGenericArguments()[0];
            System.Diagnostics.Debug.WriteLine(typeToConvert);
            System.Diagnostics.Debug.WriteLine(typeToConvert.GetGenericArguments());
            System.Diagnostics.Debug.WriteLine(innerType);

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(OmmitableConverter<>).MakeGenericType(innerType),
                BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                null);

            System.Diagnostics.Debug.WriteLine("Here 2!");
            return converter;
        }
    }

    public class OmmitableConverter<T> : JsonConverter<Ommitable<T>>
    {
        public override Ommitable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return new Ommitable<T>(default); // Return an Ommitable with null value

            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return new Ommitable<T>(value); // Deserialize value and set IsSet to true
        }

        public override void Write(Utf8JsonWriter writer, Ommitable<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Value, options); // Serialize the Value property of Ommitable
        }
    }
}