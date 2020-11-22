using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

class SQLiteManager
{
    private readonly int SQLITE_VERSION = 2;
    private static SQLiteManager ins = null;
    private SQLiteConnection _connection = null;

    private SQLiteManager()
    {
        CreateTables();
    }

    public static SQLiteManager GetInstance()
    {
        if (ins == null) ins = new SQLiteManager();
        return ins;
    }

    public void CreateTables()
    {
        _connection = new SQLiteConnection(ApiModel.CQApi.AppDirectory + "SQLite.db");
        CheckVersion();
        _connection.CreateTable<Log>();
        _connection.CreateTable<Damage>();
        _connection.CreateTable<SaveLoad>();
    }

    public void AddVersion()
    {
        _connection.Insert(new Setting()
        {
            key = "Version",
            value = SQLITE_VERSION.ToString()
        }, "");
    }

    public void CheckVersion()
    {
        if (_connection.Query<TableName>("SELECT * FROM SQLITE_MASTER WHERE type= 'table' AND name = 'Setting'").Count == 0)
        {
            _connection.CreateTable<Setting>(); //偷个懒
        }

        List<Setting> temp = _connection.Query<Setting>("SELECT * FROM Setting WHERE key = 'Version'");
        if (temp.Count == 0)
        {
            _connection.Close();
            if (!Directory.Exists(ApiModel.CQApi.AppDirectory + "Backup/"))
                Directory.CreateDirectory(ApiModel.CQApi.AppDirectory + "Backup/");
            File.Move(ApiModel.CQApi.AppDirectory + "SQLite.db", ApiModel.CQApi.AppDirectory + "Backup/SQLite(" + GetTimeStamp().ToString() + ").db");
            _connection = new SQLiteConnection(ApiModel.CQApi.AppDirectory + "SQLite.db");
            _connection.CreateTable<Setting>();
            AddVersion();
            ApiModel.CQLog.Info("数据库版本升级", "数据库结构更新，将在原数据库备份后重新建立数据库……");
        }

        int versionNow = int.Parse(temp[0].value);
        if (versionNow < SQLITE_VERSION)
        {
            switch (versionNow)
            {
                case 1:
                    _connection.Execute("ALTER TABLE Damage ADD COLUMN troop_operator INTEGER DEFAULT -1");
                    break;
            }
            _connection.Execute("UPDATE Setting SET value = " + SQLITE_VERSION + " WHERE key = 'Version'");
        }
    }

    public void AddLog(long _group, string _text)
    {
        _connection.Insert(new Log()
        {
            time = GetTimeStamp(),
            group_number = _group,
            text = _text
        }, "");
    }

    public List<string> GetLogs(long group)
    {
        List<Log> temp = _connection.Query<Log>("SELECT * FROM Log WHERE group_number = ? ORDER BY id DESC LIMIT 7", group);
        List<string> output = new List<string>();
        for (int i = 0; i < temp.Count; ++i)
        {
            output.Add("[" + ConvertIntDateTime(temp[i].time) + "] " + temp[i].text);
        }
        return output;
    }

