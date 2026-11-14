using UnityEngine;

public class Map
{
    private GameObject _root;

    public Map(string resourcePath)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError("No se pudo cargar el prefab del mapa en: " + resourcePath);
            return;
        }

        _root = GameObject.Instantiate(prefab);
    }

    public void Dispose()
    {
        if (_root != null)
        {
            GameObject.Destroy(_root);
        }
    }
}
