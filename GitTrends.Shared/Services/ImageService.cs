using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GitTrends.Shared
{
    // https://stackoverflow.com/a/40831310/5953643
    static class ImageService
    {
        const string ErrorMessage = "Could not read image data";
        const int ChunkSize = 1024;

        static readonly Lazy<HttpClient> _clientHolder = new Lazy<HttpClient>(() => new HttpClient { Timeout = HttpClientTimeout });

        public static TimeSpan HttpClientTimeout { get; } = TimeSpan.FromSeconds(1);

        static HttpClient Client => _clientHolder.Value;

        static readonly Dictionary<byte[], Func<BinaryReader, Size>> ImageFormatDecoders = new Dictionary<byte[], Func<BinaryReader, Size>>()
        {
            { new byte[]{ 0x42, 0x4D }, DecodeBitmap},
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, DecodeGif },
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, DecodeGif },
            { new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePng },
            { new byte[]{ 0xff, 0xd8 }, DecodeJfif },
        };

        /// <summary>
        /// Gets the dimensions of an image.
        /// </summary>
        /// <returns>The dimensions of the specified image.</returns>
        /// <exception cref="ArgumentException">The image was of an unrecognized format.</exception>    
        public static Size GetDimensions(BinaryReader binaryReader)
        {
            int maxMagicBytesLength = ImageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;

            byte[] magicBytes = new byte[maxMagicBytesLength];

            for (int i = 0; i < maxMagicBytesLength; i += 1)
            {
                magicBytes[i] = binaryReader.ReadByte();

                foreach (var kvPair in ImageFormatDecoders)
                {
                    if (ByteStartsWith(magicBytes, kvPair.Key))
                    {
                        return kvPair.Value(binaryReader);
                    }
                }
            }

            throw new ArgumentException(ErrorMessage, nameof(binaryReader));
        }

        /// <summary>
        /// Retrieve the dimensions of an online image, downloading as little as possible
        /// </summary>
        public static async Task<Size> GetWebDimensions(Uri uri)
        {
            var moreBytes = true;
            var currentStart = 0;
            byte[] allBytes = { };

            while (moreBytes)
            {
                try
                {
                    var newBytes = await GetSomeBytes(uri, currentStart, currentStart + ChunkSize - 1).ConfigureAwait(false);
                    if (newBytes.Length < ChunkSize)
                        moreBytes = false;

                    allBytes = Combine(allBytes, newBytes);
                    return GetDimensions(new BinaryReader(new MemoryStream(allBytes)));
                }
                catch
                {
                    currentStart += ChunkSize;
                }
            }

            return new Size(0, 0);
        }

        static async Task<byte[]> GetSomeBytes(Uri uri, int startRange, int endRange)
        {
            var request = new HttpRequestMessage { RequestUri = uri };
            request.Headers.Range = new RangeHeaderValue(startRange, endRange);
            try
            {
                var response = await Client.SendAsync(request).ConfigureAwait(false);
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            return new byte[] { };
        }

        // from https://stackoverflow.com/a/415839/3838199
        static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        static bool ByteStartsWith(byte[] thisBytes, byte[] thatBytes)
        {
            for (int i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        static short ReadLittleEndianInt16(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(short)];
            for (int i = 0; i < sizeof(short); i += 1)
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        static int ReadLittleEndianInt32(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        static Size DecodeBitmap(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(16);
            int width = binaryReader.ReadInt32();
            int height = binaryReader.ReadInt32();
            return new Size(width, height);
        }

        static Size DecodeGif(BinaryReader binaryReader)
        {
            int width = binaryReader.ReadInt16();
            int height = binaryReader.ReadInt16();
            return new Size(width, height);
        }

        static Size DecodePng(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(8);
            int width = ReadLittleEndianInt32(binaryReader);
            int height = ReadLittleEndianInt32(binaryReader);
            return new Size(width, height);
        }

        static Size DecodeJfif(BinaryReader binaryReader)
        {
            while (binaryReader.ReadByte() is 0xff)
            {
                byte marker = binaryReader.ReadByte();
                short chunkLength = ReadLittleEndianInt16(binaryReader);

                if (marker is 0xc0 || marker is 0xc1 || marker is 0xc2)
                {
                    binaryReader.ReadByte();

                    int height = ReadLittleEndianInt16(binaryReader);
                    int width = ReadLittleEndianInt16(binaryReader);
                    return new Size(width, height);
                }

                binaryReader.ReadBytes(chunkLength - 2);
            }

            throw new ArgumentException(ErrorMessage);
        }
    }
}