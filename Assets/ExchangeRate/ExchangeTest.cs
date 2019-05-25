using UnityEngine;

public class ExchangeTest : MonoBehaviour
{

    [Header("Base")]
    public Currency baseCurrency = Currency.JPY;
    public float value = 100;

    [Header("Target")]
    public Currency targetCurrency = Currency.USD;


    async void Start()
    {

        float result = await ExchangeRateAPI.Exchange(value, baseCurrency, targetCurrency);

        Debug.Log($"{result} [{targetCurrency.ToString()}]");
    }

    void Update()
    {

    }
}
