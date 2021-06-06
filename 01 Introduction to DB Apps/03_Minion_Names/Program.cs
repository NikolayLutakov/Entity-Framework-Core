using Microsoft.Data.SqlClient;
using System;

namespace _03_Minion_Names
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;";

            SqlConnection connection = new SqlConnection(connectionString);
            int villainId = int.Parse(Console.ReadLine());
            using (connection)
            {
                connection.Open();

                string takeVillainsQueryString = "SELECT Name FROM Villains WHERE Id = @villainId";

                SqlCommand command = new SqlCommand(takeVillainsQueryString, connection);
                command.Parameters.AddWithValue("@villainId", villainId);

                SqlDataReader villainResult = command.ExecuteReader();
                using (villainResult)
                {
                    if (!villainResult.HasRows)
                    {
                        Console.WriteLine($"No villain with ID {villainId} exists in the database.");
                        return;
                    }

                    villainResult.Read();
                    Console.WriteLine($"Villain: {villainResult["Name"]}");
                }

                string takeMinionsQueryString = @"SELECT Name, Age FROM Minions AS M JOIN MinionsVillains AS MV ON MV.MinionId = M.Id
                                                    WHERE MV.VillainId = @villainId";
                SqlCommand minionCmd = new SqlCommand(takeMinionsQueryString, connection);
                minionCmd.Parameters.AddWithValue("@villainId", villainId);

                SqlDataReader minionResult = minionCmd.ExecuteReader();
                using (minionResult)
                {

                    if (!minionResult.HasRows)
                    {
                        Console.WriteLine("(no minions)");
                        return;
                    }

                    int cnt = 0;
                    while (minionResult.Read())
                    {
                        cnt++;
                        Console.WriteLine($"{cnt}. {minionResult["Name"]} {minionResult["Age"]}");
                    }
                }
            }
        }
    }
}
