using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class GuildBattle
{
    private static GuildBattle ins = null;
    private static Data data = null;
    private static List<long> bossdata = null;

    private GuildBattle()
    {
        InitFile();
        GetBossData();
        GetData();
    }

    public static GuildBattle GetInstance()
    {
        if (ins == null) ins = new GuildBattle();
        return ins;
    }

    public static string GetSignChar() { return "*"; }

    #region 业务接口
    public void SetGroup(long group)
    {
        if (data.group == -1) data.group = group;
        SaveData();
    }

    public long GetGroup() { return data.group; }

    public void AddBattleUser(long qq)
    {
        if (data.battleUser.Contains(qq)) return;
        data.battleUser.Add(qq);
        SaveData();
        ApiModel.CQApi.SendGroupMessage(data.group, "战斗状态已记录！目前战斗状态列表：\n" + PrintList(GetBattleUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS剩余HP: " + (bossdata[data.boss_number - 1] - data.damage).ToString());
    }

    public void ClearBattleUser() {
        data.battleUser.Clear();
        data.treeUser.Clear();
        ApiModel.CQApi.SendGroupMessage(data.group, "战斗列表已清空！");
    }

    public void ClearTreeUser() {
        data.treeUser.Clear();
        ApiModel.CQApi.SendGroupMessage(data.group, "挂树列表已清空！");
    }

    public List<long> GetBattleUser() { return data.battleUser; }

    public void RemoveBattleUser(long qq)
    {
        if (!data.battleUser.Contains(qq)) return;
        data.battleUser.Remove(qq);
        SaveData();
        ApiModel.CQApi.SendGroupMessage(data.group, "已移除战斗状态！目前战斗状态列表：\n" + PrintList(GetBattleUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS剩余HP: " + (bossdata[data.boss_number - 1] - data.damage).ToString());
    }

    public void AddTreeUser(long qq)
    {
        if (data.treeUser.Contains(qq)) return;
        if (!data.battleUser.Contains(qq))
        {
            ApiModel.CQApi.SendGroupMessage(data.group, "设置挂树状态前请先进入战斗状态！");
            return;
        }
        data.treeUser.Add(qq);
        SaveData();
        SQLiteManager.GetInstance().AddLog("[" + GetUserName(qq) + "] 挂在树上了...");
        ApiModel.CQApi.SendGroupMessage(data.group, "挂树状态已记录！目前挂树状态列表：\n" + PrintList(GetTreeUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS剩余HP: " + (bossdata[data.boss_number - 1] - data.damage).ToString());
    }

    public List<long> GetTreeUser() { return data.treeUser; }

    public void RemoveTreeUser(long qq)
    {
        if (!data.treeUser.Contains(qq)) return;
        data.treeUser.Remove(qq);
        SaveData();
        ApiModel.CQApi.SendGroupMessage(data.group, "已移除挂树状态！目前挂树状态列表：\n" + PrintList(GetTreeUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS剩余HP: " + (bossdata[data.boss_number - 1] - data.damage).ToString());
    }

    public void PushDamage(long qq, int troop_num, long damage)
    {
        if (troop_num > 4 || troop_num < 1)
        {
            ApiModel.CQApi.SendGroupMessage(data.group, "输入的队伍编号不正确，应为1~3(补刀可填4)！");
            return;
        }

        if (data.battleUser.Contains(qq)) data.battleUser.Remove(qq);
        if (data.treeUser.Contains(qq)) data.treeUser.Remove(qq);

        string text = "";
        long addDamage = SQLiteManager.GetInstance().AddDamage(qq, troop_num, damage);
        if (addDamage == 0)
        {
            data.damage += damage;
            text = "[" + GetUserName(qq) + "] 的第" + troop_num.ToString() + "队对第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS造成了" + damage.ToString() + "伤害";
            SQLiteManager.GetInstance().AddLog(text);

            text = "[第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS]" +
                "\n" + "[" + GetUserName(qq) + "] 的第" + troop_num.ToString() + "队共造成了" + damage.ToString() + "伤害";
        } else
        {
            data.damage += addDamage;
            text = "[" + GetUserName(qq) + "] 将第" + troop_num.ToString() + "队造成的伤害由" + (damage - addDamage).ToString() + "修改为" + damage.ToString();
            SQLiteManager.GetInstance().AddLog(text);

            text = "[第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS]" +
                "\n" + "[" + GetUserName(qq) + "] 将第" + troop_num.ToString() + "队造成的伤害由" + (damage - addDamage).ToString() + "修改为" + damage.ToString();
        }
        
        if (data.damage >= bossdata[data.boss_number - 1])
        {
            SQLiteManager.GetInstance().AddLog("第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS已被击败");
            data.damage = 0;
            data.treeUser.Clear();
            data.battleUser.Clear();
            data.boss_number += 1;
            if (data.boss_number > 5)
            {
                data.boss_number = 1;
                data.frequency += 1;
            }
            text += "\n" + "该BOSS已被击败，下一个BOSS为:" +
                "\n" + "第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS HP: " + bossdata[data.boss_number - 1].ToString();
        } else
        {
            text += "\n" + "该BOSS剩余血量: " + (bossdata[data.boss_number - 1] - data.damage).ToString();
        }
        SaveData();
        ApiModel.CQApi.SendGroupMessage(data.group, text);
    }

    public void ChangeDamage() { }

    public void SetDamage(long lessBlood)
    {
        data.damage = bossdata[data.boss_number - 1] - lessBlood;
        SaveData();
        SQLiteManager.GetInstance().AddLog("已将BOSS剩余血量重置为 " + (bossdata[data.boss_number - 1] - data.damage).ToString());
        ApiModel.CQApi.SendGroupMessage(data.group, "已将BOSS血量数据重置！" +
            "\n" + "该BOSS剩余HP: " + (bossdata[data.boss_number - 1] - data.damage).ToString());
    }

    public void SetFrequency(int frequency, int boss_num)
    {
        data.frequency = frequency;
        data.boss_number = boss_num;
        data.damage = 0;
        SaveData();
        SQLiteManager.GetInstance().AddLog("已将BOSS数据重置为第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS");
        ApiModel.CQApi.SendGroupMessage(data.group, "已将BOSS数据重置为第" + data.frequency.ToString() + "周目 " + data.boss_number.ToString() + "号BOSS，对BOSS造成的伤害已重置" +
            "\n" + "该BOSS剩余HP: " + bossdata[data.boss_number - 1].ToString());
    }

    public static string GetUserName(long qq)
    {
        GroupMemberInfo info = ApiModel.CQApi.GetGroupMemberInfo(data.group, qq, true);
        if (info.Card != null && info.Card != "") return info.Card.Replace(GetSignChar(), "") + "(" + qq.ToString() + ")";
        return info.Nick + "(" + qq.ToString() + ")";
    }

    public static string PrintList(List<long> input)
    {
        if (input.Count == 0) return "无";
        string output = "";
        for (int i = 0; i < input.Count; ++i)
        {
            if (output != "") output += "\n";
            output += GetUserName(input[i]);
        }
        return output;
    }
    #endregion

    #region 文件读写类接口
    private void GetData()
    {
        string path = ApiModel.CQApi.AppDirectory + "data.ini";
        if (File.Exists(path))
        {
            data = JsonConvert.DeserializeObject<Data>(ReadFromFile(path));
        } else
        {
            data = new Data();
            data.group = -1;
            data.frequency = 1;
            data.boss_number = 1;
            data.damage = 0;
            data.battleUser = new List<long>();
            data.treeUser = new List<long>();
        }
    }

    private void SaveData()
    {
        string path = ApiModel.CQApi.AppDirectory + "data.ini";
        string text = JsonConvert.SerializeObject(data);
        WriteToFile(path, text);
    }

    private void GetBossData()
    {
        bossdata = new List<long>();
        if (File.Exists(ApiModel.CQApi.AppDirectory + "boss.ini"))
        {
            string[] list = ReadFromFile(ApiModel.CQApi.AppDirectory + "boss.ini").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < list.Length; ++i)
            {
                long num = long.Parse(list[i]);
                bossdata.Add(num);
            }

        } else {
            bossdata.Add(6000000);
            bossdata.Add(8000000);
            bossdata.Add(10000000);
            bossdata.Add(12000000);
            bossdata.Add(20000000);
        }
    }

    public bool isAdmin(long qq)
    {
        string[] list = ReadFromFile(ApiModel.CQApi.AppDirectory + "admin.ini").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < list.Length; ++i)
        {
            long num = long.Parse(list[i]);
            if (num == 0 || num == qq) return true;
        }
        return false;
    }

    private void InitFile()
    {
        if (!File.Exists(ApiModel.CQApi.AppDirectory + "admin.ini"))
            WriteToFile(ApiModel.CQApi.AppDirectory + "admin.ini", "123456789,987654321");
        if (!File.Exists(ApiModel.CQApi.AppDirectory + "boss.ini"))
            WriteToFile(ApiModel.CQApi.AppDirectory + "boss.ini", "6000000,8000000,10000000,12000000,20000000");
    }

    private void WriteToFile(string filename, string content)
    {
        string[] contents = { content };
        WriteToFile(filename, contents);
    }

    private void WriteToFile(string filename, string[] contents)
    {
        using (StreamWriter file = new StreamWriter(@filename, false, Encoding.UTF8))
        {
            foreach (string line in contents)
            {
                file.WriteLine(line);
            }
        }
    }

    private string ReadFromFile(string filename)
    {
        string output = "";
        using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                output = line;
                break;
            }
            return output;
        }
    }
    #endregion

    public class Data
    {
        public long group { get; set; }
        public int frequency { get; set; }
        public int boss_number { get; set; }
        public long damage { get; set; }
        public List<long> battleUser { get; set; }
        public List<long> treeUser { get; set; }
    }
}