    public bool CreateDamage(long group, long qq, int troop, long damage, int frequency, int boss_num, long troop_operator = -1)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ? AND group_number = ?", qq, GetDay(), troop, group);
        if (temp.Count == 0)
        {
            _connection.Insert(new Damage()
            {
                group_number = group,
                user = qq,
                troop = troop,
                damage = damage,
                day = GetDay(),
                frequency = frequency,
                boss_num = boss_num,
                troop_operator = troop_operator
            }, "");
            return true;
        }
        return false;
    }

    public List<Damage> GetTodayDamages(long group, long qq)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE day = ? AND user = ? AND group_number = ?", GetDay(), qq, group);
        return temp;
    }

    public List<Damage> GetTodayDamages(long group)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT id, user, day, COUNT(troop) as troop, SUM(damage) as damage FROM Damage WHERE day = ? AND group_number = ? GROUP BY user", GetDay(), group);
        return temp;
    }

    public Dictionary<long, long> GetRecentDaysDamages(long group, long qq, int day_size)
    {
        List<DayDamage> temp = _connection.Query<DayDamage>("SELECT day, SUM(damage) as total FROM Damage WHERE day >= ? AND user = ? AND group_number = ? GROUP BY day", GetDay() - day_size, qq, group);
        Dictionary<long, long> output = new Dictionary<long, long>();
        foreach (DayDamage day in temp)
        {
            output.Add(day.day, day.total);
        }
        return output;
    }

    public Dictionary<long, long> GetRecentDaysGuildTotalDamages(long group, int day_size)
    {
        List<DayDamage> temp = _connection.Query<DayDamage>("SELECT day, SUM(damage) as total FROM Damage WHERE day >= ? AND group_number = ? GROUP BY day", GetDay() - day_size, group);
        Dictionary<long, long> output = new Dictionary<long, long>();
        foreach (DayDamage day in temp)
        {
            output.Add(day.day, day.total);
        }
        return output;
    }

    public long GetDamage(long group, long qq, int troop)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ? AND group_number = ?", qq, GetDay(), troop, group);
        if (temp.Count == 0) return 0;
        return temp[0].damage;
    }

    //返回LONG_MIN为新增，其它数值为修改偏移值
    public long AddDamage(long group, long qq, int troop, long damage, int frequency, int boss_num)
    {
        if (CreateDamage(group, qq, troop, damage, frequency, boss_num)) return long.MinValue;

        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ? AND group_number = ?", qq, GetDay(), troop, group);
        if (temp[0].damage == damage) return 0;

        _connection.Update(new Damage()
        {
            id = temp[0].id,
            group_number = temp[0].group_number,
            user = temp[0].user,
            day = temp[0].day,
            troop = temp[0].troop,
            damage = damage,
            frequency = temp[0].frequency,
            boss_num = temp[0].boss_num,
            troop_operator = temp[0].troop_operator
        });
        return damage - temp[0].damage;
    }

    public void SetSL(long group, long qq)
    {
        _connection.Insert(new SaveLoad()
        {
            group_number = group,
            user = qq,
            day = GetDay(),
            time = GetTimeStamp()
        }, "");
    }

    public void RemoveSL(long group, long qq)
    {
        List<SaveLoad> temp = _connection.Query<SaveLoad>("SELECT * FROM SaveLoad WHERE group_number = ? AND user = ? AND day = ?", group, qq, GetDay());
        if (temp.Count == 0) return;
        _connection.Delete(new SaveLoad()
        {
            id = temp[0].id
        });
    }

    public long GetSL(long group, long qq)
    {
        List<SaveLoad> temp = _connection.Query<SaveLoad>("SELECT time FROM SaveLoad WHERE group_number = ? AND user = ? AND day = ?", group, qq, GetDay());
        if (temp.Count == 0) return -1;
        return temp[0].time;
    }

    public List<HelpTroopData> GetHelpTroopNum(long group, int daySize)
    {
        List<HelpTroopData> output = _connection.Query<HelpTroopData>("SELECT troop_operator AS qq, COUNT(*) AS count, SUM(damage) AS totalDamage FROM Damage WHERE day >= ? AND group_number = ? AND troop_operator > 0 GROUP BY troop_operator ORDER BY totalDamage DESC", GetDay() - daySize, group);
        return output;
    }

    public static long GetTimeStamp()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }

    public static long GetDay()
    {
        return ((GetTimeStamp() / 3600) + 3) / 24;
    }

    public static string DayToDate(long day)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        return dateTime.AddDays(day).ToShortDateString();
    }

    public static string ConvertIntDateTime(long d)
    {
        DateTime time = DateTime.MinValue;
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        time = startTime.AddSeconds(d);
        return time.ToLocalTime().ToString();
    }

    protected class Log
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [NotNull]
        public long time { get; set; }
        [NotNull]
        public long group_number { get; set; }
        public string text { get; set; }
    }

    public class Damage
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [NotNull]
        public long group_number { get; set; }
        [NotNull]
        public long user { get; set; }
        [NotNull]
        public long day { get; set; }
        [NotNull]
        public int troop { get; set; }
        [NotNull]
        public long damage { get; set; }
        [NotNull]
        public int frequency { get; set; }
        [NotNull]
        public int boss_num { get; set; }
        public long troop_operator { get; set; }
}

    public class SaveLoad
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [NotNull]
        public long group_number { get; set; }
        [NotNull]
        public long user { get; set; }
        [NotNull]
        public long day { get; set; }
        [NotNull]
        public long time { get; set; }
    }

    public class HelpTroopData
    {
        public long qq { get; set; }
        public int count { get; set; }
        public long totalDamage { get; set; }
    }

    public class DayDamage
    {
        public long user { get; set; }
        public long day { get; set; }
        public long total { get; set; }
    }

    public class Setting
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [Unique]
        public string key { get; set; }
        public string value { get; set; }
    }

    public class TableName
    {
        public string name { get; set; }
    }

    public class DamageComparer : IEqualityComparer<Damage>
    {
        public static DamageComparer Default = new DamageComparer();
        public bool Equals(Damage x, Damage y)
        {
            return x.user.Equals(y.user);
        }
        public int GetHashCode(Damage obj)
        {
            return obj.GetHashCode();
        }
    }
}
