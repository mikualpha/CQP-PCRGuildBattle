using SQLite;
using System;
using System.Collections.Generic;

class SQLiteManager
{
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
        _connection.CreateTable<Log>();
        _connection.CreateTable<Damage>();
    }

    public void AddLog(string _text)
    {
        _connection.Insert(new Log()
        {
            time = GetTimeStamp(),
            text = _text
        }, "");
    }

    public List<string> GetLogs()
    {
        List<Log> temp = _connection.Query<Log>("SELECT * FROM Log ORDER BY id DESC LIMIT 7");
        List<string> output = new List<string>();
        for (int i = 0; i < temp.Count; ++i)
        {
            output.Add("[" + ConvertIntDateTime(temp[i].time) + "] " + temp[i].text);
        }
        return output;
    }

    private void CreateDamage(long qq, int troop, long damage)
    {
        _connection.Insert(new Damage()
        {
            user = qq,
            troop = troop,
            damage = damage,
            day = GetDay()
        }, "");
    }

    public List<Damage> GetTodayDamages(long qq)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE day = ? AND user = ?", GetDay(), qq);
        return temp;
    }

    public List<Damage> GetTodayDamages()
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT id, user, day, COUNT(troop) as troop, SUM(damage) as damage FROM Damage WHERE day = ? GROUP BY user", GetDay());
        return temp;
    }

    public Dictionary<long, long> GetRecentDaysDamages(long qq, int day_size)
    {
        List<DayDamage> temp = _connection.Query<DayDamage>("SELECT user, day, SUM(damage) as total FROM Damage WHERE day >= ? AND user = ? GROUP BY day", GetDay() - day_size, qq);
        Dictionary<long, long> output = new Dictionary<long, long>();
        foreach (DayDamage day in temp)
        {
            output.Add(day.day, day.total);
        }
        return output;
    }

    public Dictionary<long, long> GetRecentDaysGuildTotalDamages(int day_size)
    {
        List<DayDamage> temp = _connection.Query<DayDamage>("SELECT user, day, SUM(damage) as total FROM Damage WHERE day >= ? GROUP BY day", GetDay() - day_size);
        Dictionary<long, long> output = new Dictionary<long, long>();
        foreach (DayDamage day in temp)
        {
            output.Add(day.day, day.total);
        }
        return output;
    }

    public long GetDamage(long qq, int troop)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ?", qq, GetDay(), troop);
        if (temp.Count == 0) return 0;
        return temp[0].damage;
    }

    //0为新增，其它数值为修改偏移值
    public long AddDamage(long qq, int troop, long damage)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ?", qq, GetDay(), troop);
        if (temp.Count == 0)
        {
            CreateDamage(qq, troop, damage);
            return 0;
        }

        _connection.Update(new Damage()
        {
            id = temp[0].id,
            user = temp[0].user,
            day = temp[0].day,
            troop = temp[0].troop,
            damage = damage
        });
        return damage - temp[0].damage;
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

    private string ConvertIntDateTime(long d)
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
        public string text { get; set; }
    }

    public class Damage
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        [NotNull]
        public long user { get; set; }
        [NotNull]
        public long day { get; set; }
        [NotNull]
        public int troop { get; set; }
        [NotNull]
        public long damage { get; set; }
    }

    public class DayDamage
    {
        public long user { get; set; }
        public long day { get; set; }
        public long total { get; set; }
    }
}
