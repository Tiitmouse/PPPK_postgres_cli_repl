using System;
using Npgsql;

class Program
{
    static void Main(string[] args)
    {
        const string CONNECTION_STRING = @"
        Host=unfailingly-effective-diver.data-1.use1.tembo.io;
        Port=5432;
        Username=postgres;
        Password=awr32pdujLBQdo6f;
        Database=postgres;";

        using (var connection = new NpgsqlConnection(CONNECTION_STRING))
        {
            List<string> queryHistory = new List<string>();
            int historyIndex = 0;
            
            try
            {
                connection.Open();
                Console.WriteLine($"conn established - {connection.Host}");
                
                while (true)
                {
                    Console.Write("Enter SQL query (or 'exit' to quit): ");
                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                    string query = string.Empty;

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        if (historyIndex > 0)
                        {
                            historyIndex--;
                            query = queryHistory[historyIndex];
                            Console.Write(query);
                        }
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        if (historyIndex < queryHistory.Count - 1)
                        {
                            historyIndex++;
                            query = queryHistory[historyIndex];
                            Console.Write(query);
                        }
                    }
                    else
                    {
                        query = keyInfo.KeyChar + Console.ReadLine();
                        queryHistory.Add(query);
                        historyIndex = queryHistory.Count;
                    }

                    if (query.ToLower() == "exit")
                    {
                        break;
                    }

                    try
                    {
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        Console.Write($"{reader.GetName(i)}: {reader[i]} ");
                                    }
                                    Console.WriteLine();
                                }
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Query executed successfully.");
                                Console.ResetColor();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"connection failed {ex}");
            }
        }
    }
}



