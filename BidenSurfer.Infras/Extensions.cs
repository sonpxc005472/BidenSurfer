using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace BidenSurfer.Infras
{
    public static class Extensions
    {
        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(section).Bind(model);
            return model;
        }

        public static T ConvertTo<T>(this string input)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromString(input);
            }
            catch (NotSupportedException)
            {
                return default;
            }
        }

        public static string GetGenericTypeName(this Type type)
        {
            string typeName;
            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }

        public static string GetDescription(this Enum value)
        {
            var attributes =
                (DescriptionAttribute[])
                value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static int ToInt(this Enum value)
        {
            return Convert.ToInt32(value);
        }

        public static Guid ToGuid(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? Guid.Empty : Guid.Parse(value);
        }

        public static string ToIntString(this int value, int numberPadleft = 2)
        {
            return value.ToString().PadLeft(numberPadleft, '0');
        }

        public static string SplitCamelCase(this string input)
        {
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }

        public static string FormatToSQLParam(this string input)
        {
            return $"p_{input}";
        }

        /// <summary>
        /// Remove characters from string starting from the nth occurrence of seperating character
        /// </summary>
        /// <param name="input" value="additional\contact\number"></param>
        /// <param name="seperator" value="\"></param>
        /// <param name="occurNo" value="2"></param>
        /// <returns value="additional\contact"></returns>
        public static string GetStringToNthOccurringCharacter(this string input, string seperator, int occurNo)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            if (input.IndexOf(seperator) > 0)
            {
                var inputList = input.Split(seperator).Take(occurNo).ToList();
                return string.Join(seperator, inputList); 
            }
            return input;
        }


        #region CurrencyFormatting
        public static string ToCurrencyFormatWithFraction(this decimal val, int digits = 2)
        {
            if (digits <= 1)
            {
                digits = 1;
            }
            var convertingFormat = string.Concat("{0:#,0.", new string('0', digits), "}");
            return val == 0 ? "0" : string.Format(convertingFormat, val);
        }

        public static string ToTruncatedStringWithFraction(this decimal val, int digits = 1)
        {
            if (digits <= 1)
            {
                digits = 1;
            }
            var computeValue = Math.Truncate(val * (int)Math.Pow(10,digits)) / (int)Math.Pow(10, digits);
            
            return val == 0 ? "0" : computeValue.ToString();
        }               

        public static string ToCurrencyFormatWithFraction(this decimal? val, int digits = 2)
        {
            return val == null ? "0" : val.Value.ToCurrencyFormatWithFraction(digits);
        }

        public static string ToCurrencyFormatWithSeperator(this decimal val)
        {
            return val == 0 ? "0" : string.Format("{0:n}", val);
        }

        public static string ToCurrencyFormatWithSeperator(this decimal? val)
        {
            return (val ?? 0).ToCurrencyFormatWithSeperator();
        }
        #endregion

        #region String
        public static string CapitalizeFirstLetter(this string value)
        {
            if(string.IsNullOrWhiteSpace(value)) return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static bool IsValidGuid(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return Guid.TryParse(value, out _);
        }

        public static string GetFileNameFromUri(this string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return Path.GetFileName(uri.LocalPath);
            }
            return string.Empty;
        }

        public static bool IsValidUrl(this string url)
        {
            bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        public static bool IsValidStorageUrl(this string url, string expectedStorage)
        {
            bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) && url.StartsWith(expectedStorage);
            return result;
        }

        public static string ToValue(this string value)
        {
            return value ?? string.Empty;
        }

        public static string NullIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? null : s;
        }
        public static string NullIfWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        #endregion

        #region IEnumerable       

        public static IEnumerable<IEnumerable<T>> ToBatches<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            int itemsReturned = 0;
            var list = enumerable.ToList(); // Prevent multiple execution of IEnumerable.
            int count = list.Count;
            while (itemsReturned < count)
            {
                int currentBatchSize = Math.Min(batchSize, count - itemsReturned);
                yield return list.GetRange(itemsReturned, currentBatchSize);
                itemsReturned += currentBatchSize;
            }
        }
        #endregion

        public static byte[] CombineByteArrays(this byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        public static string ConvertArrayStringToString(this string input)
        {
            // Use in linq query
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : input.Replace("\"", "").Replace("[", "").Replace("]", "").Trim();
        }
    }
}
