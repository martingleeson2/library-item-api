using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace Example.LibraryItem.Api
{
    /// <summary>
    /// Swagger schema filter that configures enums to display as string values instead of integers.
    /// This improves API documentation readability and makes enum values self-documenting.
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies enum string conversion to OpenAPI schema generation.
        /// Converts enum schemas from integer representation to string representation
        /// with all possible enum values listed for better API documentation.
        /// </summary>
        /// <param name="schema">The OpenAPI schema being processed</param>
        /// <param name="context">The schema filter context containing type information</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Only process enum types
            if (!context.Type.IsEnum)
                return;

            // Clear the existing integer-based enum configuration
            schema.Enum?.Clear();
            schema.Type = "string";
            schema.Format = null;

            // Add all enum values as strings
            var enumValues = new List<IOpenApiAny>();
            var enumNames = Enum.GetNames(context.Type);
            
            foreach (var enumName in enumNames)
            {
                // Convert enum name to snake_case to match our JsonNaming policy
                var snakeCaseName = ConvertToSnakeCase(enumName);
                enumValues.Add(new OpenApiString(snakeCaseName));
            }

            schema.Enum = enumValues;
            
            // Add description with all possible values for better documentation
            var valuesList = string.Join(", ", enumNames.Select(ConvertToSnakeCase));
            schema.Description = $"Possible values: {valuesList}";
        }

        /// <summary>
        /// Converts PascalCase enum names to snake_case to match the API's JSON naming convention.
        /// Example: "CheckedOut" becomes "checked_out"
        /// </summary>
        /// <param name="input">The PascalCase string to convert</param>
        /// <returns>The snake_case equivalent</returns>
        private static string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new System.Text.StringBuilder();
            
            for (int i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];
                
                // Add underscore before uppercase letters (except the first character)
                if (i > 0 && char.IsUpper(currentChar))
                {
                    result.Append('_');
                }
                
                result.Append(char.ToLower(currentChar));
            }
            
            return result.ToString();
        }
    }
}