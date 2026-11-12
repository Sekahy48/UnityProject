public class MapManager
{
    private Map _currentMap;

    public void LoadMap(string name)
    {
        if (_currentMap != null)
            _currentMap.Dispose();

        _currentMap = new Map("Maps/" + name);
    }

    public Map GetCurrentMap()
    {
        return _currentMap;
    }
}
