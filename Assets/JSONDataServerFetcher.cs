using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ObjectData
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class ServerData
{
    public ObjectData object1;
    public ObjectData object2;
    public ObjectData object3;
}


[System.Serializable]
public class ParsedServerData
{
    public Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
}

public class JSONDataServerFetcher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string apiUrl;

    [Header("Debug")]
    [SerializeField] private ParsedServerData ParsedServerData = new ParsedServerData();
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
        ParsedServerData.keyValuePair.Clear();

        try
        {
            // Используем Unity's JsonUtility для десериализации
            ServerData root = JsonUtility.FromJson<ServerData>(responseText);

            if (root == null)
            {
                Debug.LogError("Неверный формат JSON: отсутствует ключ 'cnc_machine'");
                return;
            }

            // Извлечение данных осей
            var axes = new Dictionary<string, ObjectData>
        {
            { "object1", root.object1 },
            { "object2", root.object2 },
            { "object3", root.object3 }
        };

            foreach (var axis in axes)
            {

                string key = $"Channel_1_Axis_{GetAxisIndex(axis.Key)} (X)_Rotation";
                ParsedServerData.keyValuePair[key] = axis.Value.x.ToString();
                Debug.Log($"Добавлено: {key} = {axis.Value.x}");

                string key2 = $"Channel_1_Axis_{GetAxisIndex(axis.Key)} (Y)_Rotation";
                ParsedServerData.keyValuePair[key2] = axis.Value.y.ToString();
                Debug.Log($"Добавлено: {key2} = {axis.Value.y}");

                string key3 = $"Channel_1_Axis_{GetAxisIndex(axis.Key)} (Z)_Rotation";
                ParsedServerData.keyValuePair[key3] = axis.Value.z.ToString();
                Debug.Log($"Добавлено: {key3} = {axis.Value.z}");
            }

            Debug.Log($"Обработано {ParsedServerData.keyValuePair.Count} записей из JSON");
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
            "object1" => 1,
            "object2" => 2,
            "object3" => 3
        };
    }

    // Для отладки - вывести все данные
    [ContextMenu("Print All Data")]
    public void PrintAllData()
    {
        if (ParsedServerData.keyValuePair.Count == 0)
        {
            Debug.Log("Нет данных для отображения");
            return;
        }

        foreach (var pair in ParsedServerData.keyValuePair)
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
        if (ParsedServerData.keyValuePair.TryGetValue(key, out string value))
        {
            return value;
        }
        return null;
    }
}