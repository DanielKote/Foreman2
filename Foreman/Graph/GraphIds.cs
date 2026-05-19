using System;

namespace Foreman.Graph {
    public readonly struct NodeId(int value, uint epoch) : IEquatable<NodeId> {
        public int Value { get; } = value;
        public uint Epoch { get; } = epoch;

        public bool IsValid => Epoch != 0;

        public static NodeId Invalid => default;

        public bool Equals(NodeId other) => Value == other.Value && Epoch == other.Epoch;
        public override bool Equals(object? obj) => obj is NodeId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Value, Epoch);
        public static bool operator ==(NodeId left, NodeId right) => left.Equals(right);
        public static bool operator !=(NodeId left, NodeId right) => !left.Equals(right);
        public override string ToString() => IsValid ? $"Node({Value}@{Epoch})" : "Node(invalid)";
    }

    public readonly struct LinkId(int value, uint epoch) : IEquatable<LinkId> {
        public int Value { get; } = value;
        public uint Epoch { get; } = epoch;

        public bool IsValid => Epoch != 0;

        public static LinkId Invalid => default;

        public bool Equals(LinkId other) => Value == other.Value && Epoch == other.Epoch;
        public override bool Equals(object? obj) => obj is LinkId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Value, Epoch);
        public static bool operator ==(LinkId left, LinkId right) => left.Equals(right);
        public static bool operator !=(LinkId left, LinkId right) => !left.Equals(right);
        public override string ToString() => IsValid ? $"Link({Value}@{Epoch})" : "Link(invalid)";
    }
}
