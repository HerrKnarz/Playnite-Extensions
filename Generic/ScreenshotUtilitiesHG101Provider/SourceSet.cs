using System;
using System.Collections.Generic;

namespace ScreenshotUtilitiesHG101Provider
{
    /// <summary>
    /// Class stripped from AngleSharp 1.4.0 and made to work with the old version.
    /// </summary>
    // TODO: Remove class once I am able to use AngleSharp 1.4 or higher.
    public sealed class SourceSet
    {
        private static readonly char _comma = (Char)0x2c;

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
                        Descriptor = GetWidth(descriptor)
                    };
                }
            }
        }

        private static int GetWidth(string descriptor) => Int32.TryParse(descriptor?.Replace("w", ""), out var result) ? result : 0;

        /// <summary>
        /// Represents a srcset image candidate.
        /// </summary>
        public sealed class ImageCandidate
        {
            /// <summary>
            /// The descriptor of the given image.
            /// </summary>
            public int Descriptor { get; set; }

            /// <summary>
            /// The URL of the given image.
            /// </summary>
            public string Url { get; set; }
        }
    }
}
