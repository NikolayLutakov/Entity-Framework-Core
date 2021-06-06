using System;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace _08_Increase_Minion_Age
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] input = Console.ReadLine().Split(" ").Select(int.Parse).ToArray();

            string updateQuery = @"UPDATE Minions
                SET Name = UPPER(LEFT(Name, 1)) + SUBSTRING(Name, 2, LEN(Name)), Age += 1
                WHERE Id = @Id";

            string selectQuery = @"SELECT Name, Age FROM Minions";

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;"))
            {
                connection.Open();

                SqlCommand updateMinions = new SqlCommand(updateQuery, connection);               
                foreach (var item in input)
                {
                    updateMinions.Parameters.Clear();
                    updateMinions.Parameters.AddWithValue("@Id", item);
                    Console.WriteLine(updateMinions.ExecuteNonQuery());
               
                    
                }

                SqlCommand takeMinions = new SqlCommand(selectQuery, connection);
                var result = takeMinions.ExecuteReader();

                while (result.Read())
                {
                    Console.WriteLine($"{result["Name"]} {result["Age"]}");
                }


            }
        }
    }
}
