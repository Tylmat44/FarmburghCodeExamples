using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;

//Static class that handles player data.
//Also handles loading and saving of the player data.
public static class AccountManager
{
    private static DateTime lastLogin;
    private static int xp;
    private static int coins;
    private static double level;
    private static Dictionary<string, bool> marketplaceOwned;
    private static Dictionary<string, int> toolUpgrades;

    public static int Xp { get => xp; set => xp = value; }
    public static int Coins { get => coins; set => coins = value; }
    public static double Level { get => level; set => level = value; }
    public static Dictionary<string, bool> MarketplaceOwned { get => marketplaceOwned; set => marketplaceOwned = value; }
    public static Dictionary<string, int> ToolUpgrades { get => toolUpgrades; set => toolUpgrades = value; }
    public static DateTime LastLogin { get => lastLogin; set => lastLogin = value; }

    public static void loadAccountInfo()
    {
        DataManager.DatabaseConnection.Open();
        IDbCommand command = DataManager.DatabaseConnection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS ACCOUNT_INFO (" +
                    "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    "info TEXT)";
        command.ExecuteNonQuery();
        command = DataManager.DatabaseConnection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO ACCOUNT_INFO(info) VALUES(@accountString)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@accountString";
        parameter.Value = EncryptedXmlSerializer.EncryptData(DateTime.UtcNow.Ticks.ToString() + ",0,200,,1,1,1,20");
        command.Parameters.Add(parameter);                       

        command.ExecuteNonQuery();
        command = DataManager.DatabaseConnection.CreateCommand();
        command.CommandText = "SELECT * FROM ACCOUNT_INFO;";
        IDataReader rdr = command.ExecuteReader();
        rdr.Read();
        string[] results = EncryptedXmlSerializer.DecryptData(rdr.GetString(1)).Split(',');
        LastLogin = DateTime.FromBinary(long.Parse(results[0]));
        Xp = int.Parse(results[1]);
        Coins = int.Parse(results[2]);
        marketplaceOwnedFromString(results[3]);
        toolUpgrades = new Dictionary<string, int>();
        toolUpgrades.Add("PLOW", int.Parse(results[4]));
        toolUpgrades.Add("HARVEST", int.Parse(results[5]));
        toolUpgrades.Add("PLANT", int.Parse(results[6]));
        TileManager.mapSize = int.Parse(results[7]);
        DataManager.DatabaseConnection.Close();

        calculateLevel();
    }
     
    public static void saveAccountInfo()
    {
        DataManager.DatabaseConnection.Open();
        IDbCommand command = DataManager.DatabaseConnection.CreateCommand();
        command.CommandText = "DROP TABLE ACCOUNT_INFO";
        command.ExecuteNonQuery();
        command = DataManager.DatabaseConnection.CreateCommand();
        
        //If this is the first time loading the game, creates the new table for account info. If not, does nothing. 
        command.CommandText = "CREATE TABLE IF NOT EXISTS ACCOUNT_INFO (" +
                    "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    "info TEXT)";
        command.ExecuteNonQuery();
        command.CommandText = "INSERT OR IGNORE INTO ACCOUNT_INFO(info) VALUES(@accountString)";
        var parameter = command.CreateParameter();

        //Concatenates all the info into a string then encrypts it so that players cannot easliy change their data.
        string encrypt = DateTime.UtcNow.Ticks.ToString() + "," + Xp.ToString() + "," + Coins.ToString() + "," + marketplaceOwnedToString() + "," + toolUpgrades["PLOW"] + "," + toolUpgrades["HARVEST"] + "," + toolUpgrades["PLANT"] + "," + TileManager.mapSize;
        parameter.ParameterName = "@accountString";
        parameter.Value = EncryptedXmlSerializer.EncryptData(encrypt);

        command.Parameters.Add(parameter);

        command.ExecuteNonQuery();
        DataManager.DatabaseConnection.Close();
    }

    //Calculates the players level using a mathematical funtion.
    public static void calculateLevel()
    {
        Level = Mathf.Max(1, Mathf.Floor((5 + Mathf.Sqrt(Xp+25)) / 10));
    }

    //Combines all the owned marketplaces so I don't have to write down every one manually.
    public static string marketplaceOwnedToString()
    {
        StringBuilder builder = new StringBuilder();

        foreach(string marketplace in MarketplaceOwned.Keys)
        {
            builder.Append(marketplace + "?" + (MarketplaceOwned[marketplace] ? "1" : "0") + "!");
        }

        return builder.ToString();
    }

    public static void marketplaceOwnedFromString(string data)
    {
        MarketplaceOwned = new Dictionary<string, bool>();


        foreach(string marketplace in DataManager.marketplaceDB.Keys)
        {
            MarketplaceOwned.Add(marketplace, false);
        }

        if (data != "")
        {
            string[] marketplaces = data.Split('!');

            foreach (string line in marketplaces)
            {
                if (line != "")
                {
                    string[] sp = line.Split('?');
                    MarketplaceOwned[sp[0]] = (int.Parse(sp[1]) == 1 ? true : false);
                }
            }
        }
    
    }

    //Gains xp and level up if enough xp is met.
    public static void gainXP(int xp)
    {
        double oldLevel = Level;
        Xp += xp;
        calculateLevel();
        if (Level > oldLevel)
        {
            LevelManager.gainLevelPoints(Level);
        }
    }

}
