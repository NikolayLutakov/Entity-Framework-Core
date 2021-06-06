using Microsoft.Data.SqlClient;
using System;

namespace _09_Increase_Age_Stored_Procedure
{
    class Program
    {
        static void Main(string[] args)
        {
            int id = int.Parse(Console.ReadLine());
            string procQuery = "EXEC usp_GetOlder @id";
            string selectQuery = "SELECT Name, Age FROM Minions WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;"))
            {
                connection.Open();

                SqlCommand proc = new SqlCommand(procQuery, connection);
                proc.Parameters.AddWithValue("@Id", id);
                proc.ExecuteNonQuery();

                SqlCommand select = new SqlCommand(selectQuery, connection);
                select.Parameters.AddWithValue("@Id", id);
                var result = select.ExecuteReader();

                while (result.Read())
                {
                    Console.WriteLine($"{result["Name"]} – {result["Age"]} years old");
                }
            }
        }
    }
}
