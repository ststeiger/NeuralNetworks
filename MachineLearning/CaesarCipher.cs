
namespace MachineLearning
{


    public static class CaesarCipher
    {
        private static readonly System.Collections.Generic.Dictionary<char, int> s_map_char_to_index;
        private static readonly char[] s_alphabet;


        private static string RandomShuffle(string input)
        {
            char[] array = input.ToCharArray();
            System.Random rng = new System.Random();

            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n);
                n--;
                char temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            } // Whend 

            return new string(array);
        } // End Function RandomShuffle 


        static CaesarCipher()
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyzäöü";
            alphabet = alphabet + alphabet.ToUpperInvariant();

            // Good catch: This ensures a randomized alphabet for each instance of the CaesarCipher STATIC class.
            alphabet = RandomShuffle(alphabet);
            s_alphabet = alphabet.ToCharArray();

            // map the alphabet for fast lookups 
            s_map_char_to_index = new System.Collections.Generic.Dictionary<char, int>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                s_map_char_to_index[s_alphabet[i]] = i;
            } // Next i 

        } // End Static Constructor 


        internal static void Test()
        {
            // abcdefghi
            string input = "The five ÄÖÜ boxing wizards jump quickly!äöü";
            input = "Do you understand what I'm saying ? ";

            string soll = "Uif gjwf cpyjoh xjabset kvnq rvjdlmz!";

            // string b64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
            // System.Console.WriteLine(b64);

            for (int i = 8; i < 256; ++i)
            {
                string cypher = Encrypt(input, i);
                string plain = Decrypt(cypher, i);

                System.Console.WriteLine(plain);
            } // Next i 

            System.Console.WriteLine("Finish");
        } // End Sub Test 


        public static string Encrypt(string plaintext, int shift)
        {
            return Caesar(plaintext, shift, true);
        } // End Function Encrypt 


        public static string Decrypt(string ciphertext, int shift)
        {
            return Caesar(ciphertext, shift, false);
        } // End Function Decrypt 


        private static string Caesar(string text, int shift, bool encrypt)
        {
            char[] output = new char[text.Length];
            int direction = encrypt ? 1 : -1;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // This is slow 
                // int index = System.Array.IndexOf(Alphabet, c);
                // if (index != -1) // Found in the alphabet

                // Instead, use a char-to-index-map 
                // if (s_map_char_to_index.ContainsKey(c)) { int index = s_map_char_to_index[c];

                if (s_map_char_to_index.TryGetValue(c, out int index)) // which is the same as bellow 
                {
                    int shiftedIndex = (index + shift * direction + s_alphabet.Length) % s_alphabet.Length;
                    output[i] = s_alphabet[shiftedIndex];
                }
                else
                {
                    output[i] = c; // Keep the character unchanged
                }
            } // Next i 

            return new string(output);
        } // End Function Caesar 


    } // End static class CaesarCipher 


} // End Namespace MachineLearning 
