using Microsoft.Data.SqlClient;
using System;

namespace _02_Villain_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;";
            SqlConnection connection = new SqlConnection(connectionString);

            using (connection)
            {
                connection.Open();

                string queryString = @"SELECT V.Id, V.Name, COUNT(MV.MinionId) AS Count FROM Villains AS V
                                        JOIN MinionsVillains AS MV ON MV.VillainId = V.Id
                                        GROUP BY V.Id, V.Name
                                        HAVING COUNT(MV.MinionId) > 3
                                        ORDER BY COUNT(MV.MinionId) DESC";
                SqlCommand command = new SqlCommand(queryString, connection);
                SqlDataReader result = command.ExecuteReader();

                while (result.Read())
                {
                    Console.WriteLine($"{result["Name"]} - {result["Count"]}");
                }
            }
        }
    }
}
