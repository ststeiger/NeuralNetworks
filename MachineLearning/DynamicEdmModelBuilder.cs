
namespace MachineLearning
{


    // using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.OData;
    using Microsoft.OData.Edm;
    using Microsoft.OData.ModelBuilder;


    public static class DynamicEdmModelBuilder
    {


        public static IEdmModel GetEdmModel(string connectionString)
        {

            Microsoft.OData.ModelBuilder.ODataModelBuilder builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();

            // builder.AddEntitySet("MyEntity", builder.AddEntityType(t));

            System.Collections.Generic.List<DatabaseSchema> schema = GetDatabaseSchema(connectionString);
            foreach (DatabaseSchema table in schema)
            {
                Microsoft.OData.ModelBuilder.EntityTypeConfiguration entityType = builder.AddEntityType(new TypeConfiguration(table.TableName));
                foreach (Column column in table.Columns)
                {
                    Microsoft.OData.ModelBuilder.PrimitivePropertyConfiguration property = 
                        entityType.AddProperty(new PrimitivePropertyConfiguration(entityType, column.ColumnName, column.DataType));
                    
                    if (!column.IsNullable)
                    {
                        property.IsRequired();
                    }
                    // else property.IsOptional();
                }

                foreach (string pk in table.PrimaryKeys)
                {
                    // entityType.AddKey(entityType.Properties.Single(p => p.Name == pk));

                }



                //if (table.PrimaryKeys.Count > 0)
                //{
                //    var keyProperties = table.PrimaryKeys.Select(pk => entityType.Property(entityType.Properties.Single(p => p.Name == pk)));
                //    entityType.HasKey(keyProperties.ToArray());
                //}


                builder.AddEntitySet(table.TableName, entityType);
            }

            return builder.GetEdmModel();
        }


        public static System.Data.Common.DbConnection GetConnection(string connectionString)
        {
            return new System.Data.SqlClient.SqlConnection(connectionString);
        }


