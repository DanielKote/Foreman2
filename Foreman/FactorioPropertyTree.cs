using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Foreman
{
    public enum PropTreeType : byte
    {
        None = 0,
        Bool = 1,
        Number = 2,
        String = 3,
        List = 4,
        Dictionary = 5
    }

    public class FactorioPropertyTree
    {
        public PropTreeType PropertyTreeType { get; }
        public object Content;

        private FactorioPropertyTree(PropTreeType type, object content)
        {
            PropertyTreeType = type;
            Content = content;
        }

        public static FactorioPropertyTree Read(BinaryReader reader)
        {
            var type = (PropTreeType)reader.ReadByte();
            var anyTypeFlag = reader.ReadBoolean();

            switch (type)
            {
                case PropTreeType.List:
                case PropTreeType.Dictionary:
                    var dict = ReadDictionary(reader);
                    return new FactorioPropertyTree(type, dict);
                case PropTreeType.None:
                    return new FactorioPropertyTree(type, null);
                case PropTreeType.Bool:
                    var boolVal = reader.ReadBoolean();
                    return new FactorioPropertyTree(type, boolVal);
                case PropTreeType.Number:
                    var numVal = reader.ReadDouble();
                    return new FactorioPropertyTree(type, numVal);
                case PropTreeType.String:
                    var strVal = ReadString(reader);
                    return new FactorioPropertyTree(type, strVal);
                default:
                    throw new Exception($"Unhandled PropTreeType {type}");

            }
        }

        public static Dictionary<string, FactorioPropertyTree> ReadDictionary(BinaryReader reader)
        {
            var numElements = reader.ReadUInt32();
            var dict = new Dictionary<string, FactorioPropertyTree>();

            for (int i = 0; i < numElements; i++)
            {
                var str = ReadString(reader);
                var propTree = Read(reader);
                dict.Add(str, propTree);
            }

            return dict;
        }

        public static string ReadString(BinaryReader reader)
        {
            var empty = reader.ReadBoolean();
            string str = null;
            if (!empty)
            {
                var first = reader.ReadByte();
                uint size = first;

                if (first >= 255)
                {
                    reader.BaseStream.Position -= 1;
                    size = reader.ReadUInt32();
                }

                var buf = new byte[size];
                var numRead = reader.Read(buf, 0, (int)size);

                str = Encoding.UTF8.GetString(buf);
            }

            return str;
        }

        public static Type GetTypeFor(PropTreeType type)
        {
            switch (type)
            {
                case PropTreeType.Bool:
                    return typeof(bool);
                case PropTreeType.Dictionary:
                case PropTreeType.List:
                    return typeof(Dictionary<string, FactorioPropertyTree>);
                case PropTreeType.None:
                    return null;
                case PropTreeType.Number:
                    return typeof(double);
                case PropTreeType.String:
                    return typeof(string);
            }

            return null;
        }
    }
}
