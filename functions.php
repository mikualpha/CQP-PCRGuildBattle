<?php
include 'settings.php';
ini_set('date.timezone', "Europe/Helsinki");

$SQLite = new SQLite3(DATABASE_PATH);

$picDir = 'img/back/';
$user = array();
ReadUser();

function ReadLineFromFile($file)
{
    $output = [];
    if (!is_file($file)) return $output;
    $handle = @fopen($file, "r");
    if ($handle) {
        while (!feof($handle)) {
            $buffer = trim(fgets($handle));
            $temp = explode("\t", $buffer);
            $output[$temp[1]] = $temp[0];
        }
    }
    return $output;
}

function ReadUser()
{
    global $user;
    $user = ReadLineFromFile('member.ini');
}

function GetUserName($qq)
{
    global $user;
    if (!array_key_exists($qq, $user)) return substr_replace(strval($qq), '****', 3, 4);
    return $user[$qq];
}

function GetDay($dateStr = null)
{
    if (!isset($dateStr)) $dateStr = date('Y-m-d');
    $start = date_create('1970-01-01');
    $now = date_create($dateStr);
    return intval(date_diff($start, $now)->format("%a"));
}

function InGuildBattle() {
    $dayOffset = GetDay() - GetDay(START_DATE);
    return ($dayOffset <= DAY_NUM && $dayOffset >= 0);
}

function GetGuildBattleID() {
    $start = date_create('2020-06-' . date('d'));
    $now = date_create(date('Y-m-d'));
    return intval(date_diff($start, $now)->format("%m"));
}

function Day2Date($day)
{
    $start = date_create('1970-01-01');
    date_add($start, date_interval_create_from_date_string($day . " days"));
    return date_format($start, "m/d");
}

function GetChar($offset)
{
    return str_repeat(" ", $offset);
}

function GetFileList($dir) {
    $handler = opendir($dir);  
    while (($filename = readdir($handler)) !== false) {
        if ($filename !== "." && $filename !== "..") $files[] = $filename ;  
    }  
    closedir($handler);
    return $files;
}

function RandomPic() {
    global $picDir;
    $list = GetFileList($picDir);
    return $list[rand(0, count($list) - 1)];
}

function GetOwnRank() {
	return GetRank(GROUP_NAME);
}

function PrintGroupRank() {
    if (!defined('COOKIE') || COOKIE == '') return;
    if (!InGuildBattle()) return;
    $json = json_decode(GetOwnRank(), true)['data'];
    echo '<span style="margin-right: 1%;">' . "公会名: " . $json['clan_name'] . '</span>';
    echo '<span style="margin-right: 1%;">' . "当前排名: " . $json['rank'] . '</span>';
    echo '<span style="margin-right: 1%;">' . "总分: " . $json['damage'] . '</span>';
    echo '<span style="margin-right: 1%;">' . "与上一名分差: " . $json['delta_damage'] . '</span>';

    if (GetDay() != GetDay(START_DATE)) {
        echo '<span style="margin-right: 1%;">' . "今日05:00AM排名: " . GetDayRank(date('Y'), intval(date('m')), intval(date('d')), 5, 0, GetGuildBattleID(), DAY_RANK_START_PAGE) . '</span>';
        echo '<span style="margin-right: 1%;">' . "今日05:30AM排名: " . GetDayRank(date('Y'), intval(date('m')), intval(date('d')), 5, 30, GetGuildBattleID(), DAY_RANK_START_PAGE) . '</span>';
    }
}

