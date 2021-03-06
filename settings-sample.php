<?php
##### 请把本文件复制并重命名为settings.php后使用 #####
##### 还需要把member-sample.ini重命名为member.ini并且添加会员信息 #####

# 公会战开始日期
define('START_DATE', '2020-11-17');
# 公会战持续天数
define('DAY_NUM', 6);
# 公会名(公会战期间该项值必须与实际公会名一致)
define('GROUP_NAME', 'ABC');
# 公会群群号
define('GROUP_ID', 123456789);
# Mirai路径(以/结尾)
define('MIRAI_PATH', 'C:/Mirai/');
# 5点排名起始检测页(一页25名, 输入公会理论排名上限除以25即可)
define('DAY_RANK_START_PAGE', 8);
# 5点排名最大检测页(一页25名, 输入公会理论排名下限除以25即可)
define('DAY_RANK_MAX_PAGE', 60);
# BIGFUN-COOKIE
define('COOKIE', '');
# DEBUG模式
define('DEBUG_MODE', false);


# --------- 高级变量 --------- #
# Mirai数据路径
define('DATA_PATH', MIRAI_PATH . 'data/MiraiNative/data/cn.mikualpha.pcr.guild/');
# SQLite文件名
define('SQLITE_FILENAME', 'SQLite.db');
# Config文件名
define('INI_FILENAME', 'Data-' . GROUP_ID . '.ini');
