<?php
##### 请把本文件重命名为settings.php后使用 #####
##### 还需要把member-sample.ini重命名为member.ini并且添加会员信息 #####

# 公会战开始日期
define('START_DATE', '2020-11-17');
# 公会战持续天数
define('DAY_NUM', 6);
# 公会名(公会战期间不能随意修改)
define('GROUP_NAME', 'ABC');
# 公会群群号
define('GROUP_ID', 123456789);
# Mirai数据库路径
define('DATABASE_PATH', 'C:\\Mirai\\123456789\\data\\MiraiNative\\data\\cn.mikualpha.pcr.guild\\SQLite.db');
# 5点排名起始检测页(一页25名, 输入公会理论排名上限除以25即可)
define('DAY_RANK_START_PAGE', 8);
# 5点排名最大检测页(一页25名, 输入公会理论排名下限除以25即可)
define('DAY_RANK_MAX_PAGE', 60);
# BIGFUN-COOKIE
define('COOKIE', '');
