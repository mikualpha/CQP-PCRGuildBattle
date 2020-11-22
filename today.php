<?php 
require('functions.php'); 
define('MAX_TROOP_NUM', 5);

// ini_set("display_errors","On");
// error_reporting(E_ALL);

if (isset($_GET['offset'])) $dayOffset = intval($_GET['offset']);
else $dayOffset = 0;

$day = GetDay(date('Y-m-d')); //date('Y-m-d')
$day += $dayOffset;
$maxTroopNum = GetMaxTroopNum($day);
?><!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <!-- <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no"> -->
    <title>TodayStatisticView</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/css/bootstrap.min.css" integrity="sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh" crossorigin="anonymous">
    <link rel="stylesheet" href="node_modules/tablesorter/dist/css/filter.formatter.min.css">
    <script src="node_modules/jquery/dist/jquery.min.js"></script>
    <script src="node_modules/tablesorter/dist/js/jquery.tablesorter.min.js"></script>
    <script src="node_modules/tablesorter/dist/js/jquery.tablesorter.widgets.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js" integrity="sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/js/bootstrap.min.js" integrity="sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6" crossorigin="anonymous"></script>
</head>

<body>
    <?php require('style.php') ?>
    <div id="container" class="container-pc">
        <h2 style="text-align: center; margin-bottom: 2%">公会战当日伤害统计(<?php echo date('m/d', time() + $dayOffset * 3600 * 24); ?>)</h2>
		<b><?php PrintGroupRank(); ?></b>
        <table id="stat" class="table table-hover table-striped tablesorter" data-sort-name="Total" data-sort-order="desc">
            <thead>
                <tr>
                    <th data-field="Name">Name<img src="img/sort.png" /></th>
                    <th data-field="QQ">QQ<img src="img/sort.png" /></th>
                    <?php echo SetHeader(20); ?>
                    <th data-field="Total" data-sortable="true">Total<img src="img/sort.png" /></th>
                </tr>
            </thead>
            <tbody>
                <?php echo PrintTable(16); ?>
            </tbody>
        </table>
    </div>
</body>

</html>

<?php
function PrintTable($offset)
{
    global $SQLite, $day, $maxTroopNum;
    $output = "";

    $helpTroopList = GetHelpTroopList();

    $sql = 'SELECT';
    $sql .= "\n" . '	user,';
    for ($i = 0; $i < $maxTroopNum; ++$i) $sql .= "\n" . '	SUM( CASE troop WHEN ' . ($i + 1) . ' THEN id ELSE 0 END ) AS ID' . ($i + 1) . ',';
    for ($i = 0; $i < $maxTroopNum; ++$i) $sql .= "\n" . '	SUM( CASE troop WHEN ' . ($i + 1) . ' THEN damage ELSE 0 END ) AS Damage' . ($i + 1) . ',';
    for ($i = 0; $i < $maxTroopNum; ++$i) $sql .= "\n" . '	SUM( CASE troop WHEN ' . ($i + 1) . ' THEN frequency ELSE 0 END ) AS Frequency' . ($i + 1) . ',';
    for ($i = 0; $i < $maxTroopNum; ++$i) $sql .= "\n" . '	SUM( CASE troop WHEN ' . ($i + 1) . ' THEN boss_num ELSE 0 END ) AS BossNum' . ($i + 1) . ',';
    $sql .= "\n" . '	SUM( damage ) AS Total';
    $sql .= "\n" . 'FROM';
    $sql .= "\n" . '	Damage';
    $sql .= "\n" . 'WHERE';
    $sql .= "\n" . '	group_number = ' . GROUP_ID . ' AND day = ' . $day;
    $sql .= "\n" . 'GROUP BY';
    $sql .= "\n" . '	user';
    $sql .= "\n" . 'ORDER BY';
    $sql .= "\n" . '	Total DESC;';

    $ret = $SQLite->query($sql);
	$totalDamage = 0;
	$totalCount = 0;
    while ($row = $ret->fetchArray(SQLITE3_ASSOC)) {
        if ($output != "") $output .= "\n" . GetChar($offset);
        $output .= "<tr>";
        $output .= "\n" . GetChar($offset + 4) . '<td>' . GetUserName($row['user']) . '</td>';
        $output .= "\n" . GetChar($offset + 4) . '<td>' . substr_replace($row['user'], '****', 3, 4) . '</td>';
        for ($i = 0; $i < count($row) - 2 - $maxTroopNum * 3; ++$i) {
			if (intval($row['Damage' . ($i + 1)]) > 0) ++$totalCount;
            $output .= "\n" . GetChar($offset + 4) . '<td title="第' . $row['Frequency' . ($i + 1)] . '周目 ' . $row['BossNum' . ($i + 1)] . '号BOSS">' . $row['Damage' . ($i + 1)];
            // 代刀记录
            if (array_key_exists($row['ID' . ($i + 1)], $helpTroopList)) {
                $output .= '<span style="color:red" title="代刀: ' . GetUserName($helpTroopList[$row['ID' . ($i + 1)]]) . '"><b>*</b></span>';
            }
            $output .= '</td>';
		}
        $output .= "\n" . GetChar($offset + 4) . '<td>' . $row['Total'] . '</td>';
        $output .= "\n" . GetChar($offset) . "</tr>";
		$totalDamage += intval($row['Total']);
    }
	$output .= "<tr>";
    $output .= "\n" . GetChar($offset + 4) . '<td><b>合计</b></td>';
    $output .= "\n" . GetChar($offset + 4) . '<td><b>共' . $totalCount . '刀</b></td>';
    for ($i = 0; $i < $maxTroopNum; ++$i) {
        $output .= "\n" . GetChar($offset + 4) . '<td></td>';
    }
	$output .= "\n" . GetChar($offset + 4) . '<td><b>' . $totalDamage . '</b></td>';
    $output .= "\n" . GetChar($offset) . "</tr>";
    return $output . "\n";
}

function GetHelpTroopList() {
    global $SQLite, $day;
    $sql = 'SELECT id, troop_operator FROM Damage WHERE troop_operator > 0 AND day = ' . $day . ' AND group_number = ' . GROUP_ID;
    $ret = $SQLite->query($sql);
    $output = [];
    while ($row = $ret->fetchArray(SQLITE3_ASSOC)) {
        if (isset($row['troop_operator']) && $row['troop_operator'] > 0) $output[$row['id']] = $row['troop_operator'];
    }
    return $output;
}

function SetHeader($offset)
{
    global $maxTroopNum;
    $output = "";
    for ($i = 0; $i < $maxTroopNum; ++$i) {
        if ($output != "") $output .= "\n" . GetChar($offset);
        $output .= '<th data-field="Troop' . ($i + 1) . '" data-sortable="true">伤害' . ($i + 1) . '<img src="img/sort.png" /></th>';
    }
    return $output . "\n";
}

function GetMaxTroopNum($day) {
    global $SQLite;
    $sql = 'SELECT MAX(troop) AS max_troop FROM Damage WHERE day = ' . $day . ' AND group_number = ' . GROUP_ID;
    $ret = $SQLite->query($sql);
    $row = $ret->fetchArray(SQLITE3_ASSOC);
    return (isset($row['max_troop']) ? intval($row['max_troop']) : 0);
}
?>