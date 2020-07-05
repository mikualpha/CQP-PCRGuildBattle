﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class FileOptions
{
    private static FileOptions ins = null;
    protected string dir = null;
    protected string path = null;
    private Dictionary<string, string> fileOptions;

    private FileOptions()
    {
        dir = ApiModel.CQApi.AppDirectory;
        path = dir + "Config.ini";
        initalizeFile();
        initalizeOptions();
    }

    public static FileOptions GetInstance()
    {
        if (ins == null) ins = new FileOptions();
        return ins;
    }

    public Dictionary<string, string> GetOptions() { return fileOptions; }

    public bool isDisableAt()
    {
        return fileOptions["DisableAt"] == "1";
    }

    private void initalizeOptions()
    {
        fileOptions = new Dictionary<string, string>();
        //fileOptions["Group"] = "0";
        fileOptions["Admin"] = "0";
        fileOptions["TreeAdmin"] = "0";
        fileOptions["Member"] = "0";
        fileOptions["MemberChar"] = "";
        fileOptions["DisableAt"] = "0";
        ReadFromFile(path);
    }

    private bool initalizeFile()
    {
        if (File.Exists(path)) return false;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter writer = new StreamWriter(fs);
        writer.Write("//请仅修改等号后部分，其余部分修改可能会出现问题！\r\n" +
                    "//管理员列表\r\n" +
                    "Admin=123456789,987654321\r\n" +
                    "//挂树通知列表\r\n" +
                    "TreeAdmin=123456789,987654321\r\n" +
                    "//成员标识符(与成员列表二选一即可)\r\n" +
                    "MemberChar=*\r\n" +
                    "//成员列表，半角逗号分隔，标0为启用标识符模式\r\n" +
                    "Member=0\r\n" +
                    "//是否禁止管理员使用@方式获取数据，1为禁止\r\n" +
                    "DisableAt=0");
        writer.Close();
        fs.Close();
        return true;
    }

    protected void ReadFromFile(string _path)
    {
        if (!File.Exists(_path)) initalizeFile();

        using (StreamReader sr = new StreamReader(_path))
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("//")) continue;
                string[] temp = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                fileOptions[temp[0].Trim()] = temp[1].Trim();
            }
        }
    }
}

