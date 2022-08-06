using Godot;
using System;

public class IntPair
{
    public int x { get; set; } = 0;
    public int y { get; set; } = 0;

    public IntPair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public IntPair(Vector2 vector)
    {
        this.x = (int)vector.x;
        this.y = (int)vector.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public Vector2 ToGlobalPosition()
    {
        return new Vector2(x, y) * GameManager.TILE_SIZE;
    }

    public static IntPair operator *(IntPair coordinate, int number)
    {
        return new IntPair(coordinate.x * number, coordinate.y * number);
    }

    public static IntPair operator +(IntPair coordinate, int number)
    {
        return new IntPair(coordinate.x + number, coordinate.y + number);
    }

    public static IntPair operator +(IntPair coordinate, IntPair other)
    {
        return new IntPair(coordinate.x + other.x, coordinate.y + other.y);
    }

    public static IntPair operator -(IntPair coordinate, int number)
    {
        return new IntPair(coordinate.x - number, coordinate.y - number);
    }

    public static IntPair operator -(IntPair coordinate, IntPair other)
    {
        return new IntPair(coordinate.x - other.x, coordinate.y - other.y);
    }

    /// <summary>
    /// Returns an int pair where lowest value is in X and highest in Y
    /// </summary>
    /// <param name="val1">A value</param>
    /// <param name="val2">A value</param>
    /// <returns></returns>
    public static IntPair CreateOrderedPair(int val1, int val2)
    {
        if (val1 < val2)
        {
            return new IntPair(val1, val2);
        }
        return new IntPair(val2, val1);
    }

    public override string ToString()
    {
        return $"IntPair(X:{x}, Y:{y})";
    }

    public string Serialize()
    {
        return $"{x}${y}";
    }

    public static IntPair Parse(string value)
    {
        // Who needs error handling?
        string[] vals = value.Split("$");
        return new IntPair(int.Parse(vals[0]), int.Parse(vals[1]));
    }

    public override int GetHashCode()
    {
        return Tuple.Create(x, y).GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        else if (!(obj is IntPair))
        {
            return false;
        }
        IntPair pair = obj as IntPair;

        return pair.x == x && pair.y == y;
    }
}