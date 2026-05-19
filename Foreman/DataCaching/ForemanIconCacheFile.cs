using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp;

namespace Foreman.DataCaching {
    /// <summary>FOIC v1: preset icon cache (.dat) — UTF-8 names, PNG blobs, zstd-compressed payload.</summary>
    public static class ForemanIconCacheFile {
        public const ushort FormatVersion = 1;
        private static ReadOnlySpan<byte> Magic => MagicBytes;
        private static readonly byte[] MagicBytes = "FOIC"u8.ToArray();

        private const int HeaderSize = 20;
        private const int CompressedSizeHeaderOffset = 16;
        private const int MaxNameByteLength = 512;
        private const int MaxPngByteLength = 4 * 1024 * 1024;
        private const int DefaultCompressionLevel = 19;

        public static bool IsFoicFile(string path) {
            using var stream = File.OpenRead(path);
            return stream.Length >= HeaderSize && IsFoicStream(stream);
        }

        public static bool IsFoicStream(Stream stream) {
            Span<byte> magic = stackalloc byte[4];
            return stream.Read(magic) == 4 && magic.SequenceEqual(Magic);
        }

        public static Task WriteAsync(string path, IReadOnlyDictionary<string, IconColorPair> icons, CancellationToken cancellationToken = default) =>
            WriteAsync(path, icons, DefaultCompressionLevel, cancellationToken);

        public static async Task WriteAsync(string path, IReadOnlyDictionary<string, IconColorPair> icons, int compressionLevel, CancellationToken cancellationToken = default) {
            var entries = new List<(byte[] NameUtf8, int ColorArgb, byte[] PngBytes)>(icons.Count);
            foreach (var kvp in icons.OrderBy(static e => e.Key, StringComparer.Ordinal)) {
                cancellationToken.ThrowIfCancellationRequested();

                byte[] nameUtf8 = Encoding.UTF8.GetBytes(kvp.Key);
                if (nameUtf8.Length is 0 or > MaxNameByteLength)
                    throw new InvalidDataException($"Icon name length out of range: \"{kvp.Key}\".");

                byte[] pngBytes = EncodePng(kvp.Value.Icon);
                if (pngBytes.Length > MaxPngByteLength)
                    throw new InvalidDataException($"PNG for \"{kvp.Key}\" exceeds maximum size.");

                entries.Add((nameUtf8, kvp.Value.Color.ToArgb(), pngBytes));
            }

            uint entryCount = (uint)entries.Count;
            uint uncompressedSize = (uint)CalculateUncompressedPayloadSize(entries);

            using var output = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize: 64 * 1024, useAsync: true);
            await WriteHeaderAsync(output, entryCount, uncompressedSize, compressedSize: 0, cancellationToken).ConfigureAwait(false);

            long compressedPayloadStart = output.Position;
            using (var compressionStream = new CompressionStream(output, compressionLevel, leaveOpen: true))
                await WritePayloadAsync(compressionStream, entries, cancellationToken).ConfigureAwait(false);

            uint compressedSize = (uint)(output.Position - compressedPayloadStart);
            await PatchCompressedSizeAsync(output, compressedSize, cancellationToken).ConfigureAwait(false);
        }

