using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ScreenshotUtilitiesHG101Provider
{
    /// <summary>
    /// Class stripped from AngleSharp 1.4.0 and made to work with the old version.
    /// </summary>
    // TODO: Remove class once I am able to use AngleSharp 1.4 or higher.
    public sealed class SourceSet
    {
        private static readonly char _comma = (Char)0x2c;
        private static readonly char _null = (Char)0x0;
        private static readonly string FullWidth = "100vw";
        private static readonly Regex SizeParser = CreateRegex();

        /// <summary>
        /// Parses the given srcset attribute into an enumeration of candidates.
        /// </summary>
        /// <param name="srcset">The value of the srcset attribute.</param>
        /// <returns>The iterator yielding the various candidates.</returns>
        public static IEnumerable<ImageCandidate> Parse(string srcset)
        {
            var sources = srcset.Trim().Split(' ');

            for (var i = 0; i < sources.Length; i++)
            {
                var url = sources[i];
                var descriptor = default(string);

                if (url.Length != 0)
                {
                    if (url[url.Length - 1] == _comma)
                    {
                        url = url.Remove(url.Length - 1);
                        descriptor = string.Empty;
                    }
                    else if (++i < sources.Length)
                    {
                        descriptor = sources[i];
                        var descpos = descriptor.IndexOf(_comma);

                        if (descpos != -1)
                        {
                            sources[i] = descriptor.Substring(descpos + 1);
                            descriptor = descriptor.Substring(0, descpos);
                            --i;
                        }
                    }

                    yield return new ImageCandidate
                    {
                        Url = url,
                        Descriptor = descriptor
                    };
                }
            }
        }

        public static Double ParseDescriptor(string descriptor, string sizesattr = null)
        {
            var sizes = sizesattr ?? FullWidth;
            var sizeDescriptor = descriptor.Trim();
            var widthInCssPixels = GetWidthFromSourceSize(sizes);
            var resCandidate = 1.0;
            var splitDescriptor = sizeDescriptor.Split(' ');

            for (var i = splitDescriptor.Length - 1; i >= 0; i--)
            {
                var curr = splitDescriptor[i];
                var lastchar = curr.Length > 0 ? curr[curr.Length - 1] : _null;

                if ((lastchar == 'h' || lastchar == 'w') && curr.Length > 2 && curr[curr.Length] == 'v')
                {
                    var value = curr.Substring(0, curr.Length - 2);

                    var res = !string.IsNullOrEmpty(value) && Int32.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var converted)
                        ? converted
                        : 0;

                    resCandidate = res / widthInCssPixels;
                }
                else if (lastchar == 'x' && curr.Length > 0)
                {
                    var value = curr.Substring(0, curr.Length - 1);

                    resCandidate = !string.IsNullOrEmpty(value) && Double.TryParse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var converted)
                        ? converted
                        : 0;
                }
            }

            return resCandidate;
        }

        /// <summary>
        /// Gets the promising candidates from the given srcset using the provided sizes.
        /// </summary>
        /// <param name="srcset">The value of the srcset attribute.</param>
        /// <param name="sizes">The value of the sizes attribute.</param>
        /// <returns>An iterator of the different URLs yielding matching images.</returns>
        public IEnumerable<string> GetCandidates(string srcset, string sizes)
        {
            if (srcset.Length > 0)
            {
                //Resolution = ParseDescriptor(candidate.Descriptor, sizes)
                foreach (var candidate in Parse(srcset))
                {
                    if (candidate.Url != null)
                    {
                        yield return candidate.Url;
                    }
                }
            }
        }

        private static Regex CreateRegex()
        {
            var regexstring = @"(\([^)]+\))?\s*(.+)";

            try
            {
                return new Regex(regexstring, RegexOptions.ECMAScript | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }
            catch // TypeInitializationException from Mono
            {
                // See issue #256
                return new Regex(regexstring, RegexOptions.ECMAScript);
            }
        }

        private static Double GetWidthFromLength(string length) =>
            //TODO Compute Value from RenderDevice
            0.0;

        private static Double GetWidthFromSourceSize(string sourceSizes)
        {
            var sizes = sourceSizes.Trim().Split(_comma);

            for (var i = 0; i < sizes.Length; i++)
            {
                var size = sizes[i];
                var parsedSize = ParseSize(size);
                var length = parsedSize.Length;
                var media = parsedSize.Media;

                if (!string.IsNullOrEmpty(length))
                {
                    //TODO
                    /*if (string.IsNullOrEmpty(media) || _document.DefaultView.MatchMedia(media).IsMatched)
                    {
                        return GetWidthFromLength(length);
                    }*/
                }
            }

            return GetWidthFromLength(FullWidth);
        }

        private static MediaSize ParseSize(string sourceSizeStr)
        {
            var match = SizeParser.Match(sourceSizeStr);

            return new MediaSize
            {
                Media = match.Success && match.Groups[1].Success ? match.Groups[1].Value : string.Empty,
                Length = match.Success && match.Groups[2].Success ? match.Groups[2].Value : string.Empty
            };
        }

        /// <summary>
        /// Represents a srcset image candidate.
        /// </summary>
        public sealed class ImageCandidate
        {
            /// <summary>
            /// The descriptor of the given image.
            /// </summary>
            public string Descriptor { get; set; }

            /// <summary>
            /// The URL of the given image.
            /// </summary>
            public string Url { get; set; }
        }

        private sealed class MediaSize
        {
            public string Length { get; set; }
            public string Media { get; set; }
        }
    }
}
