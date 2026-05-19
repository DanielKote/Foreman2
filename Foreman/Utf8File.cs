using System.IO;
using System.Text;

namespace Foreman {
    /// <summary>
    /// UTF-8 text file I/O. Factorio and Foreman JSON use UTF-8; avoid <see cref="Encoding.Default"/> (system ANSI / ACP).
    /// </summary>
    internal static class Utf8File {
        private static readonly Encoding ReadEncoding = Encoding.UTF8;
        private static readonly Encoding WriteEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public static string ReadAllText(string path) => File.ReadAllText(path, ReadEncoding);

        public static string[] ReadAllLines(string path) => File.ReadAllLines(path, ReadEncoding);

        public static void WriteAllText(string path, string contents) => File.WriteAllText(path, contents, WriteEncoding);

        public static void AppendAllText(string path, string contents) => File.AppendAllText(path, contents, WriteEncoding);
    }
}
