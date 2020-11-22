# CQP-PCRGuildBattle Web模块
公主连接Re:Dive 公会战统计机器人 Web面板模块

基于PHP编写和SQLite读取的Web面板(因为起初是自用型所以代码比较乱，见谅)。

### 环境需求
- Windows Server(因为mirai-native仅支持Windows，如果有共享SQLite读写的方式也可以在Linux上运行)
- Nginx / Apache
- PHP 7.0+
- Redis

Nginx + PHP环境如果安装比较困难可以借助WNMP一键包完成。

Redis安装可以参考[这个Repository](https://github.com/tporadowski/redis/releases)。

### 使用方式
将代码文件放置至Nginx的网站目录下，将`settings-sample.php`更名为`settings.php`，并调整其中参数即可。(对参数有疑问可以提issue询问)

另需要将`member-sample.ini`更名为`member.ini`, 并添加成员信息，以在页面中显示成员昵称。

目前功能页面分为
```
/index.php 日伤害页面
/today.php 今日伤害页面(添加?offset=-1可查看前一天数据，以此类推)
/boss.php 今日BOSS输出贡献页面(添加?offset=-1可查看前一天数据，以此类推)
```

### 使用组件
[JQuery](https://github.com/jquery/jquery) ([MIT License](https://github.com/jquery/jquery/blob/master/LICENSE.txt))

[TableSort](https://github.com/Mottie/tablesorter) ([MIT License](https://github.com/Mottie/tablesorter/blob/master/README.md))
