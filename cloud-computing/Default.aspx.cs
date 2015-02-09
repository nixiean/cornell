using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;

namespace NearRestaurants
{
    public partial class Default : System.Web.UI.Page
    {

        static string restDatabase = "http://cuonly.cs.cornell.edu/Courses/CS5412/2015sp/_cuonly/restaurants_all.csv";
        static string googleAddressURL = "https://maps.googleapis.com/maps/api/geocode/json?address=";
        static string googleLatLongURL = "https://maps.googleapis.com/maps/api/geocode/json?latlng=";
        static string googleApiKey = "AIzaSyBaey0c8jLN07Wh2LaDe-RDSVSBt56J1V4";
        static bool useGoogleKey = true;
        static List<string> nearbyRestaurants; 

        static Dictionary<string, List<string>> pinCodes = new Dictionary<string, List<string>>();
        static latLong userRequest;
        static double radiusInMiles;

        static string addressBoxServer;
        static string gpsLatBoxServer;
        static string gpsLongBoxServer;

        static System.TimeSpan timeTaken;

        public class latLong
        {
            public double latitude;
            public double longitude;
            public double latitudeR;
            public double longitudeR;
            public List<String> pinCodes;

            public latLong(string latitude, string longitude)
            {
                this.latitude = Double.Parse(latitude);
                this.longitude = Double.Parse(longitude);
                this.latitudeR = Math.PI*(Double.Parse(latitude))/180;
                this.longitudeR = Math.PI*(Double.Parse(longitude))/180;
                pinCodes = new List<String>();
            }

        }

        public class GoogleGeoCodeResponse
        {

            public string status { get; set; }
            public results[] results { get; set; }

        }

        public class results
        {
            public string formatted_address { get; set; }
            public geometry geometry { get; set; }
            public string[] types { get; set; }
            public address_component[] address_components { get; set; }
        }

        public class geometry
        {
            public string location_type { get; set; }
            public location location { get; set; }
        }

        public class location
        {
            public string lat { get; set; }
            public string lng { get; set; }
        }

        public class address_component
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public string[] types { get; set; }
        }

        public static void getCSV()
        {
            if (userRequest != null)
            {
                try
                {
                    HttpWebRequest csvWebRequest = (HttpWebRequest)
                                  HttpWebRequest.Create(new Uri(restDatabase));

                    if ((csvWebRequest.GetResponse().ContentLength > 0))
                    {
                        System.IO.StreamReader rowReader =
                          new System.IO.StreamReader(
                          csvWebRequest.GetResponse().GetResponseStream());
                        String csvRow;
                        while ((csvRow = rowReader.ReadLine()) != null)
                        {
                            //Console.WriteLine(csvRow);
                            string[] csvElements = csvRow.Split(',');

                            string pinCode = csvElements[7];
                            List<string> restaurants;
                            if (pinCodes.TryGetValue(pinCode, out restaurants))
                            {
                                restaurants.Add(csvElements[3] + " " + csvElements[4] + " " +
                                    csvElements[5] + " " + csvElements[6] + " " + csvElements[7]);
                                pinCodes[pinCode] = restaurants;
                            }
                            else
                            {
                                restaurants = new List<string>();
                                restaurants.Add(csvElements[3] + " " + csvElements[4] + " " +
                                    csvElements[5] + " " + csvElements[6] + " " + csvElements[7]);
                                pinCodes[pinCode] = restaurants;
                            }
                        }

                        if (rowReader != null) rowReader.Close();
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Could not fetch the csv file");
                }
            }
        }


        public static void getUserRequest()
        {
            userRequest = null;
            if (addressBoxServer!= "")
            {
                    string urlAddress = googleAddressURL
                                + Uri.EscapeDataString(addressBoxServer);
                    if (useGoogleKey == true)
                    {
                        urlAddress = urlAddress +"&key=" + googleApiKey;
                    }
                            
                    var result = new System.Net.WebClient().DownloadString(urlAddress);
                    GoogleGeoCodeResponse test = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);
                    if (test.status == "OK")
                    {
                        userRequest = new latLong(test.results[0].geometry.location.lat, test.results[0].geometry.location.lng);
                    }
            
            }
            else if (gpsLatBoxServer != "" && gpsLongBoxServer != "")
            {
               userRequest = new latLong(gpsLatBoxServer.ToString(),gpsLongBoxServer.ToString());
            }
           
            if (userRequest != null)
            {
                string urlLatLong = googleLatLongURL
                                   + userRequest.latitude + "," + userRequest.longitude;
                if (useGoogleKey == true)
                {
                    urlLatLong = urlLatLong + "&key=" + googleApiKey;
                }
                var resultPinCodes = new System.Net.WebClient().DownloadString(urlLatLong);
                var jsonPinCodes = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(resultPinCodes);
                if (jsonPinCodes.status == "OK")
                {
                    address_component[] address_components = jsonPinCodes.results[0].address_components;
                    foreach (address_component add_comp in address_components)
                    {
                        if (add_comp.types[0] == "postal_code")
                        {
                            userRequest.pinCodes.Add(add_comp.long_name);
                        }
                    }
                }
            }
        }

