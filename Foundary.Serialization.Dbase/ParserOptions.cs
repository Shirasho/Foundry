using System.Globalization;
using System.Text;

namespace Foundary.Serialization.Dbase
{
    public class ParserOptions
    {
        /// <summary>
        /// The encoding to use for reading the underlying DBF file.
        /// Defaults to <see cref="Encoding.ASCII"/>.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;

        /// <summary>
        /// The culture to use when parsing numbers and dates.
        /// Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Whether dates are to be parsed using <see cref="CultureInfo.InvariantCulture"/>
        /// instead of the culture specified in <see cref="Culture"/>.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool InvariantDates { get; set; }

        /// <summary>
        /// Whether numbers are to be parsed using <see cref="CultureInfo.InvariantCulture"/>
        /// instead of the culture specified in <see cref="Culture"/>.
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public bool InvariantNumbers { get; set; }
    }
}
