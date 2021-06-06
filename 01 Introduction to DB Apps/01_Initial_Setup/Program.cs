using Microsoft.Data.SqlClient;
using System;

namespace _01_Initial_Setup
{
    class Program
    {
        static void Main()
        {
            string connectionString = "Server=192.168.0.116,1433;Database=master;User ID=nlyutakov;Password=A123456a;";
            SqlConnection connection = new SqlConnection(connectionString);
            
            using (connection)
            {
                connection.Open();
                string ifExistsQuery = @"select DB_ID('MinionsDB')";
                SqlCommand ifExistsCommand= new SqlCommand(ifExistsQuery, connection);

                int? exists = ifExistsCommand.ExecuteNonQuery();

                string createDatabaseString = @"CREATE DATABASE MinionsDB";
                SqlCommand createDatabaseCommand = new SqlCommand(createDatabaseString, connection);

                if (exists == -1)
                {
                    createDatabaseCommand.ExecuteNonQuery();
                }
                
                
                string useDbString = @"USE MinionsDB";
                SqlCommand useDbcommand = new SqlCommand(useDbString, connection);
                useDbcommand.ExecuteNonQuery();

                string createTablesString = @"
                                               CREATE TABLE Countries
                                               (
                                               Id INT PRIMARY KEY IDENTITY,
                                               Name VARCHAR(50)
                                               )
                                               
                                               CREATE TABLE Towns
                                               (
                                               Id INT PRIMARY KEY IDENTITY,
                                               Name VARCHAR(50),
                                               CountryCode INT REFERENCES Countries(Id)
                                               )
                                               
                                               CREATE TABLE Minions
                                               (
                                               Id INT PRIMARY KEY IDENTITY,
                                               Name VARCHAR(50),
                                               Age INT,
                                               TownId INT REFERENCES Towns(Id)
                                               )
                                               
                                               CREATE TABLE EvilnessFactors
                                               (
                                               Id INT PRIMARY KEY IDENTITY,
                                               Name VARCHAR(50)
                                               )
                                               
                                               CREATE TABLE Villains
                                               (
                                               Id INT PRIMARY KEY IDENTITY,
                                               Name VARCHAR(50),
                                               EvilnessFactorId INT REFERENCES EvilnessFactors(Id)
                                               )
                                               
                                               CREATE TABLE MinionsVillains
                                               (
                                               MinionId INT NOT NULL REFERENCES Minions(Id),
                                               VillainId INT NOT NULL REFERENCES Villains(Id)
                                               PRIMARY KEY (MinionId, VillainId)
                                               )
                                               
                 ";
                SqlCommand createTablesCommand = new SqlCommand(createTablesString, connection);
                createTablesCommand.ExecuteNonQuery();
                

                string insertDataString = @"
                                            INSERT INTO Countries ([Name]) 
                                            VALUES 
                                                    ('Bulgaria'),
                                                    ('England'),
                                                    ('Cyprus'),
                                                    ('Germany'),
                                                    ('Norway')

                                            INSERT INTO Towns ([Name], CountryCode) 
                                            VALUES 
                                                    ('Plovdiv', 1),
                                                    ('Varna', 1),
                                                    ('Burgas', 1),
                                                    ('Sofia', 1),        
                                                    ('London', 2),                   
                                                    ('Southampton', 2),
                                                    ('Bath', 2), 
                                                    ('Liverpool', 2),
                                                    ('Berlin', 3),      
                                                    ('Fra nkfurt', 3),
                                                    ('Oslo', 4)
                                                                                        
                                            INSERT INTO Minions (Name,Age, TownId) 
                                            VALUES
                                                    ('Bob', 42, 3),
                                                    ('Kevin', 1, 1),
                                                    ('Bob ', 32, 6),
                                                    ('Simon', 45, 3),
                                                    ('Cathleen', 11, 2),
                                                    ('Carry', 50, 10),
                                                    ('Becky', 125, 5),
                                                    ('Mars', 21, 1),
                                                    ('Misho', 5, 10),
                                                    ('Zoe', 125, 5),
                                                    ('Json', 21, 1)
                                                                                        
                                            INSERT INTO EvilnessFactors (Name) 
                                            VALUES
                                                    ('Super good'),
                                                    ('Good'),
                                                    ('Bad'), 
                                                    ('Evil'),
                                                    ('Super evil')
                                                                                                                                            
                                            INSERT INTO Villains (Name, EvilnessFactorId) 
                                            VALUES 
                                                    ('Gru',2),
                                                    ('Victor',1),
                                                    ('Jilly',3),
                                                    ('Miro',4),
                                                    ('Rosen',5),
                                                    ('Dimityr',1),
                                                    ('Dobromir',2)
                                                                                        
                                            INSERT INTO MinionsVillains (MinionId, VillainId) 
                                            VALUES 
                                                    (4,2),
                                                    (1,1),
                                                    (5,7),
                                                    (3,5),
                                                    (2,6),
                                                    (11,5),
                                                    (8,4),
                                                    (9,7),
                                                    (7,1),
                                                    (1,3),
                                                    (7,3),
                                                    (5,3),
                                                    (4,3),
                                                    (1,2),
                                                    (2,1),
                                                    (2,7)

                ";
                SqlCommand insertData = new SqlCommand(insertDataString, connection);
                
                insertData.ExecuteNonQuery();
               

            }
        }
    }
}
