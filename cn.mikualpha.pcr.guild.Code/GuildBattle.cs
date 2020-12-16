using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class GuildBattle
{
    private static Dictionary<long, GuildBattle> ins = null;
    private Data data = null;
    private static List<long> bossdata = null;
    private List<long> member = null;
    private long group = 0;
    private static long damageLimit = long.MaxValue;
    private readonly int MAX_TROOP = 6;

    public const int BOSS_MAX = 5;

    private GuildBattle(long _group)
    {
        group = _group;
        InitFile();
        GetBossData();
        GetData();
    }

    public static GuildBattle GetInstance(long group)
    {
        if (ins == null) ins = new Dictionary<long, GuildBattle>();
        if (!ins.ContainsKey(group))
        {
            lock(ins) { if (!ins.ContainsKey(group)) ins.Add(group, new GuildBattle(group)); }
        }
        return ins[group];
    }

    #region 业务接口
    public static string GetSignChar() { return FileOptions.GetInstance().GetOptions()["MemberChar"]; }

    public static long GetDamageLimit()
    {
        if (damageLimit != long.MaxValue) return damageLimit;

        long output;
        if (!long.TryParse(FileOptions.GetInstance().GetOptions()["DamageLimit"], out output)) return long.MaxValue;
        damageLimit = output;
        return damageLimit;
    }

    //遗留函数，现在当开关用
    public void SetActive(bool active)
    {
        data.isActive = active;
        SaveData();
    }

    //遗留函数，现在当开关用
    public bool GetActive() { return data.isActive; }

    public string GetBossStatus() {
        return "【公会战BOSS状态】\n" +
            (data.treeUser.Count == 0 ? "" : "【注意】 目前有 " + data.treeUser.Count.ToString() + " 人正在挂树！\n") +
            "[第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS] 剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString();
    }

    public void AddBattleUser(long qq, long helper = 0)
    {
        if (data.battleUser.Contains(qq)) return;
        data.battleUser.Add(qq);
        //发送代刀消息
        if (helper > 0) AddHelpInfo(qq, helper);
        SaveData();

        long SLTime = GetSLStatus(qq); // SL时间，-1为未SL
        ApiModel.CQApi.SendGroupMessage(group, "战斗状态已记录！目前战斗状态列表：\n" + PrintList(group, GetBattleUser()) + "\n\n" + 
            (data.treeUser.Count == 0 ? "" : "【注意】 目前有 " + data.treeUser.Count.ToString() + " 人正在挂树！\n") +
            (SLTime == -1 ? "" : "【注意】 " + (helper > 0 ? "该账号" : "您") + "今日已于" + SQLiteManager.ConvertIntDateTime(SLTime) + "(GMT+8) 进行过SL操作！\n") +
            "第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS 剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString()
        );
    }

    public void ClearBattleUser() {
        data.battleUser.Clear();
        data.treeUser.Clear();
        SaveData();
        ApiModel.CQApi.SendGroupMessage(group, "战斗列表已清空！");
    }

    public void ClearTreeUser() {
        data.treeUser.Clear();
        SaveData();
        ApiModel.CQApi.SendGroupMessage(group, "挂树列表已清空！");
    }

    public List<long> GetBattleUser() { return data.battleUser; }

    public void RemoveBattleUser(long qq)
    {
        if (!data.battleUser.Contains(qq)) return;
        data.battleUser.Remove(qq);
        RemoveHelpInfo(qq, false); // 移除代刀数据
        SaveData();
        ApiModel.CQApi.SendGroupMessage(group, "已移除战斗状态！目前战斗状态列表：\n" + PrintList(group, GetBattleUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS 剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString());

    }

    public void AddTreeUser(long qq)
    {
        if (data.treeUser.Contains(qq)) return;
        if (!data.battleUser.Contains(qq))
        {
            ApiModel.CQApi.SendGroupMessage(group, "设置挂树状态前请先进入战斗状态！");
            return;
        }
        data.treeUser.Add(qq);
        SaveData();
        SQLiteManager.GetInstance().AddLog(group, "[" + GetUserName(group, qq) + "] 挂在树上了...");
        string outputStr = "挂树状态已记录！目前挂树状态列表：\n" + PrintList(group, GetTreeUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS 剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString();
        string treeAdminStr = GetTreeAdminStr();
        if (treeAdminStr != "") outputStr += "\n" + treeAdminStr + "快组织救人啦！";
        ApiModel.CQApi.SendGroupMessage(group, outputStr);
    }

    public List<long> GetTreeUser() { return data.treeUser; }

    public void RemoveTreeUser(long qq)
    {
        if (!data.treeUser.Contains(qq)) return;
        data.treeUser.Remove(qq);
        SaveData();
        ApiModel.CQApi.SendGroupMessage(group, "已移除挂树状态！目前挂树状态列表：\n" + PrintList(group, GetTreeUser()) + "\n\n第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS 剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString());
    }

    public void PushDamage(long qq, int troop_num, long damage, bool can_modify = false, long troop_operator = -1)
    {
        if (troop_num > MAX_TROOP || troop_num < 1)
        {
            ApiModel.CQApi.SendGroupMessage(group, "输入的队伍编号不正确，应为1~3(补刀可填4~6)！");
            return;
        }

        if (damage >= GetDamageLimit())
        {
            ApiModel.CQApi.SendGroupMessage(group, "输入伤害超出系统限制(" + GetDamageLimit().ToString() + ")！");
            return;
        }

        if (data.battleUser.Contains(qq)) data.battleUser.Remove(qq);
        if (data.treeUser.Contains(qq)) data.treeUser.Remove(qq);
        if (data.helpInfo.ContainsKey(qq)) RemoveHelpInfo(qq, true);

        string text = "[第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS]";

        long addDamage;
        if (can_modify)
        {
            addDamage = SQLiteManager.GetInstance().AddDamage(group, qq, troop_num, Min(damage, bossdata[data.bossNumber - 1] - data.damage), data.frequency, data.bossNumber);
        } else {
            bool isLastTroop = (damage >= bossdata[data.bossNumber - 1] - data.damage);
            if (SQLiteManager.GetInstance().CreateDamage(group, qq, troop_num, Min(damage, bossdata[data.bossNumber - 1] - data.damage), 
                data.frequency, data.bossNumber, troop_operator, isLastTroop, SQLiteManager.GetInstance().IsRemiburseTroopToday(group, qq)))
            {
                addDamage = long.MinValue;
            } else
            {
                addDamage = 0; //指令无效
            }
        }

        if (addDamage == long.MinValue)
        {
            data.damage += damage;
            SQLiteManager.GetInstance().AddLog(group, "[" + GetUserName(group, qq) + "] 的第" + troop_num.ToString() + "队对第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS造成了" + damage.ToString() + "伤害");

            text += "\n" + "[" + GetUserName(group, qq) + "] 的第" + troop_num.ToString() + "队共造成了" + damage.ToString() + "伤害";
        } else if (addDamage != 0)
        {
            data.damage += addDamage;
            SQLiteManager.GetInstance().AddLog(group, "[" + GetUserName(group, qq) + "] 将第" + troop_num.ToString() + "队造成的伤害由" + (damage - addDamage).ToString() + "修改为" + damage.ToString());

            text += "\n" + "[" + GetUserName(group, qq) + "] 将第" + troop_num.ToString() + "队造成的伤害由" + (damage - addDamage).ToString() + "修改为" + damage.ToString();
        } else
        {
            if (can_modify) text += "\n" + "您本次的伤害数据已经被正确记录！该指令无效！";
            else text += "\n" + "您本队伍的伤害数据已有记录！该指令无效！";
        }
        
        if (data.damage >= bossdata[data.bossNumber - 1])
        {
            SQLiteManager.GetInstance().AddLog(group, "第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS已被击败");
            data.damage = 0;
            data.treeUser.Clear();
            data.battleUser.Clear();
            data.bossNumber += 1;
            if (data.bossNumber > BOSS_MAX)
            {
                data.bossNumber = 1;
                data.frequency += 1;
            }
            text += "\n" + "该BOSS已被击败，下一个BOSS为:" +
                "\n" + "第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS HP: " + bossdata[data.bossNumber - 1].ToString();
            ApiModel.CQApi.SendGroupMessage(group, text);

            string atStr = GetSubscribeStr(data.bossNumber);
            if (atStr != "")
            {
                ApiModel.CQApi.SendGroupMessage(group, "[BOSS预约提醒] 您预约的BOSS已出现，请及时出刀\n" + atStr);
            }
            
        } else
        {
            text += "\n" + "该BOSS剩余血量: " + (bossdata[data.bossNumber - 1] - data.damage).ToString();
            ApiModel.CQApi.SendGroupMessage(group, text);
        }
        SaveData();
    }

    public void SetDamage(long lessBlood)
    {
        data.damage = bossdata[data.bossNumber - 1] - lessBlood;
        SaveData();
        SQLiteManager.GetInstance().AddLog(group, "已将BOSS剩余血量重置为 " + (bossdata[data.bossNumber - 1] - data.damage).ToString());
        ApiModel.CQApi.SendGroupMessage(group, "已将BOSS血量数据重置！" +
            "\n" + "该BOSS剩余HP: " + (bossdata[data.bossNumber - 1] - data.damage).ToString());
    }

    public void SetFrequency(int frequency, int boss_num)
    {
        data.frequency = frequency;
        data.bossNumber = boss_num;
        data.damage = 0;
        SaveData();
        SQLiteManager.GetInstance().AddLog(group, "已将BOSS数据重置为第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS");
        ApiModel.CQApi.SendGroupMessage(group, "已将BOSS数据重置为第" + data.frequency.ToString() + "周目 " + data.bossNumber.ToString() + "号BOSS，对BOSS造成的伤害已重置" +
            "\n" + "该BOSS剩余HP: " + bossdata[data.bossNumber - 1].ToString());
    }

    private void AddHelpInfo(long account, long helper)
    {
        if (!FileOptions.GetInstance().CanHelpSignal()) return;

        data.helpInfo.Add(account, helper);
        ApiModel.CQApi.SendPrivateMessage(account, "[" + GuildBattle.GetUserName(group, helper) + "] 代刀中，请注意避免重复登录导致不必要的损失");
    }

    private void RemoveHelpInfo(long account, bool isSuccess = true)
    {
        if (!FileOptions.GetInstance().CanHelpSignal()) return;
        if (data.helpInfo.ContainsKey(account))
        {
            if ( isSuccess ) ApiModel.CQApi.SendPrivateMessage(account, "[" + GuildBattle.GetUserName(group, data.helpInfo[account]) + "] 已完成本次出刀\n(如造成骚扰可屏蔽本窗口对话)");
            else ApiModel.CQApi.SendPrivateMessage(account, "[" + GuildBattle.GetUserName(group, data.helpInfo[account]) + "] 已取消本次出刀\n(如造成骚扰可屏蔽本窗口对话)");
            data.helpInfo.Remove(account);
        }
    }

    public string GetHelpTroopNum()
    {
        List<SQLiteManager.HelpTroopData> helpTroopData = SQLiteManager.GetInstance().GetHelpTroopNum(group, 10);
        string output = "【代刀数统计(含补刀)】";

        int totalTroop = 0, totalReimburseTroop = 0;
        long allTotalDamage = 0;  // 所有人代刀总伤害
        foreach (SQLiteManager.HelpTroopData data in helpTroopData)
        {
            totalTroop += data.count;
            totalReimburseTroop += data.reimburseCount;
            allTotalDamage += data.totalDamage;
            output += "\n" + GetUserName(group, data.qq) + "\t\t" + (data.count - data.reimburseCount).ToString();
            if (data.reimburseCount > 0) output += "(+" + data.reimburseCount.ToString() + ")";
            output += "刀\t伤害: " + data.totalDamage.ToString();
        }
        output += "\n【总代刀数(含补刀)】 " + (totalTroop - totalReimburseTroop).ToString();
        if (totalReimburseTroop > 0) output += "(+" + totalReimburseTroop.ToString() + ")";
        output += " 刀";
        output += "\n【代刀总伤害】 " + allTotalDamage.ToString();
        return output;
    }

    public void AddMessage(long qq, string message)
    {
        if (data.messages.ContainsKey(qq)) RemoveMessage(qq);
        data.messages.Add(qq, message);
        SaveData();
    }

    public void RemoveMessage(long qq)
    {
        if (!data.messages.ContainsKey(qq)) return;
        data.messages.Remove(qq);
        SaveData();
    }

    public void ClearMessage()
    {
        data.messages.Clear();
        SaveData();
    }

    public Dictionary<long, string> GetMessages() { return data.messages; }

    public string GetSubscribeStr(int boss_num)
    {
        string output = "";
        List<long> removeList = new List<long>();
        Dictionary<long, int> temp = new Dictionary<long, int>(data.subscribe);
        foreach (KeyValuePair<long, int> kvp in temp)
        {
            if (kvp.Value != boss_num) continue;
            output += "[CQ:at,qq=" + kvp.Key + "] ";
            removeList.Add(kvp.Key);
        }

        for (int i = 0; i < removeList.Count; ++i) data.subscribe.Remove(removeList[i]);
        return output;
    }

    public List<string> GetSubscribeList()
    {
        List<string> output = new List<string>();
        List<List<string>> temp = new List<List<string>>();

        for (int i = 0; i < BOSS_MAX; ++i) temp.Add(new List<string>());

        foreach (KeyValuePair<long, int> kvp in data.subscribe)
        {
            temp[kvp.Value - 1].Add("[" + GetUserName(group, kvp.Key) + "] 第" + kvp.Value + "号BOSS");
        }

        for (int i = 0; i < BOSS_MAX; ++i) output.AddRange(temp[i]);
        return output;
    }

    public bool AddSubscribe(long qq, int boss_num)
    {
        if (data.bossNumber == boss_num) return false;
        if (data.subscribe.ContainsKey(qq)) data.subscribe.Remove(qq);
        data.subscribe.Add(qq, boss_num);
        SaveData();
        return true;
    }

    public void RemoveSubscribe(long qq)
    {
        if (data.subscribe.ContainsKey(qq)) data.subscribe.Remove(qq);
        SaveData();
    }

    public void ClearSubscribe()
    {
        data.subscribe.Clear();
        SaveData();
    }

    public bool SetSL(long qq)
    {
        if (GetSLStatus(qq) > 0) return false;
        SQLiteManager.GetInstance().SetSL(group, qq);
        return true;
    }

    public bool RemoveSL(long qq)
    {
        if (GetSLStatus(qq) == -1) return false;
        SQLiteManager.GetInstance().RemoveSL(group, qq);
        return true;
    }

    public long GetSLStatus(long qq)
    {
        return SQLiteManager.GetInstance().GetSL(group, qq);
    }

    public static string GetUserName(long group, long qq)
    {
        GroupMemberInfo info = ApiModel.CQApi.GetGroupMemberInfo(group, qq, true);
        if (info.Card != null && info.Card != "")
        {
            if (GuildBattle.GetInstance(group).GetMemberList() == null) return info.Card.Replace(GetSignChar(), "") + "(" + qq.ToString() + ")";
            return info.Card + "(" + qq.ToString() + ")";
        }
        return info.Nick + "(" + qq.ToString() + ")";
    }

    public static string PrintList(long group, List<long> input)
    {
        if (input.Count == 0) return "无";
        string output = "";
        for (int i = 0; i < input.Count; ++i)
        {
            if (output != "") output += "\n";
            output += GetUserName(group, input[i]);
        }
        return output;
    }
    #endregion

    #region 文件读写类接口
    private void GetData()
    {
        string path = ApiModel.CQApi.AppDirectory + "Data-" + group.ToString() + ".ini";

        if (File.Exists(path))
        {
            data = JsonConvert.DeserializeObject<Data>(ReadFromFile(path));
        }
        else
        {
            data = new Data();
            data.isActive = false;
            data.frequency = 1;
            data.bossNumber = 1;
            data.damage = 0;
        }

        if (data.battleUser == null) data.battleUser = new List<long>();
        if (data.treeUser == null) data.treeUser = new List<long>();
        if (data.messages == null) data.messages = new Dictionary<long, string>();
        if (data.subscribe == null) data.subscribe = new Dictionary<long, int>();
        if (data.helpInfo == null) data.helpInfo = new Dictionary<long, long>();
    }

    private void SaveData()
    {
        string path = ApiModel.CQApi.AppDirectory + "Data-" + group.ToString() + ".ini";
        string text = JsonConvert.SerializeObject(data);
        WriteToFile(path, text);
    }

    public static void SaveAllData()
    {
        foreach(KeyValuePair<long, GuildBattle> temp in ins)
        {
            temp.Value.SaveData();
        }
    }

    private void GetBossData()
    {
        if (bossdata != null) return;
        bossdata = new List<long>();
        if (File.Exists(ApiModel.CQApi.AppDirectory + "Boss.ini"))
        {
            string[] list = ReadFromFile(ApiModel.CQApi.AppDirectory + "Boss.ini").Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < list.Length; ++i)
            {
                long num;
                if (!long.TryParse(list[i], out num))
                {
                    ApiModel.CQLog.Warning("BossData", "BOSS血量数据读取失败，格式不正确！");
                    InitBossData();
                    return;
                }
                bossdata.Add(num);
            }
        } else {
            InitBossData();
        }
    }

    private void InitBossData()
    {
        bossdata.Add(6000000);
        bossdata.Add(8000000);
        bossdata.Add(10000000);
        bossdata.Add(12000000);
        bossdata.Add(20000000);
    }

    public bool isAdmin(long qq)
    {
        string adminStr = FileOptions.GetInstance().GetOptions()["Admin"];
        string[] list = adminStr.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < list.Length; ++i)
        {
            long num;
            if (!long.TryParse(list[i], out num))
            {
                ApiModel.CQLog.Warning("AdminData", "管理员列表读取失败，格式不正确！");
                return false;
            }
            if (num == 0 || num == qq) return true;
        }
        return false;
    }

    private string GetTreeAdminStr()
    {
        string treeAdminStr = FileOptions.GetInstance().GetOptions()["TreeAdmin"];

        if (treeAdminStr == "0" || treeAdminStr == "") return "";

        string[] list = treeAdminStr.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        string output = "";
        for (int i = 0; i < list.Length; ++i)
        {
            long num;
            if (!long.TryParse(list[i], out num))
            {
                ApiModel.CQLog.Warning("TreeAdminData", "挂树通知列表读取失败，格式不正确！");
                return "";
            }
            output += "[CQ:at,qq=" + num + "] ";
        }
        return output;
    }

    public List<long> GetMemberList()
    {
        string memberStr = FileOptions.GetInstance().GetOptions()["Member"];
        if (memberStr == "0") return null;

        if (member != null) return member;

        string[] list = memberStr.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        List<long> output = new List<long>();
        for (int i = 0; i < list.Length; ++i)
        {
            long num;
            if (!long.TryParse(list[i], out num))
            {
                ApiModel.CQLog.Warning("MemberListData", "成员列表读取失败，格式不正确！");
                return null;
            }
            output.Add(num);
        }
        member = output;
        return output;
    }

    public static void InitFile()
    {
        FileOptions.GetInstance();
        if (!File.Exists(ApiModel.CQApi.AppDirectory + "Boss.ini"))
            WriteToFile(ApiModel.CQApi.AppDirectory + "Boss.ini", "6000000,8000000,10000000,12000000,20000000");
    }

    private static void WriteToFile(string filename, string content)
    {
        string[] contents = { content };
        WriteToFile(filename, contents);
    }

    private static void WriteToFile(string filename, string[] contents)
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

    private long Min(long a, long b)
    {
        return (a > b ? b : a);
    }
    #endregion

    public class Data
    {
        public bool isActive { get; set; }
        public int frequency { get; set; }
        public int bossNumber { get; set; }
        public long damage { get; set; }
        public List<long> battleUser { get; set; }
        public List<long> treeUser { get; set; }
        public Dictionary<long, string> messages { get; set; }
        public Dictionary<long, int> subscribe { get; set; }
        public Dictionary<long, long> helpInfo { get; set; }
    }
}

