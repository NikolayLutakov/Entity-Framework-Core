using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace _07_Print_All_Minion_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            string selectNameQuery = "SELECT Name FROM Minions";

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;")) 
            {
                connection.Open();

                SqlCommand takeNames = new SqlCommand(selectNameQuery, connection);

                var result = takeNames.ExecuteReader();

                List<string> names = new List<string>();

                while (result.Read())
                {
                    names.Add((string)result["Name"]);
                }

                //Console.WriteLine(string.Join(", ", names));

                for (int i = 0; i < names.Count / 2; i++)
                {
                    Console.WriteLine(names[i]);
                    Console.WriteLine(names[names.Count - 1 - i]);
                }

                if (names.Count % 2 != 0)
                {
                    Console.WriteLine(names[names.Count / 2]);
                }
            }
        }
    }
}
