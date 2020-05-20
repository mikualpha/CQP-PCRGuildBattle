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

    private void CreateDamage(long group, long qq, int troop, long damage)
    {
        _connection.Insert(new Damage()
        {
            group_number = group,
            user = qq,
            troop = troop,
            damage = damage,
            day = GetDay()
        }, "");
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

    //0为新增，其它数值为修改偏移值
    public long AddDamage(long group, long qq, int troop, long damage)
    {
        List<Damage> temp = _connection.Query<Damage>("SELECT * FROM Damage WHERE user = ? AND day = ? AND troop = ? AND group_number = ?", qq, GetDay(), troop, group);
        if (temp.Count == 0)
        {
            CreateDamage(group, qq, troop, damage);
            return 0;
        }

        _connection.Update(new Damage()
        {
            id = temp[0].id,
            group_number = temp[0].group_number,
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
    }

    public class DayDamage
    {
        public long user { get; set; }
        public long day { get; set; }
        public long total { get; set; }
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
