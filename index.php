<?php 
require('functions.php');

$day = GetDay(START_DATE);

// ini_set("display_errors","On");
// error_reporting(E_ALL);
?>
<!DOCTYPE html>
<html>

<head>
    <meta charset="utf-8" />
    <!-- <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no"> -->
    <title>StatisticView</title>
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
        <h2 style="text-align: center; margin-bottom: 2%">公会战伤害统计</h2>
		<b><?php PrintGroupRank(); ?></b>
        <table id="stat" class="table table-hover table-striped tablesorter" data-sort-name="Total" data-sort-order="desc">
            <thead>
                <tr>
                    <th data-field="Name">Name<img src="img/sort.png" /></th>
                    <th data-field="QQ">QQ<img src="img/sort.png" /></th>
                    <?php echo SetHeader(20); ?>
                    <th data-field="Total" data-sortable="true">Total<img src="img/sort.png" /></th>
                    <th data-field="helpday" data-sortable="true">全代刀天数<img src="img/sort.png" /></th>
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
    global $day, $SQLite;
    $output = "";

    $helpTroopList = GetHelpTroopList();

    $sql = 'SELECT';
    $sql .= "\n" . '	user,';
    for ($i = 0; $i < DAY_NUM; ++$i) $sql .= "\n" . '	SUM( CASE day WHEN ' . ($day + $i) . ' THEN damage ELSE 0 END ) AS Day' . ($i + 1) . ',';
    $sql .= "\n" . '	SUM( damage ) AS Total';
    $sql .= "\n" . 'FROM';
    $sql .= "\n" . '	Damage ';
    $sql .= "\n" . 'WHERE';
    $sql .= "\n" . '	group_number = ' . GROUP_ID . ' AND day >= ' . $day;
    $sql .= "\n" . 'GROUP BY';
    $sql .= "\n" . '	user;';

    $length = 0;
    $totalDamage = [];
    $ret = $SQLite->query($sql);
    while ($row = $ret->fetchArray(SQLITE3_ASSOC)) {
        if ($output != "") $output .= "\n" . GetChar($offset);
        if ($length == 0) {
            $length = DAY_NUM;
            for ($i = 0; $i < $length + 1; ++$i) $totalDamage[$i] = 0;
        }

        $helpTroopDays = 0; # 一刀未出天数

        $output .= "<tr>";
        $output .= "\n" . GetChar($offset + 4) . '<td>' . GetUserName($row['user']) . '</td>';
        $output .= "\n" . GetChar($offset + 4) . '<td>' . substr_replace($row['user'], '****', 3, 4) . '</td>';
        for ($i = 0; $i < DAY_NUM; ++$i) {
            $totalDamage[$i] += $row['Day' . ($i + 1)];
            $output .= "\n" . GetChar($offset + 4) . '<td>' . $row['Day' . ($i + 1)];
            // 代刀记录
            if (array_key_exists($day + $i, $helpTroopList)) {
                if (array_key_exists($row['user'], $helpTroopList[$day + $i])) {
                    $dict = $helpTroopList[$day + $i][$row['user']];
                    if (count($dict) > 0) {
                        if (count($dict) >= 3) ++$helpTroopDays;
                        $output .= '<sup style="color:red" title="代刀x' . count($dict) . ': ';
                        for ($j = 0; $j < count($dict); ++$j) {
                            $output .= PHP_EOL . $dict[$j];
                        }
                        $output .= '"><b>' . count($dict) . '</b></sup>';
                    }
                }
            }
            $output .= '</td>';
        }
        $totalDamage[$length] += $row['Total'];
        $output .= "\n" . GetChar($offset + 4) . '<td>' . $row['Total'] . '</td>';
        $output .= "\n" . GetChar($offset + 4) . '<td>' . $helpTroopDays . '</td>';
        $output .= "\n" . GetChar($offset) . "</tr>";
    }

    $output .= "<tr>";
    $output .= "\n" . GetChar($offset + 4) . '<td><b>合计</b></td>';
    $output .= "\n" . GetChar($offset + 4) . '<td></td>';
    for ($i = 0; $i < DAY_NUM + 1; ++$i) {
        $output .= "\n" . GetChar($offset + 4) . '<td><b>' . $totalDamage[$i] . '</b></td>';
    }
    $output .= "\n" . GetChar($offset + 4) . '<td></td>';
    $output .= "\n" . GetChar($offset) . "</tr>";

    return $output . "\n";
}

function GetHelpTroopList() {
    global $SQLite, $day;
    $sql = 'SELECT day, user, damage, troop_operator FROM Damage WHERE troop_operator > 0 AND day >= ' . $day . ' AND group_number = ' . GROUP_ID;
    $ret = $SQLite->query($sql);
    $output = [];
    while ($row = $ret->fetchArray(SQLITE3_ASSOC)) {
        if (!array_key_exists($row['day'], $output)) $output[$row['day']] = [];
        if (!array_key_exists($row['user'], $output[$row['day']])) $output[$row['day']][$row['user']] = [];
        $output[$row['day']][$row['user']][] = GetUserName($row['troop_operator']) . ' (' . $row['damage'] . ')';
    }
    return $output;
}

function SetHeader($offset)
{
    $output = "";
    for ($i = 0; $i < DAY_NUM; ++$i) {
        if ($output != "") $output .= "\n" . GetChar($offset);
        $output .= '<th data-field="Day' . ($i + 1) . '" data-sortable="true">' . Day2Date(GetDay(START_DATE) + $i) . '<img src="img/sort.png" /></th>';
    }
    return $output . "\n";
}
?>