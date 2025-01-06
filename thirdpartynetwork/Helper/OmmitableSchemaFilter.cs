using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace thirdpartynetwork.Helper
{
    public class OmmitableSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(NullableWithState<>))
            {
                var innerType = context.Type.GetGenericArguments()[0];
                var innerSchema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository);

                schema.Type = innerSchema.Type;
                schema.Format = innerSchema.Format;
                schema.Items = innerSchema.Items;
                schema.Properties = innerSchema.Properties;
                schema.AdditionalPropertiesAllowed = innerSchema.AdditionalPropertiesAllowed;
            }
        }
    }
}