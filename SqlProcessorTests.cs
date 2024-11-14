namespace SqlProcessing.Tests;

public class SqlProcessorTests
{
    public static IEnumerable<object[]> TestData => SqlProcessorTestCases.GetTestCases();

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestSqlProcessingMsSql(SqlProcessorTestCases.TestCase test)
    {
        if (test.SqlDialect != SqlDialect.MsSql)
        {
            return;
        }

        var sanitized = SqlProcessor.GetSanitizedSql(test.Sql);
        Assert.Equal(test.Sanitized, sanitized);
    }
}
