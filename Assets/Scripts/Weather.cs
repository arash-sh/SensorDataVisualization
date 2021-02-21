using TMPro;
using UnityEngine;

public class Weather : MonoBehaviour {

    private GameObject WeatherGO;
    private GameObject WeatherIcon;
    
     void Start () {
        WeatherGO = Instantiate<GameObject>(Specs.WeatherPrefab);
        WeatherGO.transform.position = new Vector3(175, 6, 15);
        GameObject WeatherIcon = WeatherGO.transform.Find("Sun").gameObject;
        WeatherIcon.SetActive(true);

        MouseHandler mh = WeatherIcon.AddComponent<MouseHandler>();
        mh.SetHandler(ShowWeatherInfo, MouseHandler.MOUSE_EVENT.ENTER);
        mh.SetHandler(HideWeatherInfo, MouseHandler.MOUSE_EVENT.EXIT);

    }

    void Update()
    {
        WeatherGO.transform.LookAt(2* WeatherGO.transform.position - Camera.main.transform.position);
        //WeatherGO.transform.Rotate(0, 90, 0);
    }

    void ShowWeatherInfo()
    {
        WeatherGO.GetComponent<TextMeshPro>().text = "18 °C";

    }
    void HideWeatherInfo()
    {
        WeatherGO.GetComponent<TextMeshPro>().text = "";

    }
}
