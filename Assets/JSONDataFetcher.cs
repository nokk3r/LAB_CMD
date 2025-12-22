using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class AxisData
{
    public float min_position;
    public float max_position;
    public float current_position;
    public string units;
}

[System.Serializable]
public class Axes
{
    public AxisData x;
    public AxisData y;
    public AxisData z;
    public AxisData a;
    public AxisData c;
}

[System.Serializable]
public class CncMachineData
{
    public string machine_id;
    public string model;
    public Axes axes;
    public object spindle; // Можно расширить, если нужно
    public object tool_changer; // Можно расширить, если нужно
    public object workpiece_zero; // Можно расширить, если нужно
    public string timestamp;
    public string status;
}

[System.Serializable]
public class RootCncData
{
    public CncMachineData cnc_machine;
}

[System.Serializable]
public class ParsedData
{
    public Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
}

public class JSONDataFetcher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string apiUrl;

    [Header("Debug")]
    [SerializeField] private ParsedData parsedData = new ParsedData();
    [SerializeField] private bool autoFetchOnStart = true;
    [SerializeField] private float refreshInterval = 2.0f;

    void Start()
    {
        if (autoFetchOnStart)
        {
            StartCoroutine(FetchDataPeriodically());
        }
    }

    public IEnumerator FetchDataPeriodically()
    {
        while (true)
        {
            yield return StartCoroutine(FetchData());
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    public IEnumerator FetchData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    ProcessJsonData(responseText);
                    Debug.Log("Данные успешно обновлены!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Ошибка обработки данных: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Ошибка запроса: {request.error}");
            }
        }
    }

    private void ProcessJsonData(string responseText)
    {
        parsedData.keyValuePair.Clear();

        try
        {
            // Используем Unity's JsonUtility для десериализации
            RootCncData root = JsonUtility.FromJson<RootCncData>(responseText);

            if (root?.cnc_machine == null)
            {
                Debug.LogError("Неверный формат JSON: отсутствует ключ 'cnc_machine'");
                return;
            }

            // Извлечение данных осей
            var axes = new Dictionary<string, AxisData>
        {
            { "x", root.cnc_machine.axes.x },
            { "y", root.cnc_machine.axes.y },
            { "z", root.cnc_machine.axes.z },
            { "a", root.cnc_machine.axes.a },
            { "c", root.cnc_machine.axes.c }
        };

            foreach (var axis in axes)
            {
                
                string key = $"Channel_1_Axis_{GetAxisIndex(axis.Key)} ({axis.Key.ToUpper()})_CurPos";
                parsedData.keyValuePair[key] = axis.Value.current_position.ToString();
                Debug.Log($"Добавлено: {key} = {axis.Value.current_position}");
            }

            Debug.Log($"Обработано {parsedData.keyValuePair.Count} записей из JSON");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка обработки JSON: {e.Message}\n{responseText}");
        }
    }


    private int GetAxisIndex(string axisName)
    {
        return axisName.ToLower() switch
        {
            "x" => 1,
            "y" => 2,
            "z" => 3,
            "a" => 4,
            "c" => 5,
            _ => -1
        };
    }

    // Для отладки - вывести все данные
    [ContextMenu("Print All Data")]
    public void PrintAllData()
    {
        if (parsedData.keyValuePair.Count == 0)
        {
            Debug.Log("Нет данных для отображения");
            return;
        }

        foreach (var pair in parsedData.keyValuePair)
        {
            Debug.Log($"{pair.Key}: {pair.Value}");
        }
    }

    // Получить числовое значение (для позиций осей)
    public float GetFloatValue(string key, float defaultValue = 0f)
    {
        string stringValue = GetValue(key);
        if (stringValue != null && float.TryParse(stringValue, out float result))
        {
            return result;
        }
        return defaultValue;
    }

    private string GetValue(string key)
    {
        if (parsedData.keyValuePair.TryGetValue(key, out string value))
        {
            return value;
        }
        return null;
    }
}