        public static Task<Dictionary<string, IconColorPair>> ReadAsync(
            string path,
            IProgress<(int Decoded, int Total)>? progress = null,
            CancellationToken cancellationToken = default) =>
            Task.Run(async () => {
                using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 64 * 1024, useAsync: true);
                ReadHeader(input, out uint entryCount, out uint uncompressedSize, out uint compressedSize);

                long expectedEndPosition = HeaderSize + compressedSize;
                if (input.Length != expectedEndPosition)
                    throw new InvalidDataException("FOIC file length does not match header.");

                progress?.Report((0, (int)entryCount));

                using var decompressionStream = new DecompressionStream(input);
                var iconCache = await ParsePayloadAsync(decompressionStream, entryCount, uncompressedSize, progress, cancellationToken).ConfigureAwait(false);

                return input.Position != expectedEndPosition ? throw new InvalidDataException("FOIC compressed payload size mismatch.") : iconCache;
            }, cancellationToken);

        private static int CalculateUncompressedPayloadSize(IReadOnlyList<(byte[] NameUtf8, int ColorArgb, byte[] PngBytes)> entries) {
            int size = sizeof(int);
            foreach (var (nameUtf8, _, pngBytes) in entries)
                size += sizeof(ushort) + nameUtf8.Length + sizeof(int) + sizeof(int) + pngBytes.Length;
            return size;
        }

        private static async Task WritePayloadAsync(Stream output, List<(byte[] NameUtf8, int ColorArgb, byte[] PngBytes)> entries, CancellationToken cancellationToken) {
            await WriteInt32Async(output, entries.Count, cancellationToken).ConfigureAwait(false);

            foreach (var (nameUtf8, colorArgb, pngBytes) in entries) {
                cancellationToken.ThrowIfCancellationRequested();
                await WriteUInt16Async(output, (ushort)nameUtf8.Length, cancellationToken).ConfigureAwait(false);
                await output.WriteAsync(nameUtf8, cancellationToken).ConfigureAwait(false);
                await WriteInt32Async(output, colorArgb, cancellationToken).ConfigureAwait(false);
                await WriteInt32Async(output, pngBytes.Length, cancellationToken).ConfigureAwait(false);
                if (pngBytes.Length > 0)
                    await output.WriteAsync(pngBytes, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<Dictionary<string, IconColorPair>> ParsePayloadAsync(
            Stream payload,
            uint expectedEntryCount,
            uint expectedUncompressedSize,
            IProgress<(int Decoded, int Total)>? progress,
            CancellationToken cancellationToken) {
            long payloadBytesRead = sizeof(int);
            int payloadEntryCount = await ReadInt32Async(payload, cancellationToken).ConfigureAwait(false);
            if (payloadEntryCount < 0 || (uint)payloadEntryCount != expectedEntryCount)
                throw new InvalidDataException("FOIC entry count mismatch.");

            var iconCache = new Dictionary<string, IconColorPair>(payloadEntryCount, StringComparer.Ordinal);
            for (int i = 0; i < payloadEntryCount; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                int nameByteLength = await ReadUInt16Async(payload, cancellationToken).ConfigureAwait(false);
                payloadBytesRead += sizeof(ushort);
                if (nameByteLength is 0 or > MaxNameByteLength)
                    throw new InvalidDataException("FOIC icon name length out of range.");

                string name = await ReadUtf8StringAsync(payload, nameByteLength, cancellationToken).ConfigureAwait(false);
                payloadBytesRead += nameByteLength;

                int colorArgb = await ReadInt32Async(payload, cancellationToken).ConfigureAwait(false);
                payloadBytesRead += sizeof(int);

                int pngByteLength = await ReadInt32Async(payload, cancellationToken).ConfigureAwait(false);
                payloadBytesRead += sizeof(int);
                if (pngByteLength < 0 || pngByteLength > MaxPngByteLength)
                    throw new InvalidDataException("FOIC PNG length out of range.");

                Bitmap? icon = null;
                try {
                    if (pngByteLength > 0) {
                        byte[] pngBytes = await ReadBytesAsync(payload, pngByteLength, cancellationToken).ConfigureAwait(false);
                        payloadBytesRead += pngByteLength;
                        using var pngStream = new MemoryStream(pngBytes, writable: false);
                        icon = new Bitmap(pngStream);
                    }

                    iconCache.Add(name, new IconColorPair(icon, Color.FromArgb(colorArgb)));
                    icon = null;
                } finally {
                    icon?.Dispose();
                }
                progress?.Report((i + 1, payloadEntryCount));
                await Task.Yield();
            }

            return payloadBytesRead != expectedUncompressedSize
                ? throw new InvalidDataException("FOIC uncompressed payload size mismatch.")
                : iconCache;
        }

        private static async Task WriteHeaderAsync(Stream output, uint entryCount, uint uncompressedSize, uint compressedSize, CancellationToken cancellationToken) {
            await output.WriteAsync(MagicBytes, cancellationToken).ConfigureAwait(false);
            byte[] headerTail = new byte[HeaderSize - 4];
            BinaryPrimitives.WriteUInt16LittleEndian(headerTail, FormatVersion);
            BinaryPrimitives.WriteUInt16LittleEndian(headerTail.AsSpan(2), 0);
            BinaryPrimitives.WriteUInt32LittleEndian(headerTail.AsSpan(4), entryCount);
            BinaryPrimitives.WriteUInt32LittleEndian(headerTail.AsSpan(8), uncompressedSize);
            BinaryPrimitives.WriteUInt32LittleEndian(headerTail.AsSpan(12), compressedSize);
            await output.WriteAsync(headerTail, cancellationToken).ConfigureAwait(false);
        }

        private static async Task PatchCompressedSizeAsync(Stream output, uint compressedSize, CancellationToken cancellationToken) {
            output.Seek(CompressedSizeHeaderOffset, SeekOrigin.Begin);
            await WriteUInt32Async(output, compressedSize, cancellationToken).ConfigureAwait(false);
            output.Seek(0, SeekOrigin.End);
        }

        private static void ReadHeader(Stream stream, out uint entryCount, out uint uncompressedSize, out uint compressedSize) {
            Span<byte> header = stackalloc byte[HeaderSize];
            ReadExactly(stream, header);

            if (!header[..4].SequenceEqual(Magic))
                throw new InvalidDataException("Not a FOIC icon cache file.");

            ushort version = BinaryPrimitives.ReadUInt16LittleEndian(header[4..]);
            if (version != FormatVersion)
                throw new InvalidDataException($"Unsupported FOIC version {version}.");

            entryCount = BinaryPrimitives.ReadUInt32LittleEndian(header[8..]);
            uncompressedSize = BinaryPrimitives.ReadUInt32LittleEndian(header[12..]);
            compressedSize = BinaryPrimitives.ReadUInt32LittleEndian(header[16..]);
        }

        private static async Task WriteInt32Async(Stream stream, int value, CancellationToken cancellationToken) {
            byte[] buffer = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        private static async Task WriteUInt16Async(Stream stream, ushort value, CancellationToken cancellationToken) {
            byte[] buffer = new byte[sizeof(ushort)];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        private static async Task WriteUInt32Async(Stream stream, uint value, CancellationToken cancellationToken) {
            byte[] buffer = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<int> ReadInt32Async(Stream stream, CancellationToken cancellationToken) {
            byte[] buffer = new byte[sizeof(int)];
            await ReadExactlyAsync(stream, buffer, cancellationToken).ConfigureAwait(false);
            return BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        private static async Task<int> ReadUInt16Async(Stream stream, CancellationToken cancellationToken) {
            byte[] buffer = new byte[sizeof(ushort)];
            await ReadExactlyAsync(stream, buffer, cancellationToken).ConfigureAwait(false);
            return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        private static async Task<string> ReadUtf8StringAsync(Stream stream, int byteLength, CancellationToken cancellationToken) {
            byte[] buffer = new byte[byteLength];
            await ReadExactlyAsync(stream, buffer, cancellationToken).ConfigureAwait(false);
            return Encoding.UTF8.GetString(buffer);
        }

        private static async Task<byte[]> ReadBytesAsync(Stream stream, int byteLength, CancellationToken cancellationToken) {
            byte[] buffer = new byte[byteLength];
            await ReadExactlyAsync(stream, buffer, cancellationToken).ConfigureAwait(false);
            return buffer;
        }

        private static void ReadExactly(Stream stream, Span<byte> buffer) {
            int totalRead = 0;
            while (totalRead < buffer.Length) {
                int read = stream.Read(buffer[totalRead..]);
                if (read == 0)
                    throw new EndOfStreamException();
                totalRead += read;
            }
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken) {
            int totalRead = 0;
            while (totalRead < buffer.Length) {
                int read = await stream.ReadAsync(buffer.AsMemory(totalRead), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    throw new EndOfStreamException();
                totalRead += read;
            }
        }

        private static byte[] EncodePng(Bitmap? icon) {
            if (icon is null)
                return [];
            using var stream = new MemoryStream();
            icon.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

    }
}
