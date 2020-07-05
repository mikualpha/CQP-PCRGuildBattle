using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class Event_GroupMessage : IGroupMessage
{
    public void GroupMessage(object sender, CQGroupMessageEventArgs e)
    {
        if (e.IsFromAnonymous)
        {
            e.Handler = false;
            return;
        }

        if (e.Message.Text.Contains("#启用公会战工具") && isAdmin(e))
        {
            if (GuildBattle.GetInstance(e.FromGroup.Id).GetActive()) e.CQApi.SendGroupMessage(e.FromGroup.Id, "工具已启用，该指令无效！");
            //else if (GuildBattle.GetInstance(e.FromGroup.Id).GetGroup() != -1) e.CQApi.SendGroupMessage(e.FromGroup.Id, "已在其它群启用工具，请先禁用！");
            else
            {
                GuildBattle.GetInstance(e.FromGroup.Id).SetActive(true);
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功启用！");
            }
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("#help"))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "[指令列表]\n" + "https://docs.qq.com/sheet/DYXBDZ1RmRXdXR0dH");
            e.Handler = true;
            return;
        }

        if (!GuildBattle.GetInstance(e.FromGroup.Id).GetActive())
        {
            e.Handler = false;
            return;
        }

        if (e.Message.Text.Contains("#禁用公会战工具") && isAdmin(e))
        {
            if (!GuildBattle.GetInstance(e.FromGroup.Id).GetActive()) e.CQApi.SendGroupMessage(e.FromGroup.Id, "工具已禁用，该指令无效！");
            else
            {
                GuildBattle.GetInstance(e.FromGroup.Id).SetActive(false);
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功禁用！");
            }
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("公会战状态"))
        {
            List<string> logs = SQLiteManager.GetInstance().GetLogs(e.FromGroup.Id);
            string text = "【日志记录】";

            for (int i = 0; i < logs.Count; ++i) text += "\n" + logs[i];

            text += "\n\n" + "【战斗列表】" + "\n" + GuildBattle.PrintList(e.FromGroup.Id, GuildBattle.GetInstance(e.FromGroup.Id).GetBattleUser());
            text += "\n\n" + "【挂树列表】" + "\n" + GuildBattle.PrintList(e.FromGroup.Id, GuildBattle.GetInstance(e.FromGroup.Id).GetTreeUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("出刀列表"))
        {
            string text = "【战斗列表】" + "\n" + GuildBattle.PrintList(e.FromGroup.Id, GuildBattle.GetInstance(e.FromGroup.Id).GetBattleUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("挂树列表"))
        {
            string text = "【挂树列表】" + "\n" + GuildBattle.PrintList(e.FromGroup.Id, GuildBattle.GetInstance(e.FromGroup.Id).GetTreeUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }


        if (e.Message.Text.Equals("我挂树了") || e.Message.Text.Equals("救救救"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).AddTreeUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Contains("我去去就来") || e.Message.Text.Equals("申请出刀"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).AddBattleUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.StartsWith("取消挂树 [CQ:at,qq=") || e.Message.Text.StartsWith("取消挂树[CQ:at,qq=")) && isAdmin(e))
        {
            long qq = GetOperateQQ(e.Message.Text);
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveTreeUser(qq);
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.StartsWith("取消出刀 [CQ:at,qq=") || e.Message.Text.StartsWith("取消出刀[CQ:at,qq=")) && isAdmin(e))
        {
            long qq = GetOperateQQ(e.Message.Text);
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveBattleUser(qq);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("取消挂树"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveTreeUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("取消出刀"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveBattleUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.StartsWith("伤害 [CQ:at,qq=") || e.Message.Text.StartsWith("修改伤害 [CQ:at,qq=")) && isAdmin(e))
        {
            long qq = GetOperateQQ(e.Message.Text);

            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int troop_num;
            long damage;

            if (temp.Length != 4 || !int.TryParse(temp[2], out troop_num) || !long.TryParse(temp[3], out damage))
            {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            if (e.Message.Text.StartsWith("修改伤害 [CQ:at,qq=")) GuildBattle.GetInstance(e.FromGroup.Id).PushDamage(qq, troop_num, damage, true);
            else GuildBattle.GetInstance(e.FromGroup.Id).PushDamage(qq, troop_num, damage, false);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("伤害 ") || e.Message.Text.StartsWith("修改伤害 "))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int troop_num;
            long damage;

            if (temp.Length != 3 || !int.TryParse(temp[1], out troop_num) || !long.TryParse(temp[2], out damage)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            if (e.Message.Text.StartsWith("修改伤害 ")) GuildBattle.GetInstance(e.FromGroup.Id).PushDamage(e.FromQQ.Id, troop_num, damage, true);
            else GuildBattle.GetInstance(e.FromGroup.Id).PushDamage(e.FromQQ.Id, troop_num, damage, false);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("设置BOSS ") && isAdmin(e))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int frequency, boss_num;

            if (temp.Length != 3 || !int.TryParse(temp[1], out frequency) || !int.TryParse(temp[2], out boss_num)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }
            GuildBattle.GetInstance(e.FromGroup.Id).SetFrequency(frequency, boss_num);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("设置血量 ") && isAdmin(e))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 2) e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
            long blood;
            if (!long.TryParse(temp[1], out blood)) e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
            GuildBattle.GetInstance(e.FromGroup.Id).SetDamage(blood);
            e.Handler = true;
            return;
        }


        if (e.Message.Text.Equals("清空出刀") && isAdmin(e))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).ClearBattleUser();
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("清空挂树") && isAdmin(e))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).ClearTreeUser();
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("今日伤害 [CQ:at,qq=") && isAdmin(e) && !FileOptions.GetInstance().isDisableAt())
        {
            long qq = GetOperateQQ(e.Message.Text);
            e.CQApi.SendPrivateMessage(e.FromQQ.Id, GetTodayDamage(e.FromGroup.Id, qq));
            //e.CQApi.SendPrivateMessage(qq, "[" + GuildBattle.GetUserName(e.FromGroup.Id, e.FromQQ.Id) + "] 查询了您的今日伤害数据");
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "相关数据已通过私聊发送！");
            e.Handler = true;
            return;
        }

        if (Regex.IsMatch(e.Message.Text, @"今日伤害 (\d+)") && isAdmin(e))
        {
            Match match = Regex.Match(e.Message.Text, @"今日伤害 (\d+)");
            long qq;
            if (!long.TryParse(match.Groups[1].Value, out qq)) return;
            e.CQApi.SendPrivateMessage(e.FromQQ.Id, GetTodayDamage(e.FromGroup.Id, qq));
            e.CQApi.SendPrivateMessage(qq, "[" + GuildBattle.GetUserName(e.FromGroup.Id, e.FromQQ.Id) + "] 查询了您的今日伤害数据");
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "相关数据已通过私聊发送！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("今日伤害"))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetTodayDamage(e.FromGroup.Id, e.FromQQ.Id));
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("查看总伤害 [CQ:at,qq=") && isAdmin(e) && !FileOptions.GetInstance().isDisableAt())
        {
            long qq = GetOperateQQ(e.Message.Text);
            e.CQApi.SendPrivateMessage(e.FromQQ.Id, GetRecentDaysDamages(e.FromGroup.Id, qq));
            //e.CQApi.SendPrivateMessage(qq, "[" + GuildBattle.GetUserName(e.FromGroup.Id, e.FromQQ.Id) + "] 查询了您的今日伤害数据");
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "相关数据已通过私聊发送！");
            e.Handler = true;
            return;
        }

        if (Regex.IsMatch(e.Message.Text, @"查看总伤害 (\d+)") && isAdmin(e))
        {
            Match match = Regex.Match(e.Message.Text, @"查看总伤害 (\d+)");
            long qq;
            if (!long.TryParse(match.Groups[1].Value, out qq)) return;
            e.CQApi.SendPrivateMessage(e.FromQQ.Id, GetRecentDaysDamages(e.FromGroup.Id, qq));
            e.CQApi.SendPrivateMessage(qq, "[" + GuildBattle.GetUserName(e.FromGroup.Id, e.FromQQ.Id) + "] 查询了您的今日伤害数据");
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "相关数据已通过私聊发送！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("查看总伤害"))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetRecentDaysDamages(e.FromGroup.Id, e.FromQQ.Id));
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("查看公会总伤害") && isAdmin(e))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetRecentDaysGuildTotalDamages(e.FromGroup.Id));
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("出刀统计") && isAdmin(e))
        {
            e.CQApi.SendGroupMessage(e.FromGroup, BattleStatistics(e));
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("查看留言板") || e.Message.Text.Equals("查看留言"))
        {
            string output = "【留言板】";
            Dictionary<long, string> messages = new Dictionary<long, string>(GuildBattle.GetInstance(e.FromGroup.Id).GetMessages()); //简单粗暴解决并发问题，有待优化
            if (messages == null || messages.Count == 0)
            {
                output += "\n无记录";
            }
            else
            {
                foreach (KeyValuePair<long, string> kvp in messages)
                {
                    output += "\n" + "[" + GuildBattle.GetUserName(e.FromGroup.Id, kvp.Key) + "] " + kvp.Value;
                }
            }
            e.CQApi.SendGroupMessage(e.FromGroup, output);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("留言 "))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string addMessage = Regex.Replace(temp[1], @"\[CQ[^\s\]]+\]", "");
            GuildBattle.GetInstance(e.FromGroup.Id).AddMessage(e.FromQQ.Id, addMessage.Trim());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "设置留言成功！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("删除留言") || e.Message.Text.Equals("取消留言"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveMessage(e.FromQQ.Id);
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "删除留言成功！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("清空留言") && isAdmin(e))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).ClearMessage();
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功清空所有留言！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("预约 "))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            int boss_num = 0;
            if (!int.TryParse(temp[1], out boss_num) || boss_num < 1 || boss_num > GuildBattle.BOSS_MAX)
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "参数错误！");
                e.Handler = true;
                return;
            }

            if (GuildBattle.GetInstance(e.FromGroup.Id).AddSubscribe(e.FromQQ.Id, boss_num))
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功为[CQ:at,qq=" + e.FromQQ.Id.ToString() + "] 预约第" + temp[1] + "号BOSS" + "\n" + "注：只能同时预约一个BOSS，多次预约的数据将被覆盖");
            } else
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "当前已经是第" + temp[1] + "号BOSS，预约无效");
            }

            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("取消预约"))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).RemoveSubscribe(e.FromQQ.Id);
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "取消成功！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("清空预约") && isAdmin(e))
        {
            GuildBattle.GetInstance(e.FromGroup.Id).ClearSubscribe();
            e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功清空所有预约！");
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("查看预约") || e.Message.Text.Equals("预约列表"))
        {
            List<string> list = GuildBattle.GetInstance(e.FromGroup.Id).GetSubscribeList();
            string text = "【预约列表】";
            for (int i = 0; i < list.Count; ++i)
            {
                text += "\n" + list[i];
            }
            if (list.Count == 0) text += "\n无记录";
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("BOSS状态"))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GuildBattle.GetInstance(e.FromGroup.Id).GetBossInfo());
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("申请SL") || (e.Message.Text.StartsWith("申请SL [CQ:at,qq=") && isAdmin(e)))
        {
            long qq = e.FromQQ.Id;
            if (e.Message.Text.StartsWith("申请SL [CQ:at,qq="))
            {
                qq = GetOperateQQ(e.Message.Text);
            }
            if (GuildBattle.GetInstance(e.FromGroup.Id).SetSL(qq))
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "今日SL次数使用状态已记录！\n[今日SL使用状态] 已使用\n[使用时间] " + SQLiteManager.ConvertIntDateTime(SQLiteManager.GetTimeStamp()));
            } else
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "今日SL次数已用完！指令无效！");
            }
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("撤销SL") || (e.Message.Text.StartsWith("撤销SL [CQ:at,qq=") && isAdmin(e)))
        {
            long qq = e.FromQQ.Id;
            if (e.Message.Text.StartsWith("撤销SL [CQ:at,qq="))
            {
                qq = GetOperateQQ(e.Message.Text);
            }
            if (GuildBattle.GetInstance(e.FromGroup.Id).RemoveSL(qq))
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "今日SL使用状态已成功撤销！\n[今日SL使用状态] 未使用\n[使用时间] 无");
            }
            else
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "今日SL次数仍未被使用！指令无效！");
            }
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("查询SL") || (e.Message.Text.StartsWith("查询SL [CQ:at,qq=") && isAdmin(e)))
        {
            long qq = e.FromQQ.Id;
            if (e.Message.Text.StartsWith("查询SL [CQ:at,qq="))
            {
                qq = GetOperateQQ(e.Message.Text);
            }
            long checkTime = GuildBattle.GetInstance(e.FromGroup.Id).GetSLStatus(qq);
            if (checkTime > -1)
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "[今日SL使用状态] 已使用\n[使用时间] " + SQLiteManager.ConvertIntDateTime(checkTime));
            }
            else
            {
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "[今日SL使用状态] 未使用\n[使用时间] 无");
            }
            e.Handler = true;
            return;
        }
    }

    public long GetOperateQQ(string input)
    {
        string exp = @"\[CQ:at,qq=(\d+)\]";
        Match match = Regex.Match(input, exp);
        long qq = long.Parse(match.Groups[1].Value);
        return qq;
    }

    public string GetRecentDaysDamages(long group, long qq)
    {
        Dictionary<long, long> dayDamages = new Dictionary<long, long>(SQLiteManager.GetInstance().GetRecentDaysDamages(group, qq, 10));
        string output = "[" + GuildBattle.GetUserName(group, qq) + "] 近期对BOSS造成的伤害：";
        if (dayDamages.Count == 0)
        {
            output += "\n无记录";
            return output;
        }

        long allDamage = 0;
        foreach (KeyValuePair<long, long> kvp in dayDamages)
        {
            output += "\n" + "[" + SQLiteManager.DayToDate(kvp.Key) + "]\t" + kvp.Value.ToString();
            allDamage += kvp.Value;
        }

        output += "\n" + "[近期总伤害]\t" + allDamage.ToString();
        return output;
    }

    public string GetRecentDaysGuildTotalDamages(long group)
    {
        Dictionary<long, long> dayDamages = new Dictionary<long, long>(SQLiteManager.GetInstance().GetRecentDaysGuildTotalDamages(group, 10));
        string output = "本公会近期对BOSS造成的伤害：";
        if (dayDamages.Count == 0)
        {
            output += "\n无记录";
            return output;
        }

        long allDamage = 0;
        foreach (KeyValuePair<long, long> kvp in dayDamages)
        {
            output += "\n" + "[" + SQLiteManager.DayToDate(kvp.Key) + "]\t" + kvp.Value.ToString();
            allDamage += kvp.Value;
        }

        output += "\n" + "[近期总伤害]\t" + allDamage.ToString();
        return output;
    }

    public string GetTodayDamage(long group, long qq)
    {
        List<string> temp = new List<string>();
        List<SQLiteManager.Damage> damages = SQLiteManager.GetInstance().GetTodayDamages(group, qq);
        string output = "[" + GuildBattle.GetUserName(group, qq) + "] 今日对BOSS造成的伤害：";
        if (damages.Count == 0)
        {
            output += "\n无记录";
            return output;
        }

        long allDamage = 0;
        for (int i = 0; i < damages.Count; ++i)
        {
            temp.Add(damages[i].troop.ToString() + "队伤害: " + damages[i].damage.ToString());
            allDamage += damages[i].damage;
        }
        temp.Sort();

        for (int i = 0; i < temp.Count; ++i) output += "\n" + temp[i];
        output += "\n[今日伤害总计] " + allDamage.ToString();
        return output;
    }

    public string BattleStatistics(CQGroupMessageEventArgs e)
    {
        string output = "【今日出刀状况】";

        List<SQLiteManager.Damage> damages = SQLiteManager.GetInstance().GetTodayDamages(e.FromGroup.Id);

        GroupMemberInfoCollection infos = e.CQApi.GetGroupMemberList(e.FromGroup);
        foreach (GroupMemberInfo info in infos)
        {
            if (GuildBattle.GetInstance(e.FromGroup.Id).GetMemberList() == null)
            {
                if (!info.Card.Contains(GuildBattle.GetSignChar())) continue;
            } else
            {
                if (!GuildBattle.GetInstance(e.FromGroup.Id).GetMemberList().Contains(info.QQ.Id)) continue;
            }
            SQLiteManager.Damage temp = new SQLiteManager.Damage();
            temp.user = info.QQ.Id;
            temp.damage = temp.troop = 0;
            if (damages.Contains(temp, SQLiteManager.DamageComparer.Default)) continue;
            damages.Add(temp);
        }

        for (int i = 0; i < damages.Count; ++i)
        {
            for (int j = i + 1; j < damages.Count; ++j)
            {
                if (damages[i].troop < damages[j].troop || (damages[i].troop == damages[j].troop && damages[i].damage < damages[j].damage))
                {
                    SQLiteManager.Damage temp = damages[i];
                    damages[i] = damages[j];
                    damages[j] = temp;
                }
            }
        }

        long totalTroop = 0;
        long totalDamage = 0;
        for (int i = 0; i < damages.Count; ++i)
        {
            totalTroop += damages[i].troop;
            totalDamage += damages[i].damage;
            output += "\n" + GuildBattle.GetUserName(e.FromGroup.Id, damages[i].user) + "\t\t" + damages[i].troop.ToString() + "刀\t伤害: " + damages[i].damage.ToString();
        }
        output += "\n【已进行战斗次数(含补刀)】 " + totalTroop.ToString() + " 次";
        output += "\n【本日总伤害】 " + totalDamage.ToString();
        return output;
    }

    private bool isAdmin(CQGroupMessageEventArgs e)
    {
        return GuildBattle.GetInstance(e.FromGroup.Id).isAdmin(e.FromQQ.Id);
    }
}