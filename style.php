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

        .page-title {
            text-align: center;
            margin-bottom: 2%;
        }

        .page-title-mobile {
            text-align: center;
            margin-top: 4%;
            margin-bottom: 2%;
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

        .container-pc > .top-button {
            display: block;
            cursor: pointer;
            position: absolute;
            top: 20px;
            right: 80px;
        }

        .container-mobile > .top-button {
            display: block;
            cursor: pointer;
            position: absolute;
            top: 20px;
            right: 30px;
        }

        button {
            size: 50%;
            margin-left: 5px;
        }

        .hide {
            display: none;
        }
    </style>


    <script type="text/javascript">
        window.onload = function() {
            $("#stat").tablesorter();
            if (document.documentElement.clientWidth < 1000) {
                document.getElementById('container').className = "container-mobile";
                document.getElementById('page-title').className = "page-title-mobile";
                if (document.getElementById('discord')) {
                    document.getElementById('discord').className = "hide";
                }
            } else {
                document.getElementById('container').className = "container-pc";
                if (document.getElementById('discord')) {
                    document.getElementById('discord').height = document.getElementById('stat').offsetHeight;
                }
            }
        };
    </script>