
namespace MachineLearning.foobar20000
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.OData.Edm;
    using Microsoft.OData.ModelBuilder;
    using System.Reflection;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Extensions.DependencyInjection;

    public class DynamicEdmModelGenerator
    {
        public static IEdmModel GetEdmModel(DbContext dbContext)
        {
            var builder = new ODataConventionModelBuilder();

            // Get all entity types from the DbContext
            var entityTypes = GetEntityTypes(dbContext);

            foreach (var entityType in entityTypes)
            {
                // Create an entity type configuration
                var edmEntityType = CreateEntityTypeConfiguration(builder, entityType);

                // Add entity set
                builder.AddEntitySet(entityType.Name, edmEntityType);
            }

            return builder.GetEdmModel();
        }

        private static IEnumerable<Type> GetEntityTypes(DbContext dbContext)
        {
            // Get all DbSet properties from the context
            return dbContext.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType &&
                       p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]);
        }

        private static EntityTypeConfiguration CreateEntityTypeConfiguration(
            ODataConventionModelBuilder builder,
            Type entityType)
        {
            // Create the entity type configuration
            var edmEntityType = builder.AddEntityType(entityType);

            // Get all properties
            var properties = entityType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Skip collections and navigation properties
                if (IsSimpleType(property.PropertyType))
                {
                    var propConfig = edmEntityType.AddProperty(property);

                    // Handle nullability
                    var nullableAttr = property.GetCustomAttribute<RequiredAttribute>();
                    if (nullableAttr != null ||
                        (!property.PropertyType.IsValueType ||
                         Nullable.GetUnderlyingType(property.PropertyType) != null))
                    {
                        propConfig.IsRequired();
                    }
                }
            }

            // Identify and set primary key
            var keyProperties = entityType.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .ToList();

            if (keyProperties.Any())
            {
                foreach (var keyProp in keyProperties)
                {
                    edmEntityType.HasKey(keyProp);
                }
            }
            else
            {
                // Fallback to conventional key naming
                var conventionalKeyProp = properties
                    .FirstOrDefault(p =>
                        p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals($"{entityType.Name}Id", StringComparison.OrdinalIgnoreCase));

                if (conventionalKeyProp != null)
                {
                    edmEntityType.HasKey(conventionalKeyProp);
                }
            }

            return edmEntityType;
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid) ||
                   (Nullable.GetUnderlyingType(type) != null &&
                    IsSimpleType(Nullable.GetUnderlyingType(type)));
        }
    }

    // Example usage in Startup.cs or Program.cs
    public class ExampleUsage
    {

        public static void Test()
        {
            DynamicEdmModelGenerator.GetEdmModel(new DynamicContext());
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            string cs = ""; // Configuration.GetConnectionString("DefaultConnection")

            // Your DbContext configuration
            services.AddDbContext<DynamicContext>(options =>
                options.UseSqlServer(cs));

            // Configure OData
            // services.AddControllers()
            // .AddOData(opt => opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
            // .AddModel("odata", DynamicEdmModelGenerator.GetEdmModel(new DynamicContext())));
        }
    }
}
