using System;

namespace Jeu_D_echec.Models
{
    public struct Position : IEquatable<Position>
    {
        public int Row { get; }
        public int Column { get; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool IsValid => Row >= 0 && Row < 8 && Column >= 0 && Column < 8;

        public static Position operator +(Position a, Position b) => new(a.Row + b.Row, a.Column + b.Column);
        public static Position operator -(Position a, Position b) => new(a.Row - b.Row, a.Column - b.Column);
        public static Position operator *(Position a, int multiplier) => new(a.Row * multiplier, a.Column * multiplier);

        public bool Equals(Position other) => Row == other.Row && Column == other.Column;
        public override bool Equals(object? obj) => obj is Position other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Row, Column);
        public override string ToString() => $"{(char)('a' + Column)}{8 - Row}";

        public static bool operator ==(Position left, Position right) => left.Equals(right);
        public static bool operator !=(Position left, Position right) => !left.Equals(right);
    }
}
