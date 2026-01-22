using FluentAssertions;

namespace NemesisEuchre.GameEngine.Tests;

public class CardShufflerTests
{
    [Fact]
    public void Shuffle_WithArray_ModifiesArray()
    {
        var shuffler = new CardShuffler();
        var original = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        shuffler.Shuffle(array);

        array.Should().NotBeEquivalentTo(original, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Shuffle_WithArray_ContainsSameElements()
    {
        var shuffler = new CardShuffler();
        var original = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var array = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        shuffler.Shuffle(array);

        array.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Shuffle_WithSingleElement_LeavesUnchanged()
    {
        var shuffler = new CardShuffler();
        var array = new[] { 42 };

        shuffler.Shuffle(array);

        array.Should().BeEquivalentTo([42]);
    }

    [Fact]
    public void Shuffle_WithEmptyArray_DoesNotThrow()
    {
        var shuffler = new CardShuffler();
        var array = Array.Empty<int>();

        var act = () => shuffler.Shuffle(array);

        act.Should().NotThrow();
    }
}
