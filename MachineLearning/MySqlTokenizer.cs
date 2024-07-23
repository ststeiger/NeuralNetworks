
namespace MachineLearning
{


    // Round brackets() are used for methods. aka Parentheses 
    // Square brackets[] are used for arrays. aka brackets
    // Curly brackets { } are used to set scope. aka Braces  
    // Angle brackets <> are used to delimit variables/types/expressions. aka Chevrons / Brokets  (blend of broken & bracket)

    public enum TokenType
    {
        Keyword,
        Identifier,
        Number,
        StringLiteral,

        Dot,
        Comma,
        Semicolon,

        OpenParenthesis,
        CloseParenthesis,

        OpenCurlyBracket,
        CloseCurlyBracket,



        Operator,
        Punctuation,
        Whitespace,
        SingleLineComment,
        MultiLineComment,

        BracketIdentifier,
        QuotedIdentifier,
        Variable,
        LocalTempTableName,
        GlobalTempTableName


            , Select
            , Coalesce

    } // End Enum TokenType 


    [System.Diagnostics.DebuggerDisplay("{Type}: {Value}")]
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }


        protected string m_rawValue;

        public string RawValue
        {
            get
            {
                if (this.m_rawValue != null)
                    return this.m_rawValue;

                return this.Value;
            }
            set { this.m_rawValue = value; }
        }


        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    } // End Class Token 



    internal class MySqlTokenizer
    {


        internal static void Test()
        {
            foreach (string fileContent in MySqlTokenizerTests.TestCases)
            {
                string sql = fileContent;
                // sql = @"{ ""hello"": ""world""} ";
                // sql = "https://www.example.com/foo?abc=def#test123;";

                System.Collections.Generic.List<Token> tokens = Tokenize(sql);
                // System.Console.WriteLine(tokens);
                string compare = Detokenize(tokens);

                if (!System.StringComparer.Ordinal.Equals(sql, compare))
                    System.Console.WriteLine("foo", sql, compare);

                // System.Console.WriteLine(sql);
            } // Next fileContent 

            System.Console.WriteLine("finished");
        } // End Sub Test 



        public static void Log(string where)
        {
#if false
            System.Console.WriteLine(where);
#endif
        }


        public static string Detokenize(System.Collections.Generic.List<Token> tokens, bool removeComments)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();


            for (int i = 0; i < tokens.Count; ++i)
            {
                bool addWhitespace = false;
                bool isComment = false;

                if (removeComments)
                {
                    isComment = tokens[i].Type == TokenType.MultiLineComment || tokens[i].Type == TokenType.SingleLineComment;

                    if (isComment)
                    {
                        bool leadingWhitespace = false;
                        bool trailingWhitespace = true; // so nothing gets appended afterwards if it's the end of the token list  

                        if (i > 0)
                            leadingWhitespace = tokens[i - 1].Type == TokenType.Whitespace;

                        if (i + 1 < tokens.Count)
                            trailingWhitespace = tokens[i + 1].Type == TokenType.Whitespace;

                        if (!leadingWhitespace && !trailingWhitespace)
                            addWhitespace = true;
                    }
                }

                if (addWhitespace)
                    sb.Append(' ');

                if (tokens[i].Type == TokenType.QuotedIdentifier)
                    sb.Append('"');

                if (tokens[i].Type == TokenType.BracketIdentifier)
                    sb.Append('[');

                if (tokens[i].Type == TokenType.StringLiteral)
                    sb.Append('\'');

                if (removeComments)
                {
                    if (!isComment)
                        sb.Append(tokens[i].RawValue);
                }
                else
                    sb.Append(tokens[i].RawValue);

                if (tokens[i].Type == TokenType.StringLiteral)
                    sb.Append('\'');

                if (tokens[i].Type == TokenType.QuotedIdentifier)
                    sb.Append('"');

                if (tokens[i].Type == TokenType.BracketIdentifier)
                    sb.Append(']');


                if (addWhitespace)
                    sb.Append(' ');
            } // Next i 

            string ret = sb.ToString();
            sb.Clear();
            return ret;
        } // End Function Detokenize 


        public static string Detokenize(System.Collections.Generic.List<Token> tokens)
        {
            return Detokenize(tokens, false);
        } // End Function Detokenize 


        public static string RemoveComments(string sql)
        {
            System.Collections.Generic.List<Token> tokens = Tokenize(sql);
            // System.Console.WriteLine(tokens);
            string uncommented = Detokenize(tokens, true);
            tokens.Clear();

            return uncommented;
        } // End Function RemoveComments



        public static System.Collections.Generic.List<Token> Tokenize(string sql)
        {
            System.Collections.Generic.List<Token> tokens = new System.Collections.Generic.List<Token>();
            int position = 0;

            while (position < sql.Length)
            {
                char currentChar = sql[position];

                if (char.IsWhiteSpace(currentChar))
                {
                    Log("Whitespace");

                    int start = position;
                    while (position < sql.Length && char.IsWhiteSpace(sql[position]))
                    {
                        position++;
                    }
                    tokens.Add(new Token { Type = TokenType.Whitespace, Value = sql.Substring(start, position - start), StartIndex = start, EndIndex = position });
                }
                else if (char.IsLetter(currentChar) || currentChar == '_') // valid identifiers can start with _
                {
                    Log("Identifier");

                    int start = position;
                    while (position < sql.Length && (char.IsLetterOrDigit(sql[position]) || sql[position] == '_' || sql[position] == '$' || sql[position] == '#' || sql[position] == ':')) // geography::point
                    {
                        position++;
                    }
                    string tokenValue = sql.Substring(start, position - start);
                    tokens.Add(new Token { Type = IsKeyword(tokenValue) ? TokenType.Keyword : TokenType.Identifier, Value = tokenValue, StartIndex = start, EndIndex = position });
                }


                else if (currentChar == '@')
                {
                    Log("Variable");

                    int start = position;
                    position++;
                    while (position < sql.Length && (char.IsLetterOrDigit(sql[position]) || sql[position] == '_' || sql[position] == '$' || sql[position] == '#'))
                    {
                        position++;
                    }
                    string tokenValue = sql.Substring(start, position - start);
                    tokens.Add(new Token { Type = TokenType.Variable, Value = tokenValue, StartIndex = start, EndIndex = position });
                }



                else if (currentChar == '#')
                {
                    Log("TempTable");

                    TokenType t = TokenType.LocalTempTableName;

                    int start = position;
                    position++;


                    if (position < sql.Length && sql[position] == '#')
                    {
                        t = TokenType.GlobalTempTableName;
                        position++;
                    }

                    while (position < sql.Length && (char.IsLetterOrDigit(sql[position]) || sql[position] == '_' || sql[position] == '$' || sql[position] == '#'))
                    {
                        position++;
                    }
                    string tokenValue = sql.Substring(start, position - start);
                    tokens.Add(new Token { Type = t, Value = tokenValue, StartIndex = start, EndIndex = position });
                }



                //else if (char.IsDigit(currentChar))
                //{
                //    int start = position;
                //    while (position < sql.Length && char.IsDigit(sql[position]))
                //    {
                //        position++;
                //    }
                //    tokens.Add(new Token { Type = TokenType.Number, Value = sql.Substring(start, position - start), StartIndex = start, EndIndex = position });
                //}



                else if (char.IsDigit(currentChar))
                {
                    Log("Number");

                    int start = position;
                    bool hasDecimalPoint = false;
                    bool hasExponent = false;

                    while (position < sql.Length)
                    {
                        if (char.IsDigit(sql[position]))
                        {
                            position++;
                        }
                        else if (sql[position] == '.' && !hasDecimalPoint)
                        {
                            hasDecimalPoint = true;
                            position++;
                        }
                        else if (char.ToUpper(sql[position]) == 'E' && !hasExponent)
                        {
                            hasExponent = true;
                            position++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    tokens.Add(new Token { Type = TokenType.Number, Value = sql.Substring(start, position - start), StartIndex = start, EndIndex = position });
                }


                //else if (currentChar == '\'')
                //{
                //    int start = position;
                //    position++;
                //    while (position < sql.Length && sql[position] != '\'')
                //    {
                //        position++;
                //    }
                //    position++; // Skip closing quote
                //    tokens.Add(new Token { Type = TokenType.String, Value = sql.Substring(start + 1, position - start - 2), StartIndex = start, EndIndex = position });
                //}

                else if (currentChar == '\'')
                {
                    Log("String");

                    int start = position;
                    position++;
                    while (position < sql.Length)
                    {
                        if (sql[position] == '\'')
                        {
                            if (position + 1 < sql.Length && sql[position + 1] == '\'')
                            {
                                // Escape sequence: two consecutive single quotes
                                position++;
                            }
                            else
                            {
                                break; // End of string
                            }
                        }
                        position++;
                    }
                    if (position >= sql.Length)
                    {
                        throw new System.Exception("Unterminated string");
                    }
                    position++; // Skip closing quote

                    string raw = sql.Substring(start + 1, position - start - 2);
                    string val = raw.Replace("''", "'");

                    tokens.Add(new Token { Type = TokenType.StringLiteral, Value = val, RawValue = raw, StartIndex = start, EndIndex = position });
                }

                // single-line comments 
                else if (currentChar == '-' && position + 1 < sql.Length && sql[position + 1] == '-')
                {
                    Log("Singleline comment");

                    int start = position;
                    position += 2;
                    while (position < sql.Length && sql[position] != '\r' && sql[position] != '\n')
                    {
                        char c = sql[position];
                        position++;
                    }
                    tokens.Add(new Token { Type = TokenType.SingleLineComment, Value = sql.Substring(start, position - start), StartIndex = start, EndIndex = position });
                }

                // Multiline comments  
                else if (currentChar == '/' && position + 1 < sql.Length && sql[position + 1] == '*')
                {
                    Log("Multiline comment");

                    int start = position;
                    position += 2;
                    while (position + 1 < sql.Length && (sql[position] != '*' || sql[position + 1] != '/'))
                    {
                        position++;
                    }
                    position += 2; // Skip closing comment
                    tokens.Add(new Token { Type = TokenType.MultiLineComment, Value = sql.Substring(start, position - start), StartIndex = start, EndIndex = position });
                }

                // Quoted identifiers 
                //else if (currentChar == '[')
                //{
                //    int start = position;
                //    position++;
                //    while (position < sql.Length && sql[position] != ']')
                //    {
                //        position++;
                //    }
                //    if (position >= sql.Length)
                //    {
                //        throw new System.Exception("Unterminated quoted identifier");
                //    }
                //    position++; // Skip closing bracket
                //    tokens.Add(new Token { Type = TokenType.BracketIdentifier, Value = sql.Substring(start + 1, position - start - 2), StartIndex = start, EndIndex = position });
                //}



                else if (currentChar == '[')
                {
                    Log("Bracket identifier");

                    int start = position;
                    position++;
                    while (position < sql.Length)
                    {
                        if (sql[position] == ']')
                        {
                            if (position + 1 < sql.Length && sql[position + 1] == ']')
                            {
                                // Escape sequence: two consecutive closing brackets
                                position++;
                            }
                            else
                            {
                                break; // End of identifier
                            }
                        }
                        position++;
                    }
                    if (position >= sql.Length)
                    {
                        throw new System.Exception("Unterminated bracket identifier");
                    }
                    position++; // Skip closing bracket


                    string rawVal = sql.Substring(start + 1, position - start - 2);
                    string val = rawVal.Replace("]]", "]");

                    tokens.Add(new Token { Type = TokenType.BracketIdentifier, Value = val, RawValue = rawVal, StartIndex = start, EndIndex = position });
                }



                //else if (currentChar == '"')
                //{
                //    int start = position;
                //    position++;
                //    while (position < sql.Length && sql[position] != '"')
                //    {
                //        position++;
                //    }
                //    if (position >= sql.Length)
                //    {
                //        throw new System.Exception("Unterminated quoted identifier");
                //    }
                //    position++; // Skip closing quote
                //    tokens.Add(new Token { Type = TokenType.QuotedIdentifier, Value = sql.Substring(start + 1, position - start - 2), StartIndex = start, EndIndex = position });
                //}


                else if (currentChar == '"')
                {
                    Log("Quoted identifier");

                    int start = position;
                    position++;
                    while (position < sql.Length)
                    {
                        if (sql[position] == '"')
                        {
                            if (position + 1 < sql.Length && sql[position + 1] == '"')
                            {
                                // Escape sequence: two consecutive closing brackets
                                position++;
                            }
                            else
                            {
                                break; // End of identifier
                            }
                        }
                        position++;
                    }
                    if (position >= sql.Length)
                    {
                        throw new System.Exception("Unterminated quoted identifier");
                    }
                    position++; // Skip closing bracket

                    string rawVal = sql.Substring(start + 1, position - start - 2);
                    string val = rawVal.Replace("\"\"", "\"");

                    tokens.Add(new Token { Type = TokenType.QuotedIdentifier, Value = val, RawValue = rawVal, StartIndex = start, EndIndex = position });
                }



                else if (currentChar == '(')
                {
                    Log("OpenParenthesis");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.OpenParenthesis, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }
                else if (currentChar == ')')
                {
                    Log("CloseParenthesis");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.CloseParenthesis, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }

                // ODBC escape-sequences

                else if (currentChar == '{')
                {
                    Log("OpenCurlyBracket");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.OpenCurlyBracket, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }
                else if (currentChar == '}')
                {
                    Log("CloseCurlyBracket");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.CloseCurlyBracket, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }


                else if (currentChar == ',')
                {
                    Log("Comma");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.Comma, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }


                else if (currentChar == '.')
                {
                    Log("Dot");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.Dot, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }



                else if (currentChar == ';')
                {
                    Log("Semicolon");

                    // Handle punctuation
                    tokens.Add(new Token { Type = TokenType.Semicolon, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }


                else if (IsOperator(currentChar))
                {
                    Log("Operator");

                    // Handle operators
                    tokens.Add(new Token { Type = TokenType.Operator, Value = currentChar.ToString(), StartIndex = position, EndIndex = position });
                    position++;
                }
                else
                {
                    Log("ERRRRRRRRRRRRRRRRORRRRRRRRRRRRRRRRRRRRRRRRRRRR");

                    // Handle unexpected characters



                    System.Console.WriteLine(currentChar);
                    string xxx = sql.Substring(0, position);
                    System.Console.WriteLine(xxx);



                    System.Console.WriteLine(sql);
                    throw new System.Exception($"Unexpected character: {currentChar}");
                }
            } // Whend 

            return tokens;
        } // End Function Tokenize 


        // Helper methods for keywords, operators, punctuation
        private static bool IsKeyword(string word)
        {
            if ("select".Equals(word, System.StringComparison.InvariantCultureIgnoreCase))
                return true;

            // Implement keyword checking
            return false;
        } // End Function IsKeyword 


        private static bool IsOperator(char c)
        {
            if (c == '~') return true;
            if (c == '&') return true;
            if (c == '|') return true;
            if (c == '<') return true;
            if (c == '>') return true;
            if (c == '=') return true;
            if (c == '!') return true;

            if (c == '^') return true;
            if (c == '+') return true;
            if (c == '-') return true;
            if (c == '*') return true;
            if (c == '/') return true;
            if (c == '%') return true;

            if (c == '\\') return true; // for XML content 
            if (c == ':') return true; // for json
            if (c == '?') return true; // for url
            // if (c == '$') return true; // for javascript

            // Implement operator checking
            return false;
        } // End Function IsOperator 


        private static bool IsBracket(char c)
        {
            if (c == '(') return true;
            if (c == ')') return true;

            // Implement operator checking
            return false;
        } // End Function IsBracket 


    } // End Class MySqlTokenizer 


} // End Namespace 