        public void getRestaurants()
        {
            if (userRequest != null)
            {
                nearbyRestaurants = new List<string>();
                List<string> restaurants = new List<string>();
                foreach (string pinCode in userRequest.pinCodes)
                {
                    List<string> rests;
                    if (pinCodes.TryGetValue(pinCode, out rests))
                    {
                        restaurants.AddRange(pinCodes[pinCode]);
                        Console.WriteLine(pinCodes[pinCode].Count);
                    }
                }
                foreach (string restaurant in restaurants)
                {
                    getDistance(restaurant);
                    Thread.Sleep(200);
                }
            }
        }


        public static void getDistance(string restaurant)
        {
            string urlAddress = googleAddressURL
                                + Uri.EscapeDataString(restaurant);
            if (useGoogleKey == true)
            {
                urlAddress = urlAddress + "&key=" + googleApiKey;
            }
            var result = new System.Net.WebClient().DownloadString(urlAddress);
            GoogleGeoCodeResponse test = JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);
            if (test.status == "OK")
            {
                double lat1 = Double.Parse(test.results[0].geometry.location.lat);
                double long1 = Double.Parse(test.results[0].geometry.location.lng);
                double latR1 = Math.PI*(lat1)/180;
                double longR1 = Math.PI*(long1)/180;
                double distance = Math.Acos(Math.Sin(latR1) * Math.Sin(userRequest.latitudeR) + Math.Cos(latR1) * Math.Cos(userRequest.latitudeR) * Math.Cos(longR1 - userRequest.longitudeR))*3963.1676; 
                if ( distance <= radiusInMiles)
                {
                    distance = Math.Truncate(100 * distance) / 100;
                    nearbyRestaurants.Add(restaurant+ "," + lat1.ToString() + "," + long1.ToString() + ", ; Distance:" + distance + " Miles");
                }

            }
        }
        public void messageDisplay(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + message + "');", true);
        }

        /*
        public void updateMarkers()
        {
            if (addressEntry.Checked && addressBox != null)
            {
                nearbyRestaurants.Add("Sangam Restuarant,42.442186,-76.487660");
                nearbyRestaurants.Add("Mehek Restaurant,42.441950,-76.487706");
            }
        }*/
         
        protected void Button1_Click(object sender, EventArgs e)
        {
            //Default obj1 = new Default();
            //obj1.updateMarkers();
        }
        
        /*
        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            //obj1.getCSV();
            Console.WriteLine();
        }
        */

        protected void Page_Load(object sender, EventArgs e)
        {
            string value = distanceRadiusBox.Text.ToString();
            if (value != "")
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                
                addressBoxServer = addressBox.Text.ToString();
                gpsLatBoxServer = gpsLatBox.Text.ToString();
                gpsLongBoxServer = gpsLongBox.Text.ToString();
                radiusInMiles = Double.Parse(value);
                addressBox.Text = "";
                gpsLatBox.Text = "";
                gpsLongBox.Text = "";
                distanceRadiusBox.Text = "";
                getUserRequest();

                /*
                // Write the modified stock picks list back to session state.
                Session["PinCodes"] = pinCodes;
                
                // When retrieving an object from session state, cast it to 
                // the appropriate type.
                Dictionary<string, List<string>> stockPicks = (Dictionary<string, List<string>>)Session["PinCodes"];
                 * */
                if (pinCodes.Count == 0)
                {
                    getCSV();
                }
                getRestaurants();
                hiddenMarkers.InnerHtml = "";
                foreach (string restaurant in nearbyRestaurants)
                {
                    hiddenMarkers.InnerHtml = hiddenMarkers.InnerHtml + "\n" + restaurant;
                }
                hiddenLatitude.InnerHtml = "";
                hiddenLatitude.InnerHtml = "";
                if (userRequest != null)
                {
                    hiddenLatitude.InnerHtml = userRequest.latitude.ToString();
                    hiddenLongitude.InnerHtml = userRequest.longitude.ToString();
                }
                hiddenRadius.InnerHtml = radiusInMiles.ToString();
                sw.Stop();
                timeTaken = sw.Elapsed;
                runDetails.InnerHtml = "Time taken: " + timeTaken.Minutes.ToString() + " minutes " + timeTaken.Seconds.ToString() + " seconds" +"\n"
                    + "Total number of restaurants: " + nearbyRestaurants.Count.ToString();
            }
        }
        
    }
}