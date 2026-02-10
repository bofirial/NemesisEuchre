using FluentAssertions;

using NemesisEuchre.DataAccess.Services;

namespace NemesisEuchre.DataAccess.Tests.Services;

public class EntityListDataReaderTests
{
    private readonly (string name, Type type, Func<TestEntity, object> getValue)[] _columns =
    [
        ("Id", typeof(int), e => e.Id),
        ("Name", typeof(string), e => e.Name),
        ("Score", typeof(float), e => e.Score),
    ];

    [Fact]
    public void Read_AdvancesThroughAllEntities()
    {
        var entities = new List<TestEntity>
        {
            new(1, "Alice", 1.5f),
            new(2, "Bob", 2.5f),
            new(3, "Carol", 3.5f),
        };

        using var reader = new EntityListDataReader<TestEntity>(entities, _columns);

        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeFalse();
    }

    [Fact]
    public void FieldCount_ReturnsColumnCount()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.FieldCount.Should().Be(3);
    }

    [Fact]
    public void GetName_ReturnsColumnName()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.GetName(0).Should().Be("Id");
        reader.GetName(1).Should().Be("Name");
        reader.GetName(2).Should().Be("Score");
    }

    [Fact]
    public void GetFieldType_ReturnsColumnType()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.GetFieldType(0).Should().Be<int>();
        reader.GetFieldType(1).Should().Be<string>();
        reader.GetFieldType(2).Should().Be<float>();
    }

    [Fact]
    public void GetValue_ReturnsEntityValue()
    {
        var entities = new List<TestEntity> { new(42, "Test", 9.9f) };
        using var reader = new EntityListDataReader<TestEntity>(entities, _columns);

        reader.Read();

        reader.GetValue(0).Should().Be(42);
        reader.GetValue(1).Should().Be("Test");
        reader.GetValue(2).Should().Be(9.9f);
    }

    [Fact]
    public void GetValue_WithNullable_ReturnsDbNull()
    {
        var entities = new List<NullableEntity> { new(1, null) };
        (string name, Type type, Func<NullableEntity, object> getValue)[] columns =
        [
            ("Id", typeof(int), e => e.Id),
            ("Value", typeof(int), e => (object?)e.Value ?? DBNull.Value),
        ];

        using var reader = new EntityListDataReader<NullableEntity>(entities, columns);

        reader.Read();

        reader.GetValue(0).Should().Be(1);
        reader.GetValue(1).Should().Be(DBNull.Value);
        reader.IsDBNull(1).Should().BeTrue();
        reader.IsDBNull(0).Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEmptyList_ReadsNothing()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.HasRows.Should().BeFalse();
        reader.Read().Should().BeFalse();
    }

    [Fact]
    public void GetOrdinal_ReturnsCorrectIndex()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.GetOrdinal("Id").Should().Be(0);
        reader.GetOrdinal("Name").Should().Be(1);
        reader.GetOrdinal("Score").Should().Be(2);
    }

    [Fact]
    public void GetOrdinal_WithUnknownColumn_ThrowsIndexOutOfRange()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        var act = () => reader.GetOrdinal("Unknown");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetValues_PopulatesArray()
    {
        var entities = new List<TestEntity> { new(7, "Hello", 3.14f) };
        using var reader = new EntityListDataReader<TestEntity>(entities, _columns);
        reader.Read();

        var values = new object[3];
        int count = reader.GetValues(values);

        count.Should().Be(3);
        values[0].Should().Be(7);
        values[1].Should().Be("Hello");
        values[2].Should().Be(3.14f);
    }

    [Fact]
    public void NextResult_ReturnsFalse()
    {
        using var reader = new EntityListDataReader<TestEntity>([], _columns);

        reader.NextResult().Should().BeFalse();
    }

    [Fact]
    public void HasRows_WithEntities_ReturnsTrue()
    {
        var entities = new List<TestEntity> { new(1, "A", 0f) };
        using var reader = new EntityListDataReader<TestEntity>(entities, _columns);

        reader.HasRows.Should().BeTrue();
    }

#pragma warning disable SA1201
    private sealed record TestEntity(int Id, string Name, float Score);

    private sealed record NullableEntity(int Id, int? Value);
#pragma warning restore SA1201
}