        private static System.Collections.Generic.List<DatabaseSchema> GetDatabaseSchema(string connectionString)
        {
            System.Collections.Generic.List<DatabaseSchema> schemas = new System.Collections.Generic.List<DatabaseSchema>();

            using (System.Data.Common.DbConnection connection = GetConnection(connectionString))
            {
                connection.Open();

                System.Collections.Generic.Dictionary<string, DatabaseSchema> tables = new System.Collections.Generic.Dictionary<string, DatabaseSchema>();

                // Get tables and columns
                using (System.Data.Common.DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"  SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS";

                    using (System.Data.Common.DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            if (!tables.ContainsKey(tableName))
                            {
                                tables[tableName] = new DatabaseSchema { TableName = tableName };
                            }

                            Column column = new Column
                            {
                                ColumnName = reader.GetString(1),
                                DataType = reader.GetString(2),
                                IsNullable = reader.GetString(3) == "YES"
                            };

                            tables[tableName].Columns.Add(column);
                        }
                    }
                }
                // Get primary keys
                using (System.Data.Common.DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT TABLE_NAME, COLUMN_NAME 
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_NAME), 'IsPrimaryKey') = 1 
";


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tableName = reader.GetString(0);
                            var columnName = reader.GetString(1);
                            if (tables.ContainsKey(tableName))
                            {
                                tables[tableName].PrimaryKeys.Add(columnName);
                            }
                        }
                    }
                }
                schemas.AddRange(tables.Values);
            }

            return schemas;
        }
    }

    public class DatabaseSchema
    {
        public string TableName { get; set; }
        public System.Collections.Generic.List<Column> Columns { get; set; } = new System.Collections.Generic.List<Column>();
        public System.Collections.Generic.List<string> PrimaryKeys { get; set; } = new System.Collections.Generic.List<string>();
    }

    public class Column
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
    }


    public class PrimitivePropertyConfiguration
        : System.Reflection.PropertyInfo //: MemberInfo
    {

        private readonly string _name;
        private readonly System.Type _propertyType;
        private readonly bool _isNullable;

        private static System.Type GetTypeFromDataType(string dataType)
        {
            return dataType switch
            {
                "int" => typeof(int),
                "bigint" => typeof(long),
                "smallint" => typeof(short),
                "tinyint" => typeof(byte),
                "bit" => typeof(bool),
                "decimal" => typeof(decimal),
                "numeric" => typeof(decimal),
                "money" => typeof(decimal),
                "smallmoney" => typeof(decimal),
                "float" => typeof(double),
                "real" => typeof(float),
                "datetime" => typeof(System.DateTime),
                "smalldatetime" => typeof(System.DateTime),
                "char" => typeof(string),
                "varchar" => typeof(string),
                "text" => typeof(string),
                "nchar" => typeof(string),
                "nvarchar" => typeof(string),
                "ntext" => typeof(string),
                "binary" => typeof(byte[]),
                "varbinary" => typeof(byte[]),
                "image" => typeof(byte[]),
                _ => typeof(string),
            };
        }

        public PrimitivePropertyConfiguration(EntityTypeConfiguration entityType, string columnName, string dataType)
        {
            _name = columnName;
            _propertyType = GetTypeFromDataType(dataType);
            //_isNullable = entityType.Properties.Find(p => p.Name == columnName)?.IsNullable ?? false;
        }




        // public override PropertyAttributes Attributes => throw new System.NotImplementedException();

        public override System.Reflection.PropertyAttributes Attributes => System.Reflection.PropertyAttributes.None;


        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override System.Type PropertyType => _propertyType;

        public override System.Type? DeclaringType => throw new System.NotImplementedException();

        public override string Name => throw new System.NotImplementedException();

        public override System.Type? ReflectedType => throw new System.NotImplementedException();

        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new System.NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new System.NotImplementedException();
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.MethodInfo? GetGetMethod(bool nonPublic)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.ParameterInfo[] GetIndexParameters()
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.MethodInfo? GetSetMethod(bool nonPublic)
        {
            throw new System.NotImplementedException();
        }

        public override object? GetValue(object? obj,
            System.Reflection.BindingFlags invokeAttr,
            System.Reflection.Binder? binder, object?[]? index, System.Globalization.CultureInfo? culture)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            throw new System.NotImplementedException();
        }

        public override void SetValue(object? obj, object? value,
            System.Reflection.BindingFlags invokeAttr,
            System.Reflection.Binder? binder, 
            object?[]? index, 
            System.Globalization.CultureInfo? culture
        )
        {
            throw new System.NotImplementedException();
        }
    }



    public class TypeConfiguration
        : System.Type
    {
        private readonly string _name;



        public TypeConfiguration(string name)
        {
            _name = name;
        }

        // Implement other required members of Type abstract class...
        public override System.Reflection.Assembly Assembly => throw new System.NotImplementedException();

        public override string? AssemblyQualifiedName => throw new System.NotImplementedException();

        public override System.Type? BaseType => throw new System.NotImplementedException();

        public override string? FullName => throw new System.NotImplementedException();

        public override System.Guid GUID => throw new System.NotImplementedException();

        public override System.Reflection.Module Module => throw new System.NotImplementedException();

        public override string? Namespace => throw new System.NotImplementedException();

        public override System.Type UnderlyingSystemType => throw new System.NotImplementedException();

        public override string Name => _name;


        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new System.NotImplementedException();
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            throw new System.NotImplementedException();
        }

        public override System.Type? GetElementType()
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.EventInfo? GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.FieldInfo? GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        [return: System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]
        public override System.Type? GetInterface(string name, bool ignoreCase)
        {
            throw new System.NotImplementedException();
        }

        public override System.Type[] GetInterfaces()
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Type? GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            throw new System.NotImplementedException();
        }

        public override object? InvokeMember(string name, 
            System.Reflection.BindingFlags invokeAttr,
            System.Reflection.Binder? binder, object? target, object?[]? args,
            System.Reflection.ParameterModifier[]? modifiers,
            System.Globalization.CultureInfo? culture, string[]? namedParameters)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            throw new System.NotImplementedException();
        }

        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override System.Reflection.ConstructorInfo? GetConstructorImpl(
            System.Reflection.BindingFlags bindingAttr,
            System.Reflection.Binder? binder, 
            System.Reflection.CallingConventions callConvention, 
            System.Type[] types, 
            System.Reflection.ParameterModifier[]? modifiers)
        {
            throw new System.NotImplementedException();
        }

        protected override System.Reflection.MethodInfo? GetMethodImpl(string name,
            System.Reflection.BindingFlags bindingAttr,
            System.Reflection.Binder? binder,
            System.Reflection.CallingConventions callConvention, 
            System.Type[]? types,
            System.Reflection.ParameterModifier[]? modifiers)
        {
            throw new System.NotImplementedException();
        }

        protected override System.Reflection.PropertyInfo? GetPropertyImpl(string name,
            System.Reflection.BindingFlags bindingAttr,
            System.Reflection.Binder? binder, System.Type? returnType, System.Type[]? types,
            System.Reflection.ParameterModifier[]? modifiers)
        {
            throw new System.NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsArrayImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsByRefImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            throw new System.NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new System.NotImplementedException();
        }
    }



}
