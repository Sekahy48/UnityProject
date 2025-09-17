public class MapManager
{
    private Map currentMap;

    public void LoadMap(string name)
    {
        if (currentMap != null)
            currentMap.Dispose();

        currentMap = new Map("Maps/" + name);
    }

    public Map GetCurrentMap()
    {
        return currentMap;
    }
}
