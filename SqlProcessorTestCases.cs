using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SqlProcessing.Tests;

public static class SqlProcessorTestCases
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    public static IEnumerable<object[]> GetTestCases()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var input = JsonSerializer.Deserialize<TestCase[]>(
            assembly.GetManifestResourceStream("SqlProcessorTestCases.json")!,
            JsonSerializerOptions);
        return GenerateTestCases(input!);
    }

    private static List<object[]> GenerateTestCases(IEnumerable<TestCase> input)
    {
        var result = new List<object[]>();

        foreach (var test in input)
        {
            foreach (var dialect in test.Dialects)
            {
                var sqldialect = dialect switch
                {
                    "mssql" => SqlDialect.MsSql,
                    "mysql" => SqlDialect.MySql,
                    "postgres" => SqlDialect.Postgres,
                    "oracle" => SqlDialect.Oracle,
                    _ => throw new NotSupportedException("Unsupported dialect"),
                };

                result.Add([new TestCase()
                {
                    SqlDialect = sqldialect,
                    Name = test.Name,
                    Sql = test.Sql,
                    Sanitized = test.Sanitized,
                }
                ]);
            }
        }

        return result;
    }

    public class TestCase
    {
        public SqlDialect SqlDialect { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Sql { get; set; } = string.Empty;

        public string Sanitized { get; set; } = string.Empty;

        public IEnumerable<string> Dialects { get; set; } = [];

        public override string ToString()
        {
            return $"{this.SqlDialect}: {this.Name}";
        }
    }
}
