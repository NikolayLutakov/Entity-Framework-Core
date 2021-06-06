using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace _05_Change_Town_Names_Casing
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = Console.ReadLine();

            string updateQueryString = @"UPDATE Towns
                                         SET Name = UPPER(Name)
                                         WHERE CountryCode = (SELECT c.Id FROM Countries AS c WHERE c.Name =                    @countryName)";

            string selectUpdatedQueryString = @"SELECT t.Name
                                        FROM Towns as t
                                        JOIN Countries AS c ON c.Id = t.CountryCode
                                        WHERE c.Name = @countryName";

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;"))
            {
                connection.Open();
                SqlCommand updateTownsCmd = new SqlCommand(updateQueryString, connection);
                updateTownsCmd.Parameters.AddWithValue("@countryName", input);

                

                int updatedTowns = updateTownsCmd.ExecuteNonQuery();

                if (updatedTowns == 0)
                {
                    Console.WriteLine("No town names were affected.");
                }
                else
                {
                    SqlCommand selectUpdatedTownsCmd = new SqlCommand(selectUpdatedQueryString, connection);
                    selectUpdatedTownsCmd.Parameters.AddWithValue("@countryName", input);

                    List<string> townNames = new List<string>();

                    using (var result = selectUpdatedTownsCmd.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            townNames.Add((string)result["Name"]);
                        }
                    }
                    Console.WriteLine($"{updatedTowns} town names were affected.");
                    Console.WriteLine($"[{string.Join(", ", townNames)}]");

                }
            }
        }
    }
}
