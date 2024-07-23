
namespace MachineLearning
{



    public class MySqlTokenizerTests
    {


        public static System.Collections.Generic.IEnumerable<string> TestCases
        {
            get 
            {
                yield return @"SELECT COALESCE((rtrim(ltrim(isnull([AL_Nr],'')))), '123') ";
                yield return @"SELECT /*123 as abc, 'def' as def, 111 as ghi*/ 222 as ijk ";


                // Mind you, if comment removed, there must be preceding or trailing whitespace 
                yield return @"SELECT 123/**/abc";
                yield return @"SELECT 123 abc/*test*/";

                yield return @"SELECT 123 as [abc []]def] -- escape sequence ]]";
                yield return @"SELECT SELECT 456 as ""uvw """"xyz"" -- escape sequence """"";
                yield return @"SELECT 
                123.456e AS abc 
,'ni hao /* 123 as abc -- hello d''Alambert */ how are you ' 


/*
                -- ,'def' AS def */
 
                ,'true' AS wahr 
-- ,'false' AS falsch 
                ,'ciao' AS hello

,123 AS [abc []]def]
,345 AS ""abc def""

,123 as [abc []]def] -- escape sequence ]]
,456 as ""uvw """"xyz"" -- escape sequence """"


WHERE 1=@myparam123

";

                yield return @"SELECT 
select 
     s.name AS schema_name
	,t.name AS table_name
	,c.name AS column_name
	,cc.name 
    ,cc.definition AS computed_column_definition
FROM sys.computed_columns AS cc 
INNER JOIN sys.columns c ON c.object_id = cc.object_id 
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE cc.is_computed = 1
AND cc.definition NOT LIKE '%try_cast%'

ORDER BY table_name 


                
                ";


                foreach (string file in System.IO.Directory.EnumerateFiles(@"D:\stefan.steiger\Documents\Visual Studio 2022\TFS", "*.sql", System.IO.SearchOption.AllDirectories))
                {
                    string sql = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
                    yield return sql;
                } // Next file 

            } // End Getter 

        } // End Property TestCases 



    } // End Class MySqlTokenizerTests 


} // End Namespace 
