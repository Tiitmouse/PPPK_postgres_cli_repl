using Npgsql;
using ConsoleTables;

class Program
{
    static void Main(string[] args)
    {
        using (var connection = new NpgsqlConnection(GetAndParseConnectionString()))
        {
            List<string> queryHistory = new List<string>();
            try
            {
                connection.Open();
                SuccesfulConnection(connection);
                while (true)
                {
                    Console.Write("Enter SQL query (-q to quit or -h to open history): ");
                    string query = Console.ReadLine();

                    if (query.ToLower() == "-q")
                    {
                        break;
                    }
                    if (query.ToLower() == "-h")
                    {
                        OpenHistory(queryHistory,connection);
                        continue;
                    }
                    Execute(query,connection,queryHistory);
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"connection failed {ex}");
            }
        }
    }

    private static string GetAndParseConnectionString()
    {
        Console.Write("Enter database connection string: ");
        string connectionString = Console.ReadLine();
        Uri uri = new Uri(connectionString);

        string host = uri.Host;
        int port = uri.Port;
        string database = uri.AbsolutePath.Trim('/');
        string userInfo = uri.UserInfo;
        string[] userPass = userInfo.Split(':');
        string username = userPass[0];
        string password = userPass[1];

        string CONNECTION_STRING = $@"
            Host={host};
            Port={port};
            Username={username};
            Password={password};
            Database={database};";
        return CONNECTION_STRING;
    }
    private static void Execute(string query, NpgsqlConnection connection, List<string> queryHistory)
    {
        try
        {
            using (var command = new NpgsqlCommand(query, connection))
            {
                if (query.Trim().ToUpper().StartsWith("SELECT"))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        TableFormater(reader);
                        SuccesfulExecution();
                        queryHistory.Add($"\ud83d\udfe2{query} ");
                    }
                }
                else
                {
                    SuccesfulExecution();
                    AffectedRows(command);
                    queryHistory.Add($"\ud83d\udfe2{query} ");
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.ResetColor();
            queryHistory.Add($"\ud83d\udd34{query}");
        }
    }
    private static void AffectedRows(NpgsqlCommand command)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        int affectedRows = command.ExecuteNonQuery();
        Console.WriteLine($"{affectedRows} row(s) affected.");
        Console.ResetColor();
    }
    private static void SuccesfulExecution()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Query executed successfully.");
        Console.ResetColor();
    }
    private static void SuccesfulConnection(NpgsqlConnection connection)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"conn established - {connection.Database}");
        Console.ResetColor();
    }
    private static void TableFormater(NpgsqlDataReader reader)
    {
        if (!reader.HasRows)
        {
           return; 
        }
        
        var table = new ConsoleTable();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            table.AddColumn(new[] { reader.GetName(i).ToUpper() });
        }
                                
        while (reader.Read())
        {
            var row = new List<object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader[i]);
            }

            table.AddRow(row.ToArray());
        }

        table.Write(Format.Minimal);
    }
    private static void OpenHistory(List<string> queryHistory,NpgsqlConnection connection)
    {
        if (queryHistory.Count == 0)
        {
            Console.WriteLine("No queries in history.");
            return;
        }

        int currentIndex = 0;
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Press enter to execute selected: ");
            for (int i = 0; i < queryHistory.Count; i++)
            {
                if (i == currentIndex)
                {
                    Console.WriteLine($"\u219d{queryHistory[i]}");
                }
                else
                {
                    Console.WriteLine(queryHistory[i]);
                }
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey();
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                }
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (currentIndex < queryHistory.Count - 1)
                {
                    currentIndex++;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                string oldQuery = TrimQuery(queryHistory[currentIndex].ToString());
                Console.Write($"Enter SQL query (-q to quit or -h to open history): {oldQuery}");
                Execute(oldQuery,connection,queryHistory);
                return;
            }
        }
    }
    private static string TrimQuery(string query)
    {
        string trimmedQuery = 
            query.Replace("\ud83d\udd34", "")
                .Replace("\ud83d\udfe2", "")
                .Trim();
        return trimmedQuery;
    }
}



