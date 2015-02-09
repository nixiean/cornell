<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="NearRestaurants.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" style="height: 100%">
<head runat="server">
    <style type="text/css">
        #gpsLatBox, #gpsLongBox
        {
            display: none;
        }
    </style>
    <title>Nearby restaurants</title>
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js">
    </script>
    <script type="text/javascript" src="https://maps.googleapis.com/maps/api/js?sensor=false">
    </script>
    <script type="text/javascript">
        function initialize() {
            var mapLat = document.getElementById('hiddenLatitude').innerHTML
            var mapLong = document.getElementById('hiddenLongitude').innerHTML;
            var zoomMap = 15;
            mapLat = (mapLat.trim) ? mapLat.trim() : mapLat.replace(/^\s+/, '');
            mapLong = (mapLong.trim) ? mapLong.trim() : mapLong.replace(/^\s+/, '');
            if (mapLat == '') {
                mapLat = 40.542130;
                mapLong = -101.222317;
                zoomMap = 4;
            }
            mapLat = parseFloat(mapLat);
            mapLong = parseFloat(mapLong);
            var mapOptions = {
                center: { lat: mapLat, lng: mapLong },
                zoom: zoomMap
            };
            var map = new google.maps.Map(document.getElementById('mapContainer'),
            mapOptions);

            var restaurants = document.getElementById('hiddenMarkers').innerHTML;
            var locations = restaurants.split("\n");
            /*marker = new google.maps.Marker({
            map: map,
            draggable: true,

            position: markPos
            });*/
            var infowindow = new google.maps.InfoWindow();

            var marker, i;

            for (i = 0; i < locations.length; i++) {
                var markerOptions = locations[i].split(",");
                marker = new google.maps.Marker({
                    position: new google.maps.LatLng(markerOptions[1], markerOptions[2]),
                    map: map,
                    animation: google.maps.Animation.DROP,
                });

                google.maps.event.addListener(marker, 'click', (function (marker, i) {
                    return function () {
                        var markerContent = markerOptions[0];
                        var space = " ";
                        markerContent = markerContent.concat(space, markerOptions[3]);
                        infowindow.setContent(markerContent);
                        infowindow.open(map, marker);
                    }
                })(marker, i));
            }

        }
        google.maps.event.addDomListener(window, 'load', initialize);

        $(document).ready(function () {

            $('input#addressEntry').click(
		function () {
		    $("#addressBox").show();
		    $("#gpsLatBox").hide();
		    $("#gpsLongBox").hide();
		});

            $('input#gpsEntry').click(
		function () {
		    $("#addressBox").hide();
		    $("#gpsLatBox").show();
		    $("#gpsLongBox").show();
		});


        });
    </script>
</head>
<body style="height: 100%" bgcolor="#cc3300">
    <div id="mainContainer" style="border-style: inset; border-width: thick; height: 100%;
        border-style: inset; border-width: thick">
        <div id="headerContainer" style="border-width: thin; margin: auto; border-style: groove;
            width: 100%; background-color: #CCCCCC; font-family: cursive; font-size: 41px;"
            align="center">
            Restaurant finder</div>
        <div id="leftContainer" style="height: 100%; border-style: groove; border-width: medium;
            float: left; width: 20%; background-color: #C0C0C0;">
            <form id="userForm" runat="server" 
            style="border-bottom-style: groove; border-width: thin; margin-bottom: 10px; padding-bottom: 10px;">
            <p style="border-style: none; border-width: medium; padding: 10px">
                Enter the address or coordinates of the location to find the nearby restuarants</p>
            <asp:RadioButton ID="addressEntry" Text="Address" Checked="True" GroupName="userLocation"
                runat="server" />
            <asp:RadioButton ID="gpsEntry" Text="GPS Coordinates" GroupName="userLocation" runat="server" />
            <asp:TextBox ID="addressBox" runat="server" placeholder="Enter address here..." type="text"
                Style="margin: 10px; border-style: inset; border-width: medium; padding: 5px;" />
            <asp:TextBox ID="gpsLatBox" runat="server" placeholder="Enter latitude here..." type="text"
                Style="margin: 10px; border-style: inset; border-width: medium; padding: 5px;" />
            <asp:TextBox ID="gpsLongBox" runat="server" placeholder="Enter longitude here..."
                type="text" Style="margin: 10px; border-style: inset; border-width: medium; padding: 5px;" />
                <p style="margin: 0px; border-style: none; border-width: medium; padding: 10px 10px 0px 10px;">Distance</p>
                <asp:TextBox ID="distanceRadiusBox" runat="server" placeholder="Enter radius in miles here..."
                type="text" Style="margin: 10px; border-style: inset; border-width: medium; padding: 5px;" />
            <asp:Button ID="submitButton" runat="server" Text="Submit" BackColor="Black" OnClick="Button1_Click"
                ForeColor="White" Height="24px" Width="100px" />
            </form>
            <div id="runDetails" runat="server" style="padding: 10px"></div>
        </div>
        <div id="mapContainer" style="height: 100%; border-style: groove; border-width: medium;
            padding: 10px">
        </div>
    </div>
    <div id="hiddenMarkers" runat="server" style="visibility: hidden"></div>
        <div id="hiddenLatitude" runat="server" style="visibility: hidden"></div>
        <div id="hiddenLongitude" runat="server" style="visibility: hidden"></div>
</body>
</html>
