
namespace MachineLearning.OData
{
    using Azure.Core;
    using Dapper;


    public class MyUser
    {
        public int UserId;
    }


    internal class UsageWithDapper
    {
        private string TranslateODataFilterToSql(string filterClause, Dapper.DynamicParameters parameters)
        {
            // Implement translation logic for filter expressions (e.g., Name eq 'Alice' -> Name = @Name)
            // Example: assume it parses "Name eq 'Alice'" to "Name = @Name" and adds @Name parameter.
            // This is a simplified example and would require a more robust implementation.
            // Pseudo-code:
            string sqlFilter = filterClause.Replace("eq", "="); // Simplified, actual parsing needed.
            parameters.Add("@Name", "Alice"); // Example parameter assignment
            return sqlFilter;
        }

        private string TranslateODataOrderByToSql(string orderByClause)
        {
            // Implement translation for order by, e.g., "Name desc" -> "Name DESC"
            return orderByClause; // Simplified for example purposes
        }



        private string GenerateNextLink(
            Microsoft.AspNetCore.OData.Query.ODataQueryOptions<MyUser> queryOptions, int skip)
        {
            string rawQueryString = ""; // Request.QueryString.Value

            // Build the next link URL with the incremented skip value
            System.Collections.Specialized.NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(rawQueryString);
            queryString["$skip"] = skip.ToString();
            // return $"{Request.Scheme}://{Request.Host}{Request.Path}?{rawQueryString}";
            return "";
        }


        public async System.Threading.Tasks.Task GetUsers(Microsoft.AspNetCore.OData.Query.ODataQueryOptions<MyUser> queryOptions)
        {
            System.Data.Common.DbConnection cnn = null;

            // Step 1: Base query
            System.Text.StringBuilder sql = new System.Text.StringBuilder("SELECT * FROM Users");
            Dapper.DynamicParameters parameters = new Dapper.DynamicParameters();

            // Step 2: Handle $filter
            if (queryOptions.Filter != null)
            {
                string? filterClause = queryOptions.Filter.ToString();
                sql.Append(" WHERE ").Append(TranslateODataFilterToSql(filterClause, parameters));
            }

            // Step 3: Handle $orderby
            if (queryOptions.OrderBy != null)
            {
                string? orderByClause = queryOptions.OrderBy.ToString();
                sql.Append(" ORDER BY ").Append(TranslateODataOrderByToSql(orderByClause));
            }

            // Step 4: Handle $skip and $top
            if (queryOptions.Skip != null || queryOptions.Top != null)
            {
                int skip = queryOptions.Skip?.Value ?? 0;
                int take = queryOptions.Top?.Value ?? 100; // default limit if not specified
                sql.Append(" OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY");
                parameters.Add("@Skip", skip);
                parameters.Add("@Take", take);
            }

            // Step 5: Execute with Dapper
            System.Collections.Generic.IEnumerable<MyUser> users = await cnn.QueryAsync<MyUser>(sql.ToString(), parameters);
            // return Ok(users);
            // Step 6: Construct OData response
            //var odataResponse = new
            //{
            //    @odata.context = "https://yourservice/odata/$metadata#Users", // Modify as needed
            //    @odata.count = totalCount,
            //    @odata.nextLink = queryOptions.Top.HasValue && users.Count() >= queryOptions.Top.Value
            //        ? GenerateNextLink(queryOptions, skip + take) // Generate URL for the next page
            //        : null,
            //    value = users
            //};

        }


    }
}
