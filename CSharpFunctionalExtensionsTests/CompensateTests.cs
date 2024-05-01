using CSharpFunctionalExtensions;
using FluentAssertions;
using Xunit.Abstractions;

namespace CSharpFunctionalExtensionsTests;

public class CompensateTests(ITestOutputHelper testOutputHelper)
{
    private ITestOutputHelper TestOutputHelper { get; } = testOutputHelper;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Compensate_HandlesResult_AsExpected(bool transactionSuccess)
    {
        string? loggedError = null;
        
        var transactionId = await StartTransaction(transactionSuccess)
            .TapError(message => loggedError = message)
            .Compensate(_ => Guid.Empty)
            .Finally(x => x.Value.ToString());

        TestOutputHelper.WriteLine("Logged error: {0}", loggedError ?? "(null)");
        TestOutputHelper.WriteLine("Transaction Guid: {0}", transactionId);
        
        transactionId.Should().NotBeEmpty();
        if (transactionSuccess)
        {
            loggedError.Should().BeNull();
            transactionId.Should().NotBe(Guid.Empty.ToString());
        }
        else
        {
            loggedError.Should().NotBeNull();
            transactionId.Should().Be(Guid.Empty.ToString());
        }
    }

    private static async Task<Result<Guid>> StartTransaction(bool isSuccess = true)
    {
        await Task.Delay(1);
        return isSuccess ? Result.Success(Guid.NewGuid()) : Result.Failure<Guid>("Failed to start a transaction.");
    }
}