using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;
    private IDbConnection connection;
    private IDbCommand command;
    private IDataReader reader;

    void Awake()
    {
        dbPath = $"URI=file:{Application.persistentDataPath}/PlayerData.db";
        CreateDatabase();
        CreateTable();
    }

    private void CreateDatabase()
    {
        connection = new SqliteConnection(dbPath);
        connection.Open();
    }

    private void CreateTable()
    {
        string createTableQuery = "CREATE TABLE IF NOT EXISTS PlayerData (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Score INTEGER)";
        ExecuteNonQuery(createTableQuery);
    }

    public void InsertPlayerData(string name, int score)
    {
        string insertQuery = $"INSERT INTO PlayerData (Name, Score) VALUES ('{name}', {score})";
        ExecuteNonQuery(insertQuery);
    }

    public List<UiManager.PlayerData> GetTopPlayers(int limit = 10)
    {
        string selectQuery = $"SELECT * FROM PlayerData ORDER BY Score DESC LIMIT {limit}";
        var dataTable = ExecuteQuery(selectQuery);

        var players = new List<UiManager.PlayerData>();
        foreach (DataRow row in dataTable.Rows)
        {
            players.Add(new UiManager.PlayerData
            {
                Id = Convert.ToInt32(row["Id"]),
                Name = row["Name"].ToString(),
                Score = Convert.ToInt32(row["Score"])
            });
        }

        return players;
    }

    private void ExecuteNonQuery(string query)
    {
        using (connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }
    }

    private DataTable ExecuteQuery(string query)
    {
        DataTable dataTable = new DataTable();
        using (connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (command = connection.CreateCommand())
            {
                command.CommandText = query;
                using (reader = command.ExecuteReader())
                {
                    dataTable.Load(reader);
                }
            }
        }
        return dataTable;
    }
}
