using System.Text;

namespace SqlProcessing;

public static class SqlProcessor
{
    public static string GetSanitizedSql(string sql)
    {
        if (sql == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        var length = sql.Length;
        for (var i = 0; i < length; ++i)
        {
            char ch = sql[i];

            // Remove multi-line comment
            if (ch == '/' && i + 1 < length && sql[i + 1] == '*')
            {
                for (i += 2; i < length; ++i)
                {
                    ch = sql[i];
                    if (ch == '*' && i + 1 < length && sql[i + 1] == '/')
                    {
                        i += 1;
                        break;
                    }
                }

                continue;
            }

            // Remove single-line comment
            if (ch == '-' && i + 1 < length && sql[i + 1] == '-')
            {
                for (i += 2; i < length; ++i)
                {
                    ch = sql[i];
                    if (ch == '\r' || ch == '\n')
                    {
                        i -= 1;
                        break;
                    }
                }

                continue;
            }

            // Replace string value with ?
            if (ch == '\'')
            {
                for (i += 1; i < length; ++i)
                {
                    ch = sql[i];
                    if (ch == '\'' && i + 1 < length && sql[i + 1] == '\'')
                    {
                        i += 1;
                        continue;
                    }

                    if (ch == '\'')
                    {
                        break;
                    }
                }

                sb.Append('?');
                continue;
            }

            // Replace hex value with ?
            if (ch == '0' && i + 1 < length && (sql[i + 1] == 'x' || sql[i + 1] == 'X'))
            {
                for (i += 2; i < length; ++i)
                {
                    ch = sql[i];
                    if (char.IsDigit(ch) ||
                        ch == 'A' || ch == 'a' ||
                        ch == 'B' || ch == 'b' ||
                        ch == 'C' || ch == 'c' ||
                        ch == 'D' || ch == 'd' ||
                        ch == 'E' || ch == 'e' ||
                        ch == 'F' || ch == 'f')
                    {
                        continue;
                    }

                    i -= 1;
                    break;
                }

                sb.Append('?');
                continue;
            }

            // Check for leading period of numeric value
            var periodMatched = false;
            if (ch == '.' && i + 1 < length && char.IsDigit(sql[i + 1]))
            {
                periodMatched = true;
                i += 1;
                ch = sql[i];
            }

            // Scan past leading sign of numeric value
            if ((ch == '-' || ch == '+') && i + 1 < length && char.IsDigit(sql[i + 1]))
            {
                i += 1;
                ch = sql[i];
            }

            // Replace numeric value with ?
            if (char.IsDigit(ch))
            {
                var exponentMatched = false;
                for (i += 1; i < length; ++i)
                {
                    ch = sql[i];
                    if (char.IsDigit(ch))
                    {
                        continue;
                    }

                    if (!periodMatched && ch == '.')
                    {
                        periodMatched = true;
                        continue;
                    }

                    if (!exponentMatched && (ch == 'e' || ch == 'E'))
                    {
                        // Scan past sign in exponent
                        if (i + 1 < length && (sql[i + 1] == '-' || sql[i + 1] == '+'))
                        {
                            i += 1;
                        }

                        exponentMatched = true;
                        continue;
                    }

                    i -= 1;
                    break;
                }

                sb.Append('?');
                continue;
            }

            // Span across identifiers
            if (char.IsLetter(ch) || ch == '_')
            {
                for (; i < length; i++)
                {
                    ch = sql[i];
                    if (char.IsLetter(ch) || ch == '_' || char.IsDigit(ch))
                    {
                        sb.Append(ch);
                        continue;
                    }
                    
                    break;
                }

                i -= 1;
                continue;
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }
}
