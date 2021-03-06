<?php 
require('functions.php'); 

if (isset($_GET['offset'])) $dayOffset = intval($_GET['offset']);
else $dayOffset = 0;
?><!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <!-- <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no"> -->
    <title>BossStatisticView</title>
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
        <h2 style="text-align: center; margin-bottom: 2%">公会战当日BOSS伤害贡献统计(<?php echo date('m/d', time() + $dayOffset * 3600 * 24); ?>)</h2>
        <?php ShowPageButtons(8, 'boss'); ?>
		<b><?php PrintGroupRank(); ?></b>
        <table id="stat" class="table table-hover table-striped tablesorter" data-sort-name="Total" data-sort-order="desc">
            <?php echo PrintTable(16); ?>
        </table>
        <?php echo PrintCopyRight(); ?>
    </div>
</body>

</html>

<?php
function PrintTable($offset)
{
    global $user, $SQLite, $dayOffset;
    $day = GetDay(date('Y-m-d')); //date('Y-m-d')
	if (isset($dayOffset)) $day += $dayOffset;
    $output = "";

    $sql = 'SELECT id, user, SUM(damage) AS damage, frequency, boss_num FROM Damage WHERE day = ' . $day . ' AND group_number = ' . GROUP_ID . ' GROUP BY frequency, boss_num, user;';

    $ret = $SQLite->query($sql);
    # 统计数据
    $bossDamage = array();
    $dataList = array();
    $userList = array();

    while ($row = $ret->fetchArray(SQLITE3_ASSOC)) {
        $key = intval($row['frequency']) * 10 + intval($row['boss_num']);
        if (!array_key_exists($key, $dataList)) $dataList[$key] = array();
        if (!array_key_exists($key, $bossDamage)) $bossDamage[$key] = 0;
        $bossDamage[$key] += $row['damage'];
        $dataList[$key][strval($row['user'])] = $row['damage'];
        if (!in_array(strval($row['user']), $userList)) $userList[] = strval($row['user']);
    }

    # 表头部分
    $output .= "<thead>";
    $output .= "\n" . GetChar($offset + 4) . "<tr>";
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="Name">Name<img src="img/sort.png" /></th>';
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="QQ">QQ<img src="img/sort.png" /></th>';
    $dataField = array_keys($dataList);
    for ($i = 0; $i < count($dataField); ++$i) {
        if ($output != "") $output .= "\n" . GetChar($offset + 8);
        $word = "第" . intval($dataField[$i] / 10) . "周目B" . intval($dataField[$i] % 10);
        $output .= '<th data-field="' . $word . '">' . $word . '<img src="img/sort.png" /></th>';
    }
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="Total" data-sortable="true">Total<img src="img/sort.png" /></th>';
    $output .= "\n" . GetChar($offset + 4) . '</tr>';
    $output .= "\n" . GetChar($offset) . '</thead>';

    # 数据部分
    $output .= "\n" . GetChar($offset) . '<tbody>';
    foreach ($userList as $userTemp) {
        $userTotalDamage = 0;
        $output .= "\n" . GetChar($offset + 4) . '<tr>';
        $output .= "\n" . GetChar($offset + 8) . '<td>' . GetUserName($userTemp) . '</td>';
        $output .= "\n" . GetChar($offset + 8) . '<td>' . substr_replace($userTemp, '****', 3, 4) . '</td>';
        foreach ($dataList as $dataTemp) {
            if ($output != "") $output .= "\n" . GetChar($offset + 8);

            if (array_key_exists($userTemp, $dataTemp)) {
                $userTotalDamage += $dataTemp[$userTemp];
                $output .= '<td>' . $dataTemp[$userTemp] . '</td>';
            } else {
                $output .= '<td></td>';
            }
        }
        $output .= "\n" . GetChar($offset + 8) . '<td>' . $userTotalDamage . '</td>';
        $output .= "\n" . GetChar($offset + 4) . '</tr>';
    }


    $output .= "\n" . GetChar($offset + 8) . '<td><b>合计</b></td>';
    $output .= "\n" . GetChar($offset + 8) . '<td></td>';
    foreach ($bossDamage as $damageTemp) {
        $output .= "\n" . GetChar($offset + 8) . '<td><b>' . $damageTemp . '</b></td>';
    }
	$output .= "\n" . GetChar($offset + 8) . '<td><b>' . array_sum($bossDamage) . '</b></td>';
    $output .= "\n" . GetChar($offset + 4) . "</tr>";

    $output .= "\n" . GetChar($offset) . '</tbody>';
    return $output . "\n";
}
?>