using System.Text;

namespace StockPrice.Methods
{
    /// <summary>
    /// Class for text modifications
    /// </summary>
    public class TextConvert
    {
        /// <summary>
        /// Convert a string value to Base64 format and back string
        /// </summary>
        /// <param name="input_string"></param>
        /// <returns></returns>
        public static string ToBase64String(string input_string)
        {
            if (input_string == null) return null;
            else
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(input_string));
            }
        }
    }
}
