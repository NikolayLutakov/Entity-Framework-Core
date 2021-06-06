using System;
using Microsoft.Data.SqlClient;

namespace _06_Remove_Villain
{
    class Program
    {
        static void Main(string[] args)
        {
            int id = int.Parse(Console.ReadLine());
            string villainName;
            int minionsCount = 0;

            string selectVillainQuery = @"SELECT Name FROM Villains WHERE Id = @villainId";

            string deleteMappingQuery = @"DELETE FROM MinionsVillains
                  WHERE VillainId = @villainId";

            string deleteVillainQuery = @"DELETE FROM Villains
                  WHERE Id = @villainId";

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;"))
            {
                connection.Open();

                SqlCommand takeVillainName = new SqlCommand(selectVillainQuery, connection);
                takeVillainName.Parameters.AddWithValue("@villainId", id);
                villainName = (string)takeVillainName.ExecuteScalar();

                if (villainName == null)
                {
                    Console.WriteLine("No such villain was found.");
                    return;
                }

                SqlTransaction sqlTran = connection.BeginTransaction();
                
                SqlCommand deleteFromMapping = new SqlCommand(deleteMappingQuery, connection);
                deleteFromMapping.Parameters.AddWithValue("@villainId", id);
                deleteFromMapping.Transaction = sqlTran;
                SqlCommand deleteVillain = new SqlCommand(deleteVillainQuery, connection);
                deleteVillain.Parameters.AddWithValue("@villainId", id);
                deleteVillain.Transaction = sqlTran;

                try
                {
                    minionsCount = deleteFromMapping.ExecuteNonQuery();
                    deleteVillain.ExecuteNonQuery();
                    sqlTran.Commit();
                }
                catch (SqlException ex)
                {
                    sqlTran.Rollback();
                    Console.WriteLine(ex.Message);
                    return;
                }
                Console.WriteLine($"{villainName} was deleted.");
                Console.WriteLine($"{minionsCount} minions were released.");

            }


        }
    }
}
