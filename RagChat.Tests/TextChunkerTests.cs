using RagChat.Api.Services;
using Shouldly;
using Xunit;

namespace RagChat.Tests;

public class TextChunkerTests
{
    private readonly TextChunker _chunker = new();

    [Fact]
    public void Chunk_ShortText_ReturnsSingleChunk()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 10));

        var chunks = _chunker.Chunk(text, maxWords: 250, overlapWords: 50);

        chunks.Count.ShouldBe(1);
        chunks[0].Split(' ').Length.ShouldBe(10);
    }

    [Fact]
    public void Chunk_LongText_ProducesOverlappingWindows()
    {
        var words = Enumerable.Range(0, 600).Select(i => $"w{i}");
        var text = string.Join(' ', words);

        var chunks = _chunker.Chunk(text, maxWords: 250, overlapWords: 50);

        chunks.Count.ShouldBe(3);
        chunks[0].Split(' ')[0].ShouldBe("w0");
        chunks[1].Split(' ')[0].ShouldBe("w200");
        chunks[^1].ShouldContain("w599");
    }

    [Fact]
    public void Chunk_EmptyText_ReturnsNoChunks()
    {
        _chunker.Chunk("   ").ShouldBeEmpty();
    }

    [Fact]
    public void Chunk_OverlapNotSmallerThanMax_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => _chunker.Chunk("some text", maxWords: 10, overlapWords: 10));
    }
}
