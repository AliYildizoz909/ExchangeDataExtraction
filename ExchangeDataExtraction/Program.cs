using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ExchangeDataExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
			//API istekleri için httpRequester
            HttpClient httpClient = new HttpClient();
			
			//Dolar kuru fiyatını çekmek için başlangıç tarihi
            DateTime starTime = new DateTime(2010, 01, 01);
			//Toplam veri sayısı= gün sayısı(Her günün kur fiyatını çekmek için )
            TimeSpan interval = DateTime.Now - starTime;
            Console.WriteLine("**********************************************");
            Console.WriteLine("Base: USD");
			//Çekilen dolar kurlarının listesi
            List<Result> exchanges = new List<Result>();

            for (int i = 0; i <= interval.Days; i++)
            {
			
                string day = starTime.AddDays(i).ToString("yyyy-MM-dd");
				//API için url oluşturma
                string url = "https://api.exchangeratesapi.io/" + day + "?base=USD";
				//API isteği sonucu ham data(resultJson)
                var resultJson = httpClient.GetStringAsync(url);
				//ham datayı c# objesine çevirme
                Result res = Newtonsoft.Json.JsonSerializer.CreateDefault()
                    .Deserialize<Result>(new JsonTextReader(new StringReader(resultJson.Result)));
				
					//Aynı data mı kontrolü(Bazen iki üç defa aynı data geliyor. o yüzden böyle bir kontrol yaptım.) 
                if (day == res.Date.ToString("yyyy-MM-dd"))
                {
					//Gelen data önceki data ile aynı değilse exchanges listemize verimizi ekliyoruz
                    exchanges.Add(res);
                    Console.WriteLine($"{nameof(res.Rates.TRY)}: {res.Rates.TRY} , {nameof(res.Date)}: {res.Date}");
                }
            }
			//verilerimizi tarihe göre sıralıyoruz
            exchanges = exchanges.OrderBy(result => result.Date).ToList();
			//verimizi csv formatında dışa aktarmak için bu nesneden fayadalanıyoruz.
            StringBuilder rawExchangeData = new StringBuilder();
            rawExchangeData.AppendLine("Day,Date,Rate");
            StringBuilder exchangeData = new StringBuilder();
            exchangeData.AppendLine("Day,Rate");
            for (int i = 0; i < exchanges.Count; i++)
            {
                string rate = exchanges[i].Rates.TRY.ToString(CultureInfo.InvariantCulture).Replace(",", ".");
                rawExchangeData.AppendLine($"{i + 1},{exchanges[i].Date:yyyy-MM-dd},{rate}");
                exchangeData.AppendLine($"{i + 1},{rate}");
            }
			
			//verilerimizi, verdiğimiz yola csv formatında katdediyoruz
            File.WriteAllText(@"C:\Users\aliyi\source\repos\ExchangeDataExtraction\ExchangeDataExtraction\exchangeData.csv", exchangeData.ToString());
            File.WriteAllText(@"C:\Users\aliyi\source\repos\ExchangeDataExtraction\ExchangeDataExtraction\rawExchangeData.csv", rawExchangeData.ToString());

        }
    }
	//İstek sonucu ham veriyi dönüştürdüğümüz nesneler
    class Result
    {
        public Rates Rates { get; set; }
        public string Base { get; set; }
        public DateTime Date { get; set; }
    }
    class Rates
    {
        public double TRY { get; set; }

    }
}
