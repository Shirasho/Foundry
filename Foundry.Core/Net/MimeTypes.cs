using System;
using System.Net.Http.Headers;

namespace Foundry.Net
{
    /// <summary>
    /// Information about a MIME type.
    /// </summary>
    public readonly struct MimeType : IEquatable<MimeType>
    {
        /// <summary>
        /// The content type.
        /// </summary>
        public readonly string ContentType { get; }

        internal MimeType(string type)
        {
            ContentType = type;
        }

        public bool IsJson()
        {
            return this == Application.ProblemJson ||
                   this == Application.Json ||
                   this == Text.Json;
        }

        /// <summary>
        /// Returns whether <paramref name="contentType"/> represents a JSON content type.
        /// </summary>
        /// <param name="contentType">The content type to check</param>
        public static bool IsJson(string contentType)
        {
            return contentType == Application.ProblemJson ||
                   contentType == Application.Json ||
                   contentType == Text.Json;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// The fully qualified type name.
        /// </returns>
        public override readonly string ToString()
        {
            return ContentType;
        }

        public static implicit operator string(MimeType mimeType)
        {
            return mimeType.ToString();
        }

        public static implicit operator MimeType(string mimeType)
        {
            return new MimeType(mimeType);
        }

        public static bool operator ==(MimeType a, MimeType b)
        {
            return string.Equals(a.ContentType, b.ContentType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(MimeType a, MimeType b)
        {
            return !(a == b);
        }

        public static bool operator ==(MimeType a, string b)
        {
            return string.Equals(a.ContentType, b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(MimeType a, string b)
        {
            return !(a == b);
        }

        public static bool operator ==(string a, MimeType b)
        {
            return string.Equals(a, b.ContentType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(string a, MimeType b)
        {
            return !(a == b);
        }

        public static bool operator ==(MimeType a, MediaTypeHeaderValue b)
        {
            return string.Equals(a.ContentType, b.MediaType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(MimeType a, MediaTypeHeaderValue b)
        {
            return !(a == b);
        }

        public static bool operator ==(MediaTypeHeaderValue a, MimeType b)
        {
            return string.Equals(a.MediaType, b.ContentType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator !=(MediaTypeHeaderValue a, MimeType b)
        {
            return !(a == b);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is MimeType other && Equals(other);
        }

        public readonly bool Equals(MimeType other)
        {
            return string.Equals(ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        }

        public override readonly int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ContentType);
        }

        public static class Application
        {
            public static MimeType AtomXml { get; } = new MimeType("application/atom+xml");
            public static MimeType AtomcatXml { get; } = new MimeType("application/atomcat+xml");
            public static MimeType Ecmascript { get; } = new MimeType("application/ecmascript");
            public static MimeType JavaArchive { get; } = new MimeType("application/java-archive");
            public static MimeType Javascript { get; } = new MimeType("application/javascript");
            public static MimeType Json { get; } = new MimeType("application/json");
            public static MimeType ProblemJson { get; } = new MimeType("application/problem+json");
            public static MimeType ProblemXml { get; } = new MimeType("application/problem+xml");
            public static MimeType Mp4 { get; } = new MimeType("application/mp4");
            public static MimeType OctetStream { get; } = new MimeType("application/octet-stream");
            public static MimeType Pdf { get; } = new MimeType("application/pdf");
            public static MimeType Pkcs10 { get; } = new MimeType("application/pkcs10");
            public static MimeType Pkcs7Mime { get; } = new MimeType("application/pkcs7-mime");
            public static MimeType Pkcs7Signature { get; } = new MimeType("application/pkcs7-signature");
            public static MimeType Pkcs8 { get; } = new MimeType("application/pkcs8");
            public static MimeType Postscript { get; } = new MimeType("application/postscript");
            public static MimeType RdfXml { get; } = new MimeType("application/rdf+xml");
            public static MimeType RssXml { get; } = new MimeType("application/rss+xml");
            public static MimeType Rtf { get; } = new MimeType("application/rtf");
            public static MimeType SmilXml { get; } = new MimeType("application/smil+xml");
            public static MimeType XFontOtf { get; } = new MimeType("application/x-font-otf");
            public static MimeType XFontTtf { get; } = new MimeType("application/x-font-ttf");
            public static MimeType XFontWoff { get; } = new MimeType("application/x-font-woff");
            public static MimeType XPkcs12 { get; } = new MimeType("application/x-pkcs12");
            public static MimeType XShockwaveFlash { get; } = new MimeType("application/x-shockwave-flash");
            public static MimeType XSilverlightApp { get; } = new MimeType("application/x-silverlight-app");
            public static MimeType XhtmlXml { get; } = new MimeType("application/xhtml+xml");
            public static MimeType Xml { get; } = new MimeType("application/xml");
            public static MimeType XmlDtd { get; } = new MimeType("application/xml-dtd");
            public static MimeType XsltXml { get; } = new MimeType("application/xslt+xml");
            public static MimeType Zip { get; } = new MimeType("application/zip");
        }

        public static class Audio
        {
            public static MimeType Midi { get; } = new MimeType("audio/midi");
            public static MimeType Mp4 { get; } = new MimeType("audio/mp4");
            public static MimeType Mpeg { get; } = new MimeType("audio/mpeg");
            public static MimeType Ogg { get; } = new MimeType("audio/ogg");
            public static MimeType Webm { get; } = new MimeType("audio/webm");
            public static MimeType XAac { get; } = new MimeType("audio/x-aac");
            public static MimeType XAiff { get; } = new MimeType("audio/x-aiff");
            public static MimeType XMpegurl { get; } = new MimeType("audio/x-mpegurl");
            public static MimeType XMsWma { get; } = new MimeType("audio/x-ms-wma");
            public static MimeType XWav { get; } = new MimeType("audio/x-wav");
        }

        public static class Image
        {
            public static MimeType Bmp { get; } = new MimeType("image/bmp");
            public static MimeType Gif { get; } = new MimeType("image/gif");
            public static MimeType Jpeg { get; } = new MimeType("image/jpeg");
            public static MimeType Png { get; } = new MimeType("image/png");
            public static MimeType SvgXml { get; } = new MimeType("image/svg+xml");
            public static MimeType Tiff { get; } = new MimeType("image/tiff");
            public static MimeType Webp { get; } = new MimeType("image/webp");
        }

        public static class Text
        {
            public static MimeType Css { get; } = new MimeType("text/css");
            public static MimeType Csv { get; } = new MimeType("text/csv");
            public static MimeType Html { get; } = new MimeType("text/html");
            public static MimeType Plain { get; } = new MimeType("text/plain");
            public static MimeType RichText { get; } = new MimeType("text/richtext");
            public static MimeType Sgml { get; } = new MimeType("text/sgml");
            public static MimeType Yaml { get; } = new MimeType("text/yaml");

            /// <remarks>
            /// Use Application.Json instead.
            /// </remarks>
            public static MimeType Json { get; } = new MimeType("text/json");
        }

        public static class Video
        {
            public static MimeType Threegpp { get; } = new MimeType("video/3gpp");
            public static MimeType H264 { get; } = new MimeType("video/h264");
            public static MimeType Mp4 { get; } = new MimeType("video/mp4");
            public static MimeType Mpeg { get; } = new MimeType("video/mpeg");
            public static MimeType Ogg { get; } = new MimeType("video/ogg");
            public static MimeType Quicktime { get; } = new MimeType("video/quicktime");
            public static MimeType Webm { get; } = new MimeType("video/webm");
        }
    }
}
