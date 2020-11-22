<style type="text/css">
        .readtext {
            font-size: 10pt;
            font-weight: bolder;
        }

        .notice {
            font-size: 18pt;
            text-align: center;
        }

        body {
            background: url('img/back/<?php echo RandomPic(); ?>') no-repeat center center fixed;
            background-size: cover;
            min-height: 100vh;
            box-sizing: border-box;
            margin: 0;
        }

        .container-pc {
            background-color: rgba(255, 255, 255, 0.75);
            border-radius: 10px;
            height: 100%;
            margin-left: 50px;
            margin-right: 50px;
            margin-top: 20px;
            padding: 20px;
        }

        .container-mobile {
            background-color: rgba(255, 255, 255, 0.78);
            border-radius: 10px;
            height: 100%;
            margin-left: 10px;
            margin-right: 10px;
            margin-top: 20px;
            padding: 10px;
        }

        #stat {
            table-layout: fixed;
        }
    </style>


    <script type="text/javascript">
        window.onload = function() {
            $("#stat").tablesorter();
            if (document.documentElement.clientWidth < 1000) {
                document.getElementById("container").className = "container-mobile";
            } else {
                document.getElementById("container").className = "container-pc";
            }
        };
    </script>