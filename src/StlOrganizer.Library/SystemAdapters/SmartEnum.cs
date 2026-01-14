namespace StlOrganizer.Library.SystemAdapters;

public abstract class SmartEnum<T>(int id, string name)
    where T : SmartEnum<T>
{
    public int Id { get; } = id;
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        return obj is SmartEnum<T> other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(SmartEnum<T>? left, SmartEnum<T>? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(SmartEnum<T>? left, SmartEnum<T>? right)
    {
        return !(left == right);
    }
    
    public static implicit operator int(SmartEnum<T> operation) => operation.Id;

    public static implicit operator string(SmartEnum<T> operation) => operation.Name;
}