﻿using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
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

        if (e.Message.Text.Contains("#启用公会战工具") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            if (GuildBattle.GetInstance().GetGroup() == e.FromGroup.Id) e.CQApi.SendGroupMessage(e.FromGroup.Id, "工具已启用，该指令无效！");
            else if (GuildBattle.GetInstance().GetGroup() != -1) e.CQApi.SendGroupMessage(e.FromGroup.Id, "已在其它群启用工具，请先禁用！");
            else
            {
                GuildBattle.GetInstance().SetGroup(e.FromGroup.Id);
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功启用！");
            }
            e.Handler = true;
            return;
        }

        if (e.FromGroup.Id != GuildBattle.GetInstance().GetGroup())
        {
            e.Handler = false;
            return;
        }

        if (e.Message.Text.Contains("#禁用公会战工具") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            if (GuildBattle.GetInstance().GetGroup() == -1) e.CQApi.SendGroupMessage(e.FromGroup.Id, "工具已禁用，该指令无效！");
            else
            {
                GuildBattle.GetInstance().SetGroup(-1);
                e.CQApi.SendGroupMessage(e.FromGroup.Id, "已成功禁用！");
            }
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("公会战状态"))
        {
            List<string> logs = SQLiteManager.GetInstance().GetLogs();
            string text = "【日志记录】";

            for (int i = 0; i < logs.Count; ++i) text += "\n" + logs[i];

            text += "\n\n" + "【战斗列表】" + "\n" + GuildBattle.PrintList(GuildBattle.GetInstance().GetBattleUser());
            text += "\n\n" + "【挂树列表】" + "\n" + GuildBattle.PrintList(GuildBattle.GetInstance().GetTreeUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("出刀列表"))
        {
            string text = "【战斗列表】" + "\n" + GuildBattle.PrintList(GuildBattle.GetInstance().GetBattleUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("挂树列表"))
        {
            string text = "【挂树列表】" + "\n" + GuildBattle.PrintList(GuildBattle.GetInstance().GetTreeUser());
            e.CQApi.SendGroupMessage(e.FromGroup.Id, text);
            e.Handler = true;
            return;
        }
        

        if (e.Message.Text.Equals("我挂树了") || e.Message.Text.Equals("救救救"))
        {
            GuildBattle.GetInstance().AddTreeUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Contains("我去去就来") || e.Message.Text.Equals("申请出刀"))
        {
            GuildBattle.GetInstance().AddBattleUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.StartsWith("取消挂树 [CQ:at,qq=") || e.Message.Text.StartsWith("取消挂树[CQ:at,qq=")) && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            long qq = GetOperateQQ(e.Message.Text);
            GuildBattle.GetInstance().RemoveTreeUser(qq);
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.StartsWith("取消出刀 [CQ:at,qq=") || e.Message.Text.StartsWith("取消出刀[CQ:at,qq=")) && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            long qq = GetOperateQQ(e.Message.Text);
            GuildBattle.GetInstance().RemoveBattleUser(qq);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("取消挂树"))
        {
            GuildBattle.GetInstance().RemoveTreeUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("取消出刀"))
        {
            GuildBattle.GetInstance().RemoveBattleUser(e.FromQQ.Id);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("伤害 [CQ:at,qq=") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            long qq = GetOperateQQ(e.Message.Text);

            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 4)
            {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            int troop_num;
            long damage;
            if (!int.TryParse(temp[2], out troop_num))
            {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }
            if (!long.TryParse(temp[3], out damage))
            {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            GuildBattle.GetInstance().PushDamage(qq, troop_num, damage);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("伤害 "))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 3)
            {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            int troop_num;
            long damage;
            if (!int.TryParse(temp[1], out troop_num)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }
            if (!long.TryParse(temp[2], out damage)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            GuildBattle.GetInstance().PushDamage(e.FromQQ, troop_num, damage);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("设置BOSS ") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 3) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }

            int frequency, boss_num;

            if (!int.TryParse(temp[1], out frequency)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }
            if (!int.TryParse(temp[2], out boss_num)) {
                e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
                e.Handler = true;
                return;
            }
            GuildBattle.GetInstance().SetFrequency(frequency, boss_num);
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("设置血量 ") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            string[] temp = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length != 2) e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
            long blood;
            if (!long.TryParse(temp[1], out blood)) e.CQApi.SendGroupMessage(e.FromGroup, "输入格式与要求不符！");
            GuildBattle.GetInstance().SetDamage(blood);
            e.Handler = true;
            return;
        }


        if (e.Message.Text.Equals("清空出刀") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            GuildBattle.GetInstance().ClearBattleUser();
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("清空挂树") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            GuildBattle.GetInstance().ClearTreeUser();
            e.Handler = true;
            return;
        }

        if (e.Message.Text.StartsWith("今日伤害 [CQ:at,qq=") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            long qq = GetOperateQQ(e.Message.Text);
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetTodayDamage(qq));
            e.Handler = true;
            return;
        }

        if (Regex.IsMatch(e.Message.Text, @"今日伤害 (\d+)") && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            Match match = Regex.Match(e.Message.Text, @"今日伤害 (\d+)");
            long qq = long.Parse(match.Groups[1].Value);
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetTodayDamage(qq));
            e.Handler = true;
            return;
        }

        if (e.Message.Text.Equals("今日伤害"))
        {
            e.CQApi.SendGroupMessage(e.FromGroup.Id, GetTodayDamage(e.FromQQ.Id));
            e.Handler = true;
            return;
        }

        if ((e.Message.Text.Equals("出刀统计") || e.Message.Text.Equals("出刀警察")) && GuildBattle.GetInstance().isAdmin(e.FromQQ.Id))
        {
            e.CQApi.SendGroupMessage(e.FromGroup, BattleStatistics(e));
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

    public string GetTodayDamage(long qq)
    {
        List<string> temp = new List<string>();
        List<SQLiteManager.Damage> damages = SQLiteManager.GetInstance().GetTodayDamages(qq);
        string output = "[" + GuildBattle.GetUserName(qq) + "] 今日对BOSS造成的伤害：";
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

        List<SQLiteManager.Damage> temp = SQLiteManager.GetInstance().GetTodayDamages();
        List<long> users = new List<long>();
        List<int> troops = new List<int>();
        List<long> damages = new List<long>();

        GroupMemberInfoCollection infos = e.CQApi.GetGroupMemberList(e.FromGroup);
        foreach (GroupMemberInfo info in infos)
        {
            if (!info.Card.Contains("*")) continue;
            users.Add(info.QQ.Id);
            troops.Add(0);
            damages.Add(0);
        }

        for (int i = 0; i < temp.Count; ++i)
        {
            if (users.Contains(temp[i].user))
            {
                int index = users.IndexOf(temp[i].user);
                troops[index]++;
                damages[index] += temp[i].damage;
            }
        }

        for (int i = 0; i < users.Count; ++i)
        {
            for (int j = i + 1; j < users.Count; ++j)
            {
                if (troops[i] < troops[j])
                {
                    long swap = users[i];
                    users[i] = users[j];
                    users[j] = swap;

                    swap = damages[i];
                    damages[i] = damages[j];
                    damages[j] = swap;

                    swap = troops[i];
                    troops[i] = troops[j];
                    troops[j] = (int)swap;
                }
            }
        }

        for (int i = 0; i < users.Count; ++i)
        {
            output += "\n" + GuildBattle.GetUserName(users[i]) + "\t\t" + troops[i].ToString() + "刀\t伤害: " + damages[i].ToString();
        }
        output += "\n【已进行战斗次数(含补刀)】 " + temp.Count.ToString() + " 次";
        return output;
    }
}