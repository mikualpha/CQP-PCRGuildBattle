# CQP-PCRGuildBattle
公主连接Re:Dive 公会战统计机器人

因CoolQ已经关闭，可采用[MiraiNative框架](https://github.com/iTXTech/mirai-native)使用本插件。

Gitter聊天室：[![Gitter](https://badges.gitter.im/MikuAlphaBot/community.svg)](https://gitter.im/MikuAlphaBot/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

### 使用方式
使用前请修改```data/MiraiNative/data/cn.mikualpha.pcr.guild/Config.ini```以设置管理员，并重启。

一般情况下，用户群名片带有\*号即可识别为公会成员。(亦可在```Config.ini```中修改为其它符号或直接更改为使用账号列表)

**具体群内指令详情可参考[指令列表](https://docs.qq.com/sheet/DYXBDZ1RmRXdXR0dH?tab=BB08J2)。**

### 附加组件
![示例图](https://github.com/mikualpha/CQP-PCRGuildBattle/raw/master/preview.png)

为本插件开发了一套Web网页模块，可以在[此分支](https://github.com/mikualpha/CQP-PCRGuildBattle/tree/web)中查看。(默认背景图取自[Pixiv](https://www.pixiv.net/artworks/80851092)，在此感谢)

### 使用组件

[C# Native SDK](https://github.com/Jie2GG/Native.Csharp.Frame/) ([MIT License](https://github.com/Jie2GG/Native.Csharp.Frame/blob/Final/LICENSE))

[SQLite-Net](https://github.com/praeclarum/sqlite-net) ([MIT License](https://archive.codeplex.com/?p=sqlitepcl))

[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) ([MIT License](https://raw.githubusercontent.com/JamesNK/Newtonsoft.Json/master/LICENSE.md))
