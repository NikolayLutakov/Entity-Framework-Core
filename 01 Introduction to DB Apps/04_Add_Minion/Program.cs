using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _04_Add_Minion
{
    class Program
    {
        static void Main(string[] args)
        {
            string minionName, minionTown, villainName;
            int minionAge;

            ProcessInput(out minionName, out minionAge, out minionTown, out villainName);
            StartProcessing(minionName, minionTown, villainName, minionAge);

        }

        private static void StartProcessing(string minionName, string minionTown, string villainName, int minionAge)
        {
            Dictionary<string, string> commands = new Dictionary<string, string>();
            FuillCommands(commands);

            int townId = 0;
            int villainId = 0;
            int minionId = 0;

            using (SqlConnection connection = new SqlConnection("Server=192.168.0.116,1433;Database=MinionsDB;User ID=nlyutakov;Password=A123456a;"))
            {
                connection.Open();

                townId = ProcessTown(minionTown, commands, connection);
                villainId = ProcessVillain(villainName, commands, connection);

                minionId = ProcessMinion(minionName, minionAge, villainName, commands, townId, villainId, connection);
                AddMinion(minionName, villainName, commands, villainId, minionId, connection);
            }
        }

        private static void ProcessInput(out string minionName, out int minionAge, out string minionTown, out string villainName)
        {
            string[] firstLineParams = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries);
            string[] secondLineParams = Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            minionName = firstLineParams[1];
            minionAge = int.Parse(firstLineParams[2]);
            minionTown = firstLineParams[3];
            villainName = secondLineParams[1];
        }

        private static void AddMinion(string minionName, string villainName, Dictionary<string, string> commands, int villainId, int minionId, SqlConnection connection)
        {
            SqlCommand insertMinionsVillains = new SqlCommand(commands["insertMinionsVillains"], connection);
            insertMinionsVillains.Parameters.AddWithValue("@villainId", villainId);
            insertMinionsVillains.Parameters.AddWithValue("@minionId", minionId);

            insertMinionsVillains.ExecuteNonQuery();

            Console.WriteLine($"Successfully added {minionName} to be minion of {villainName}.");
        }

        private static int ProcessMinion(string minionName, int minionAge, string villainName, Dictionary<string, string> commands, int townId, int villainId, SqlConnection connection)
        {
            int minionId;
            SqlCommand insertMinion = new SqlCommand(commands["insertMinion"], connection);
            insertMinion.Parameters.AddWithValue("@name", minionName);
            insertMinion.Parameters.AddWithValue("@age", minionAge);
            insertMinion.Parameters.AddWithValue("@townId", townId);

            insertMinion.ExecuteNonQuery();

            SqlCommand takeInsertedMinionId = new SqlCommand(commands["selectMinionId"], connection);
            takeInsertedMinionId.Parameters.AddWithValue("@Name", minionName);

            minionId = (int)takeInsertedMinionId.ExecuteScalar();


            return minionId;
        }

        private static int ProcessVillain(string villainName, Dictionary<string, string> commands, SqlConnection connection)
        {
            int villainId;
            SqlCommand takeVillainId = new SqlCommand(commands["selectVillains"], connection);
            takeVillainId.Parameters.AddWithValue("@Name", villainName);

            int? takenVillainId = (int?)takeVillainId.ExecuteScalar();

            if (takenVillainId == null)
            {
                SqlCommand insertVillain = new SqlCommand(commands["insertVillain"], connection);
                insertVillain.Parameters.AddWithValue("@villainName", villainName);
                insertVillain.ExecuteNonQuery();

                SqlCommand takeInsertedVillainId = new SqlCommand(commands["selectVillains"], connection);
                takeInsertedVillainId.Parameters.AddWithValue("@Name", villainName);
                villainId = (int)takeInsertedVillainId.ExecuteScalar();
                Console.WriteLine($"Villain {villainName} was added to the database.");
            }
            else
            {
                villainId = (int)takenVillainId;
            }

            return villainId;
        }

        private static int ProcessTown(string minionTown, Dictionary<string, string> commands, SqlConnection connection)
        {
            int townId;
            SqlCommand takeTownId = new SqlCommand(commands["selectTownId"], connection);
            takeTownId.Parameters.AddWithValue("@townName", minionTown);

            int? takenTownId = (int?)takeTownId.ExecuteScalar();

            if (takenTownId == null)
            {
                SqlCommand insertTown = new SqlCommand(commands["insertTown"], connection);
                insertTown.Parameters.AddWithValue("@townName", minionTown);
                insertTown.ExecuteNonQuery();

                SqlCommand takeInsertedTownId = new SqlCommand(commands["selectTownId"], connection);
                takeInsertedTownId.Parameters.AddWithValue("@townName", minionTown);

                townId = (int)takeInsertedTownId.ExecuteScalar();

                Console.WriteLine($"Town {minionTown} was added to the database.");
            }
            else
            {
                townId = (int)takenTownId;
            }

            return townId;
        }

        private static Dictionary<string, string> FuillCommands(Dictionary<string, string> commands)
        {
            commands["selectVillains"] = "SELECT Id FROM Villains WHERE Name = @Name";
            commands["selectMinionId"] = "SELECT Id FROM Minions WHERE Name = @Name";
            commands["insertMinionsVillains"] = "INSERT INTO MinionsVillains(MinionId, VillainId) VALUES(@minionId, @villainId)";
            commands["insertVillain"] = "INSERT INTO Villains(Name, EvilnessFactorId)  VALUES(@villainName, 4)";
            commands["insertMinion"] = "INSERT INTO Minions(Name, Age, TownId) VALUES(@name, @age, @townId)";
            commands["insertTown"] = "INSERT INTO Towns(Name) VALUES(@townName)";
            commands["selectTownId"] = "SELECT Id FROM Towns WHERE Name = @townName";
            return commands;
        }
    }
}
