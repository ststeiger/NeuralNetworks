
namespace MachineLearning
{


    public class SqlNode
    {
        public TokenType Type { get; set; }
        public System.Collections.Generic.List<SqlNode> Children { get; set; }
        public string Value { get; set; }
    } // End Class SqlNode 


    public class MySqlParser
    {


        internal static void Test()
        {
            SqlNode parsedNode = Parse("SELECT COALESCE((rtrim(ltrim(isnull([AL_Nr],'')))), '123') ");
            System.Console.WriteLine(parsedNode);
        } // End Sub Test 



        public static SqlNode Parse(string sql)
        {
            // Tokenize the SQL statement
            System.Collections.Generic.List<Token> tokens = MySqlTokenizer.Tokenize(sql);

            // Parse the token stream into a tree
            return ParseExpression(tokens);
        } // End Function Parse 


        private static SqlNode ParseExpression(System.Collections.Generic.List<Token> tokens)
        {
            // Implement parsing logic here
            // Recursively parse the token stream based on the grammar rules
            // Create SqlNode objects to represent the parsed expression
            
            // Token token = tokens[0];
            Token token = System.Linq.Enumerable.First(tokens);
            tokens.RemoveAt(0);

            switch (token.Type)
            {
                case TokenType.Identifier:
                    return new SqlNode { Type = TokenType.Identifier, Value = token.Value };
                case TokenType.StringLiteral:
                    return new SqlNode { Type = TokenType.StringLiteral, Value = token.Value };
                // ... other expression types

                case TokenType.Select:
                    // Parse SELECT clause
                    break;
                case TokenType.Coalesce:
                    // Parse COALESCE expression
                    var coalesceNode = new SqlNode { Type = TokenType.Coalesce, Children = new System.Collections.Generic.List<SqlNode>() };
                    while (token.Type != TokenType.CloseParenthesis)
                    {
                        coalesceNode.Children.Add(ParseExpression(tokens));
                        // token = tokens.First();
                        token = System.Linq.Enumerable.First(tokens);
                        tokens.RemoveAt(0);
                        if (token.Type == TokenType.Comma)
                        {
                            tokens.RemoveAt(0);
                        }
                    }
                    return coalesceNode;
                // ... other cases for other keywords and expressions
                default:
                    throw new System.Exception("Unexpected token");
            } // End switch 

            return null;
        } // End Function ParseExpression 


    } // End Class MySqlParser 


} // End Namespace 
