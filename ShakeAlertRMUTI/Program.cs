using Newtonsoft.Json;
using ShakeAlertRMUTI.Params;
using System.Text;

double latitude = 14.98779185053724;
double longitude = 102.11803990603767;
double radiusKm = 2000;  // รัศมีค้นหา 2000 กม. จาก มทร.อีสาน
double minMagnitude = 4.5; // ขนาดแผ่นดินไหวขั้นต่ำที่ต้องการแจ้งเตือน

string url = getUrlEarthquake(latitude , longitude , radiusKm);

string serviceMsTeamsAPI = "https://sport.rmuti.ac.th/serviceMsTeams/public/api/createMessage";
string webHookId         = "https://rmuti365.webhook.office.com/webhookb2/a40f81e9-fe0b-40e9-94a9-39639e276e7b@733e2ce0-ce28-4dfa-8af6-ad57b37090ce/IncomingWebhook/9147d81463294376b6b97c0b1a7515f1/abc7d249-cfd9-4c3a-abf9-076e9a90cd87/V2zmFw7UU_7Do6UDGrJkEl8I82I5XDiExQhqlXZ8x15mU1";

using (HttpClient client = new HttpClient())
{
    try
    {
        HttpResponseMessage responsEearthquake = await client.GetAsync(url);
        responsEearthquake.EnsureSuccessStatusCode();
        string responseBody = await responsEearthquake.Content.ReadAsStringAsync();

        var data = System.Text.Json.JsonSerializer.Deserialize<EarthquakeData>(responseBody);
        bool found = false; //กรณีพบข้อมูล

        Console.WriteLine($"minMagnitude > {minMagnitude} radiusKm {radiusKm} Km. zone > ({latitude}, {longitude})\n");

        // วนลูปตรวจสอบแผ่นดินไหวแต่ละเหตุการณ์

        foreach (var earthquake in data!.features)
        {
            double magnitude = earthquake.properties.mag;

            if (magnitude > minMagnitude)
            {
                found = true;

                string place = earthquake.properties.place;
                long timeUnix = earthquake.properties.time;
                string urlDetail = earthquake.properties.url;

                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timeUnix).DateTime;

                Console.WriteLine($"Location: {place}");
                Console.WriteLine($"Size: {magnitude} scale");
                Console.WriteLine($"Time: {dateTime}");
                Console.WriteLine($"Detail: {urlDetail}");
                Console.WriteLine(new string('-', 50));

                var msTeamsCallData = new MsTeamsCallData()
                {
                    webHookId = webHookId,
                    type = "primary",
                    title = "การแจ้งเตือนแผ่นดินไหว RMUTI",
                    message = $"📍สถานที่: {place} | " +
                $"📏 ขนาด: {magnitude} แมกนิจูด |" +
                $" ⏰ เวลา: {dateTime}",
                    button = new string[2] { "รายละเอียด", urlDetail }
                };

                var content = ConvertToFormUrlEncodedContent(msTeamsCallData);

                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("User-Agent", "MyAgent/1.0");

                // ส่งคำขอ HTTP แบบ POST
                var response = await client.PostAsync(serviceMsTeamsAPI, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());

            }
        }

        if (!found)
        {
            //✅ ไม่มีแผ่นดินไหวที่มีขนาดมากกว่า {minMagnitude} ในรัศมี {radiusKm} กม.

            var msTeamsCallData = new MsTeamsCallData()
            {
                webHookId = webHookId,
                type = "success",
                title = "การแจ้งเตือนแผ่นดินไหว RMUTI",
                message = $"ไม่มีแผ่นดินไหวที่มีขนาดมากกว่า {minMagnitude} ในรัศมี {radiusKm} กม",
            };

            var content = ConvertToFormUrlEncodedContent(msTeamsCallData);

            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            client.DefaultRequestHeaders.Add("User-Agent", "MyAgent/1.0");

            // ส่งคำขอ HTTP แบบ POST
            var response = await client.PostAsync(serviceMsTeamsAPI, content);
        }
    }
    catch (Exception e)
    {

    }
}

FormUrlEncodedContent ConvertToFormUrlEncodedContent(object obj)
{
    var dictionary = new Dictionary<string, string>();

    foreach (var property in obj.GetType().GetProperties())
    {
        var value = property.GetValue(obj);

        if (value is string strValue)
        {
            dictionary.Add(property.Name, strValue);
        }
        else if (value is string[] arrayValue)
        {
            // ถ้าค่าเป็น array ให้แปลงเป็น key-value หลายรายการ
            for (int i = 0; i < arrayValue.Length; i++)
            {
                dictionary.Add($"{property.Name}[{i}]", arrayValue[i]); // สร้าง key ในรูปแบบ array
            }
        }
    }

    return new FormUrlEncodedContent(dictionary);
}
string getUrlEarthquake(double latitude , double longitude , double radiusKm)
{
    string url = $"https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson" +
             $"&latitude={latitude}" +
             $"&longitude={longitude}" +
             $"&maxradiuskm={radiusKm}" +
             $"&limit=5"; // จำกัดข้อมูลที่ดึงมา 20 รายการ
   return url ;
}