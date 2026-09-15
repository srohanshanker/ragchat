using RagChat.Api.Services;
using Shouldly;
using Xunit;

namespace RagChat.Tests;

public class VectorMathTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        float[] a = [1, 2, 3];
        float[] b = [1, 2, 3];

        VectorMath.CosineSimilarity(a, b).ShouldBe(1.0, 0.0001);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        float[] a = [1, 0];
        float[] b = [0, 1];

        VectorMath.CosineSimilarity(a, b).ShouldBe(0.0, 0.0001);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        float[] a = [1, 0];
        float[] b = [-1, 0];

        VectorMath.CosineSimilarity(a, b).ShouldBe(-1.0, 0.0001);
    }

    [Fact]
    public void CosineSimilarity_MismatchedLength_Throws()
    {
        float[] a = [1, 2];
        float[] b = [1, 2, 3];

        Should.Throw<ArgumentException>(() => VectorMath.CosineSimilarity(a, b));
    }

    [Fact]
    public void ToBytes_FromBytes_RoundTrips()
    {
        float[] original = [0.1f, -2.5f, 3.14159f];

        var roundTripped = VectorMath.FromBytes(VectorMath.ToBytes(original));

        roundTripped.ShouldBe(original);
    }
}
