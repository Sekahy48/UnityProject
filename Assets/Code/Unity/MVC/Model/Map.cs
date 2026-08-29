using UnityEngine;

public class Map
{
    private GameObject root;

    public Map(string resourcePath)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError("No se pudo cargar el prefab del mapa en: " + resourcePath);
            return;
        }

        root = GameObject.Instantiate(prefab);
    }

    public void Dispose()
    {
        if (root != null)
        {
            GameObject.Destroy(root);
        }
    }
}
