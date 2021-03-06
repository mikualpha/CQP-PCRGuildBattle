<?php 
require('functions.php'); 
?><!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <!-- <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, minimum-scale=1.0, user-scalable=no"> -->
    <title>CommentsView</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/css/bootstrap.min.css" integrity="sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh" crossorigin="anonymous">
    <link rel="stylesheet" href="node_modules/tablesorter/dist/css/filter.formatter.min.css">
    <script src="node_modules/jquery/dist/jquery.min.js"></script>
    <script src="node_modules/tablesorter/dist/js/jquery.tablesorter.min.js"></script>
    <script src="node_modules/tablesorter/dist/js/jquery.tablesorter.widgets.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js" integrity="sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/js/bootstrap.min.js" integrity="sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6" crossorigin="anonymous"></script>
</head>

<body style="height: 100%">
    <?php require('style.php') ?>
    <div id="container" class="container-pc">
        <h2 style="text-align: center; margin-bottom: 2%">留言板</h2>
        <?php ShowPageButtons(8, 'comments'); ?>
		<b><?php PrintGroupRank(); ?></b>
        <table>
            <td>
                <table id="stat" class="table table-hover table-striped tablesorter" data-sort-name="Message" data-sort-order="desc">
                    <?php echo PrintTable(20); ?>
                </table>
            </td>
            <?php if (defined('SHOW_DISCORD_CHANNEL') && SHOW_DISCORD_CHANNEL) { ?>
            <td valign="top">
                <iframe id="discord" src="https://discord.com/widget?id=<?php echo DISCORD_ID; ?>&theme=light" width="350" min-height="500" allowtransparency="true" frameborder="0" sandbox="allow-popups allow-popups-to-escape-sandbox allow-same-origin allow-scripts"></iframe>
            </td>
            <?php } ?>
        </table>
        <?php echo PrintCopyRight(); ?>
    </div>
</body>

<?php if (defined('SHOW_DISCORD_CHANNEL') && SHOW_DISCORD_CHANNEL) { ?>
<script type="text/javascript">
    window.onload = () => {
        document.getElementById('discord').height = document.getElementById('stat').offsetHeight;
    }
</script>
<?php } ?>

</html>

<?php
function PrintTable($offset)
{
    $content = removeBOM(file_get_contents(DATA_PATH . INI_FILENAME));
    $json = json_decode($content, true);
    $messages = $json['messages'];

    # 表头
    $output = "<thead>";
    $output .= "\n" . GetChar($offset + 4) . "<tr>";
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="Name" width="200">Name<img src="img/sort.png" /></th>';
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="QQ" width="150">QQ<img src="img/sort.png" /></th>';
    $output .= "\n" . GetChar($offset + 8) . '<th data-field="Message" data-sortable="true">Message<img src="img/sort.png" /></th>';
    $output .= "\n" . GetChar($offset + 4) . '</tr>';
    $output .= "\n" . GetChar($offset) . '</thead>';

    foreach ($messages as $key => $value) {
        $output .= "\n" . GetChar($offset + 4) . '<tr>';
        $output .= "\n" . GetChar($offset + 8) . '<td>' . GetUserName($key) . '</td>';
        $output .= "\n" . GetChar($offset + 8) . '<td>' . substr_replace($key, '****', 3, 4) . '</td>';
        $output .= "\n" . GetChar($offset + 8) . '<td>' . htmlentities($value) . '</td>';
        $output .= "\n" . GetChar($offset + 4) . '</tr>';
    }

    return $output;
}
?>