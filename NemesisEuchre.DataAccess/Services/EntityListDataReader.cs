using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace NemesisEuchre.DataAccess.Services;

[SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = "DbDataReader does not require IEnumerable<T> for SqlBulkCopy usage")]
public class EntityListDataReader<T>(
    IReadOnlyList<T> entities,
    (string name, Type type, Func<T, object> getValue)[] columns) : DbDataReader
{
    private int _currentIndex = -1;

    public override int FieldCount => columns.Length;

    public override int RecordsAffected => -1;

    public override bool HasRows => entities.Count > 0;

    public override bool IsClosed => false;

    public override int Depth => 0;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool Read()
    {
        _currentIndex++;
        return _currentIndex < entities.Count;
    }

    public override object GetValue(int ordinal)
    {
        return columns[ordinal].getValue(entities[_currentIndex]);
    }

    public override string GetName(int ordinal)
    {
        return columns[ordinal].name;
    }

    public override Type GetFieldType(int ordinal)
    {
        return columns[ordinal].type;
    }

    public override int GetOrdinal(string name)
    {
        for (int i = 0; i < columns.Length; i++)
        {
            if (string.Equals(columns[i].name, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(name), name, $"Column '{name}' not found.");
    }

    public override bool IsDBNull(int ordinal)
    {
        return GetValue(ordinal) == DBNull.Value;
    }

    public override int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, columns.Length);

        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public override bool GetBoolean(int ordinal)
    {
        return (bool)GetValue(ordinal);
    }

    public override byte GetByte(int ordinal)
    {
        return (byte)GetValue(ordinal);
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException();
    }

    public override char GetChar(int ordinal)
    {
        return (char)GetValue(ordinal);
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotSupportedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return (DateTime)GetValue(ordinal);
    }

    public override decimal GetDecimal(int ordinal)
    {
        return (decimal)GetValue(ordinal);
    }

    public override double GetDouble(int ordinal)
    {
        return (double)GetValue(ordinal);
    }

    public override float GetFloat(int ordinal)
    {
        return (float)GetValue(ordinal);
    }

    public override Guid GetGuid(int ordinal)
    {
        return (Guid)GetValue(ordinal);
    }

    public override short GetInt16(int ordinal)
    {
        return (short)GetValue(ordinal);
    }

    public override int GetInt32(int ordinal)
    {
        return (int)GetValue(ordinal);
    }

    public override long GetInt64(int ordinal)
    {
        return (long)GetValue(ordinal);
    }

    public override string GetString(int ordinal)
    {
        return (string)GetValue(ordinal);
    }

    public override string GetDataTypeName(int ordinal)
    {
        return GetFieldType(ordinal).Name;
    }

    public override System.Collections.IEnumerator GetEnumerator()
    {
        throw new NotSupportedException();
    }

    public override bool NextResult()
    {
        return false;
    }
}
