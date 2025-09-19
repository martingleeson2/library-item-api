using Example.LibraryItem.Application.Dtos;
using Example.LibraryItem.Domain;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Example.LibraryItem.Api
{
    /// <summary>
    /// Provides realistic examples for Swagger documentation to replace default generic examples.
    /// This improves API usability by showing valid, working examples for all request bodies.
    /// 
    /// PREDICTABLE TEST IDs (available in Development environment):
    /// - 11111111-1111-1111-1111-111111111111 = "The Great Gatsby" by F. Scott Fitzgerald
    /// - 22222222-2222-2222-2222-222222222222 = "To Kill a Mockingbird" by Harper Lee
    /// 
    /// These IDs can be used for testing GET, PUT, PATCH, and DELETE operations.
    /// </summary>
    public class SwaggerExampleFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies custom examples to schema generation for better Swagger documentation.
        /// Replaces generic "string" examples with realistic library item data.
        /// </summary>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Apply examples to specific DTO types
            if (context.Type == typeof(ItemCreateRequestDto))
            {
                schema.Example = CreateItemCreateExample();
            }
            else if (context.Type == typeof(ItemUpdateRequestDto))
            {
                schema.Example = CreateItemUpdateExample();
            }
            else if (context.Type == typeof(ItemPatchRequestDto))
            {
                schema.Example = CreateItemPatchExample();
            }
            else if (context.Type == typeof(ItemLocationDto))
            {
                schema.Example = CreateLocationExample();
            }
        }

        /// <summary>
        /// Creates a realistic example for creating a new library item.
        /// Uses "The Great Gatsby" as a complete, valid example that passes all validations.
        /// </summary>
        private static OpenApiObject CreateItemCreateExample()
        {
            return new OpenApiObject
            {
                ["title"] = new OpenApiString("The Great Gatsby"),
                ["subtitle"] = new OpenApiString("A Novel"),
                ["author"] = new OpenApiString("F. Scott Fitzgerald"),
                ["contributors"] = new OpenApiArray
                {
                    new OpenApiString("Maxwell Perkins (Editor)")
                },
                ["isbn"] = new OpenApiString("9780743273565"),
                ["publisher"] = new OpenApiString("Scribner"),
                ["publication_date"] = new OpenApiString("1925-04-10"),
                ["edition"] = new OpenApiString("1st Edition"),
                ["pages"] = new OpenApiInteger(180),
                ["language"] = new OpenApiString("en"),
                ["item_type"] = new OpenApiString("book"),
                ["call_number"] = new OpenApiString("813.52 F55g"),
                ["classification_system"] = new OpenApiString("dewey_decimal"),
                ["collection"] = new OpenApiString("General Collection"),
                ["location"] = new OpenApiObject
                {
                    ["floor"] = new OpenApiInteger(2),
                    ["section"] = new OpenApiString("A"),
                    ["shelf_code"] = new OpenApiString("A01"),
                    ["wing"] = new OpenApiString("North Wing")
                },
                ["status"] = new OpenApiString("available"),
                ["barcode"] = new OpenApiString("123456789012"),
                ["acquisition_date"] = new OpenApiString("2023-03-15"),
                ["cost"] = new OpenApiDouble(29.95),
                ["condition_notes"] = new OpenApiString("Excellent condition"),
                ["description"] = new OpenApiString("A classic American novel set in the Jazz Age, exploring themes of wealth, love, and the American Dream."),
                ["subjects"] = new OpenApiArray
                {
                    new OpenApiString("American Literature"),
                    new OpenApiString("Fiction"),
                    new OpenApiString("Jazz Age"),
                    new OpenApiString("Classic Literature")
                }
            };
        }

        /// <summary>
        /// Creates a realistic example for updating an existing library item.
        /// Uses "The Great Gatsby" with predictable ID 11111111-1111-1111-1111-111111111111 as the update target.
        /// </summary>
        private static OpenApiObject CreateItemUpdateExample()
        {
            return new OpenApiObject
            {
                ["title"] = new OpenApiString("The Great Gatsby (Updated Edition)"),
                ["subtitle"] = new OpenApiString("A Timeless Classic"),
                ["author"] = new OpenApiString("F. Scott Fitzgerald"),
                ["contributors"] = new OpenApiArray
                {
                    new OpenApiString("F. Scott Fitzgerald"),
                    new OpenApiString("Maxwell Perkins (Editor)")
                },
                ["isbn"] = new OpenApiString("9780743273565"),
                ["publisher"] = new OpenApiString("Scribner"),
                ["publication_date"] = new OpenApiString("1925-04-10"),
                ["edition"] = new OpenApiString("Revised Edition"),
                ["pages"] = new OpenApiInteger(200),
                ["language"] = new OpenApiString("en"),
                ["item_type"] = new OpenApiString("book"),
                ["call_number"] = new OpenApiString("813.52 F55g"),
                ["classification_system"] = new OpenApiString("dewey_decimal"),
                ["collection"] = new OpenApiString("Classic Literature"),
                ["location"] = new OpenApiObject
                {
                    ["floor"] = new OpenApiInteger(2),
                    ["section"] = new OpenApiString("A"),
                    ["shelf_code"] = new OpenApiString("A01"),
                    ["wing"] = new OpenApiString("North Wing")
                },
                ["status"] = new OpenApiString("available"), // Required for updates
                ["barcode"] = new OpenApiString("123456789012"),
                ["acquisition_date"] = new OpenApiString("2023-03-15"),
                ["cost"] = new OpenApiDouble(29.95),
                ["condition_notes"] = new OpenApiString("Excellent condition - updated"),
                ["description"] = new OpenApiString("A classic American novel set in the Jazz Age, exploring themes of wealth, love, and the American Dream. Updated edition with new annotations."),
                ["subjects"] = new OpenApiArray
                {
                    new OpenApiString("American Literature"),
                    new OpenApiString("Fiction"),
                    new OpenApiString("Jazz Age"),
                    new OpenApiString("Classic Literature")
                }
            };
        }

        /// <summary>
        /// Creates a realistic example for partially updating a library item.
        /// Shows how to update just a few fields using PATCH on "The Great Gatsby" (ID: 11111111-1111-1111-1111-111111111111).
        /// </summary>
        private static OpenApiObject CreateItemPatchExample()
        {
            return new OpenApiObject
            {
                ["status"] = new OpenApiString("checked_out"),
                ["condition_notes"] = new OpenApiString("Good condition, minor wear on spine"),
                ["cost"] = new OpenApiDouble(35.00),
                ["subtitle"] = new OpenApiString("A Timeless American Classic")
            };
        }

        /// <summary>
        /// Creates a realistic example for location information.
        /// Uses typical library location hierarchy.
        /// </summary>
        private static OpenApiObject CreateLocationExample()
        {
            return new OpenApiObject
            {
                ["floor"] = new OpenApiInteger(2),
                ["section"] = new OpenApiString("A"),
                ["shelf_code"] = new OpenApiString("A01"),
                ["wing"] = new OpenApiString("North Wing"),
                ["position"] = new OpenApiString("Top Shelf"),
                ["notes"] = new OpenApiString("Near the reference desk")
            };
        }
    }
}