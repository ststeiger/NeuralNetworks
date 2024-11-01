
namespace MachineLearning.FlopOData
{


    // using Microsoft.OData.Edm; 
    // using Microsoft.OData.ModelBuilder;
    
    public class DynamicODataModelGenerator
    {
        // Cache to store dynamically created types
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Type> _dynamicTypes =
            new System.Collections.Concurrent.ConcurrentDictionary<string, System.Type>();

        public class ColumnInfo
        {
            public string Name { get; set; }
            public System.Type DataType { get; set; }
            public bool IsNullable { get; set; }
            public bool IsPrimaryKey { get; set; }
        }

        public class TableInfo
        {
            public string Name { get; set; }
            public System.Collections.Generic.List<ColumnInfo> Columns { get; set; } = new System.Collections.Generic.List<ColumnInfo>();
        }


        public static System.Data.Common.DbConnection GetConnection(string cs)
        {
            // return new System.Data.SqlClient.SqlConnection(cs);
            return new Microsoft.Data.SqlClient.SqlConnection(cs);
        }


        public static System.Collections.Generic.List<TableInfo> GetDatabaseSchema(string connectionString)
        {
            System.Collections.Generic.List<TableInfo> tables = new System.Collections.Generic.List<TableInfo>();

            using (System.Data.Common.DbConnection cnn = GetConnection(connectionString))
            {
                if(cnn.State != System.Data.ConnectionState.Open)
                    cnn.Open();

                using (System.Data.Common.DbCommand cmd = cnn.CreateCommand())
                {
                    cmd.CommandText= @"
 SELECT 
     t.name AS TableName
    ,c.name AS ColumnName
    ,ty.name AS DataType
    ,c.is_nullable AS IsNullable
    ,CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
LEFT JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.column_id = c.column_id
LEFT JOIN sys.indexes i ON i.object_id = t.object_id AND ic.index_id = i.index_id
LEFT JOIN sys.key_constraints pk ON pk.parent_object_id = t.object_id 
    AND pk.type = 'PK' AND i.index_id = pk.unique_index_id
WHERE t.is_ms_shipped = 0
ORDER BY t.name, c.column_id
";


                    using (System.Data.Common.DbDataReader reader = cmd.ExecuteReader())
                    {
                        TableInfo currentTable = null;

                        while (reader.Read())
                        {
                            string tableName = reader["TableName"].ToString();

                            // Start a new table if needed
                            if (currentTable == null || currentTable.Name != tableName)
                            {
                                if (currentTable != null)
                                    tables.Add(currentTable);

                                currentTable = new TableInfo { Name = tableName };
                            }

                            // Map SQL Server types to .NET types
                            System.Type columnType = MapSqlServerTypeToClrType(reader["DataType"].ToString());

                            // If it's a nullable value type, use Nullable<T>
                            if (columnType.IsValueType && System.Convert.ToBoolean(reader["IsNullable"]))
                            {
                                columnType = typeof(System.Nullable<>).MakeGenericType(columnType);
                            }

                            currentTable.Columns.Add(new ColumnInfo
                            {
                                Name = reader["ColumnName"].ToString(),
                                DataType = columnType,
                                IsNullable = System.Convert.ToBoolean(reader["IsNullable"]),
                                IsPrimaryKey = System.Convert.ToBoolean(reader["IsPrimaryKey"])
                            });
                        } // Whend 

                        // Add the last table
                        if (currentTable != null)
                            tables.Add(currentTable);

                    } // End Using reader 

                } // End Using cmd 

                if (cnn.State != System.Data.ConnectionState.Closed)
                    cnn.Close();
            } // End Using cnn 

            return tables;
        } // End Function GetDatabaseSchema 