function GetRank($name) {
	// Generated by curl-to-PHP: http://incarnate.github.io/curl-to-php/
	$ch = curl_init();

	curl_setopt($ch, CURLOPT_URL, 'https://www.bigfun.cn/api/feweb?target=gzlj-search-clan%2Fa&name=' . urlencode($name));
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'GET');
	curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false); //不验证证书
	curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false); //不验证证书
	curl_setopt($ch, CURLOPT_ENCODING, 'gzip, deflate');

	$headers = array();
	$headers[] = 'Connection: keep-alive';
	$headers[] = 'Accept: application/json, text/plain, */*';
	$headers[] = 'Dnt: 1';
	$headers[] = 'X-Csrf-Token: 8mYIqp8w-TGdEQUakJGuAhivMo4PHGi_o8ek';
	$headers[] = 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36';
	$headers[] = 'Referer: https://www.bigfun.cn/tools/pcrteam/search';
	$headers[] = 'Accept-Language: zh-CN,zh;q=0.9,en;q=0.8';
	$headers[] = 'Cookie: ' . COOKIE;
	curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);

    $result = curl_exec($ch);
	if (curl_errno($ch)) {
		echo 'Error:' . curl_error($ch);
	}
	curl_close($ch);
	return $result;
}

function GetDayRank($year, $month, $day, $hour, $minute, $battle_id, $start_page = 0) {
	$clan_name = GROUP_NAME;
	
    // Redis 获取
    static $redis = null;
    if (!isset($redis)) {
        $redis = new Redis();
        $redis->connect('127.0.0.1', 6379);
    }

    $redisKeyName = 'PCR-ClanRank-' . md5($clan_name) . '-' . $hour . $minute;
    if ($redis->exists($redisKeyName)) {
        return $redis->get($redisKeyName);
    }

    $ch = curl_init();
	
	$url = "https://www.bigfun.cn/api/feweb?target=gzlj-clan-battle-ranking%2Fa&page=$start_page&year=$year&month=$month&day=$day&hour=$hour&minute=$minute&battle_id=$battle_id";
    curl_setopt($ch, CURLOPT_URL, $url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
    curl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'GET');
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false); //不验证证书
	curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false); //不验证证书
    curl_setopt($ch, CURLOPT_ENCODING, 'gzip, deflate');
    
    $headers = array();
    $headers[] = 'Connection: keep-alive';
    $headers[] = 'Accept: application/json, text/plain, */*';
    $headers[] = 'Dnt: 1';
    $headers[] = 'X-Csrf-Token: 43Sw93pG-4hMhDAuznYLJvHsTWHhgR8rH3yc';
    $headers[] = 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36';
    $headers[] = 'Sec-Fetch-Site: same-origin';
    $headers[] = 'Sec-Fetch-Mode: cors';
    $headers[] = 'Sec-Fetch-Dest: empty';
    $headers[] = 'Accept-Language: zh-CN,zh;q=0.9,en;q=0.8';
    $headers[] = 'Cookie: ' . COOKIE;
    curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
    
    $result = curl_exec($ch);
    if (curl_errno($ch)) {
        echo 'Error:' . curl_error($ch);
    }
    curl_close($ch);
    
    if (strpos($result, $clan_name)) {
        $json = json_decode($result, true);
        for ($i = 0; $i < count($json['data']); ++$i) {
            if ($json['data'][$i]['clan_name'] == $clan_name) {
                $redis->set($redisKeyName, $json['data'][$i]['rank']);
                if (intval(date("G") == $hour)) 
                    $expireTime = time() + 3600;
                else 
                    $expireTime = mktime($hour - 5, $minute, 0, date('m'), date('d') + 1, date('Y'));
                $redis->expireAt($redisKeyName, $expireTime);

                return $json['data'][$i]['rank'];
            }
        }
    } else {
		if (strlen($result) < 1000) die('Request Error: ' . $url . $result);
        if ($start_page >= DAY_RANK_MAX_PAGE) die('Max page limit.');
        return GetDayRank($year, $month, $day, $hour, $minute, $battle_id, $start_page + 1);
    }
}

function PrintCopyRight() {
    echo '<div style="text-align: center; font-size: 10pt">Copyright © 2020-' . date('Y') . ' MikuAlpha All Rights Reserved.</div>';
}
?>