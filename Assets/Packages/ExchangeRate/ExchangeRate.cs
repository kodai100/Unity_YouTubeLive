using UnityEngine;
using UniRx.Async;
using SimpleJSON;
using UnityEngine.Networking;

namespace ExchangeRate
{

    public enum Currency
    {
        BGN, NZD, ILS, RUB, CAD, USD, PHP, CHF, AUD, JPY, TRY, HKD, MYR, HRK, CZK, IDR, DKK,
        NOK, HUF, GBP, MXN, THB, ISK, ZAR, BRL, SGD, PLN, INR, KRW, RON, CNY, SEK, EUR
    }

    public class ExchangeRateAPI
    {

        static readonly string url = "https://api.exchangeratesapi.io/latest";

        Currency baseCurrency;

        public ExchangeRateAPI(Currency baseCurrency)
        {
            this.baseCurrency = baseCurrency;
        }

        public async UniTask<float> Exchange(float baseCurrencyValue, Currency targetCurrency)
        {
            return await Exchange(baseCurrencyValue, baseCurrency, targetCurrency);
        }

        public static async UniTask<float> Exchange(float baseCurrencyValue, Currency baseCurrency, Currency targetCurrency)
        {

            float rate = await GetRate(baseCurrency, targetCurrency);

            return baseCurrencyValue * rate;

        }

        public static async UniTask<float> GetRate(Currency baseCurrency, Currency targetCurrency)
        {

            var jsonStr = await GetExchangeRateJson(baseCurrency);

            var jsonNode = JSON.Parse(jsonStr);

            var rates = jsonNode["rates"];

            return rates[targetCurrency.ToString()].AsFloat;
        }


        static async UniTask<string> GetExchangeRateJson(Currency baseCurrency)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{url}?base={baseCurrency.ToString()}");

            await request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.LogError("Get: [Network Error]");
                return null;
            }
            else if (request.isHttpError)
            {
                Debug.LogError($"get: [Http Error] {request.downloadHandler.text}");
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }

    }
}