        public static Microsoft.OData.Edm.IEdmModel CreateDynamicEdmModel(string connectionString)
        {
            Microsoft.OData.ModelBuilder.ODataConventionModelBuilder builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();
            System.Collections.Generic.List<TableInfo> schema = GetDatabaseSchema(connectionString);

            foreach (TableInfo table in schema)
            {
                // Generate or retrieve a dynamic type for this table
                System.Type dynamicType = GenerateDynamicType(table);

                // Create entity type using the dynamic type
                Microsoft.OData.ModelBuilder.EntityTypeConfiguration entityType = builder.AddEntityType(dynamicType);

                // Identify primary key property
                // System.Reflection.PropertyInfo? keyProperty = dynamicType.GetProperties()
                // .FirstOrDefault(p => table.Columns.First(c => c.Name == p.Name).IsPrimaryKey);

                System.Reflection.PropertyInfo? keyProperty = null; 
                foreach (System.Reflection.PropertyInfo property in dynamicType.GetProperties())
                {
                    foreach (ColumnInfo thisColumn in table.Columns)
                    {
                        if (!string.Equals(thisColumn.Name, property.Name, System.StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        if (thisColumn.IsPrimaryKey)
                        {
                            keyProperty = property;
                            break;
                        } // End if (thisColumn.IsPrimaryKey) 

                    } // Next thisColumn 

                    // Check if keyProperty is set after the inner loop finishes iterating through all columns
                    if (keyProperty != null)
                        break; // Exit the outer loop after finding the primary key property
                } // Next property 


                if (keyProperty != null)
                    entityType.HasKey(keyProperty);

                // Add entity set
                builder.AddEntitySet(table.Name, entityType);
            } // Next table 

            return builder.GetEdmModel();
        } // End Function CreateDynamicEdmModel 


        private static System.Type GenerateDynamicType(TableInfo tableInfo)
        {
            // Check if type already exists in cache
            if (_dynamicTypes.TryGetValue(tableInfo.Name, out System.Type existingType))
            {
                return existingType;
            }

            // Create a dynamic assembly and module
            System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName($"DynamicODataAssembly_{tableInfo.Name}");
            System.Reflection.Emit.AssemblyBuilder assemblyBuilder = System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(
                assemblyName, System.Reflection.Emit.AssemblyBuilderAccess.Run);
            System.Reflection.Emit.ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            // Define the dynamic type
            System.Reflection.Emit.TypeBuilder typeBuilder = moduleBuilder.DefineType(
                tableInfo.Name,
                System.Reflection.TypeAttributes.Public |
                System.Reflection.TypeAttributes.Class |
                System.Reflection.TypeAttributes.AutoClass |
                System.Reflection.TypeAttributes.AnsiClass |
                System.Reflection.TypeAttributes.BeforeFieldInit |
                System.Reflection.TypeAttributes.AutoLayout,
                typeof(object));

            // Create properties for each column
            foreach (ColumnInfo column in tableInfo.Columns)
            {
                CreateProperty(typeBuilder, column.Name, column.DataType);
            }

            // Create the type
            System.Type generatedType = typeBuilder.CreateType();

            // Cache the generated type
            _dynamicTypes[tableInfo.Name] = generatedType;

            return generatedType;
        }

        private static void CreateProperty(System.Reflection.Emit.TypeBuilder typeBuilder, string propertyName, System.Type propertyType)
        {
            // Create private field
            System.Reflection.Emit.FieldBuilder fieldBuilder = typeBuilder.DefineField(
                $"_{propertyName}",
                propertyType,
                System.Reflection.FieldAttributes.Private);

            // Create property
            System.Reflection.Emit.PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                propertyName,
                System.Reflection.PropertyAttributes.HasDefault,
                propertyType,
                null);

            // Create GET method
            System.Reflection.Emit.MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod(
                $"get_{propertyName}",
                System.Reflection.MethodAttributes.Public |
                System.Reflection.MethodAttributes.SpecialName |
                System.Reflection.MethodAttributes.HideBySig,
                propertyType,
               System.Type.EmptyTypes);

            System.Reflection.Emit.ILGenerator getIl = getPropMthdBldr.GetILGenerator();
            getIl.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            getIl.Emit(System.Reflection.Emit.OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(System.Reflection.Emit.OpCodes.Ret);

            // Create SET method
            System.Reflection.Emit.MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod(
                $"set_{propertyName}",
                System.Reflection.MethodAttributes.Public |
                System.Reflection.MethodAttributes.SpecialName |
                System.Reflection.MethodAttributes.HideBySig,
                null,
                new System.Type[] { propertyType });

            System.Reflection.Emit.ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            setIl.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            setIl.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            setIl.Emit(System.Reflection.Emit.OpCodes.Stfld, fieldBuilder);
            setIl.Emit(System.Reflection.Emit.OpCodes.Ret);

            // Map the methods to the PropertyBuilder
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        private static System.Type MapSqlServerTypeToClrType(string sqlType)
        {
            // Same implementation as in previous example
            switch (sqlType.ToLower())
            {
                case "int": return typeof(int);
                case "bigint": return typeof(long);
                case "smallint": return typeof(short);
                case "tinyint": return typeof(byte);
                case "bit": return typeof(bool);
                case "decimal":
                case "numeric": return typeof(decimal);
                case "float": return typeof(double);
                case "real": return typeof(float);
                case "datetime":
                case "date":
                case "datetime2": return typeof(System.DateTime);
                case "datetimeoffset": return typeof(System.DateTimeOffset);
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "text":
                case "ntext": return typeof(string);
                case "uniqueidentifier": return typeof(System.Guid);
                case "binary":
                case "varbinary": return typeof(byte[]);
                default: return typeof(object);
            }
        }
    }

    // Example Startup configuration
    public class ExampleUsage
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        //public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        public static void foo()
        {
            string connectionString = "";
            Microsoft.OData.Edm.IEdmModel model = DynamicODataModelGenerator.CreateDynamicEdmModel(connectionString);
            System.Console.WriteLine(model);
        }

        public void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {
            string connectionString = ""; // _configuration.GetConnectionString("DefaultConnection");

            //services.AddControllers()
            //    .AddOData(opt => opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
            //        .AddModel("odata", DynamicODataModelGenerator.CreateDynamicEdmModel(connectionString)));
        }
    }


}
