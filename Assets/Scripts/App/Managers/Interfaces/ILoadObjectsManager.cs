namespace Prototype.TopDown2DNetworked
{
    public interface ILoadObjectsManager
    {
        T GetObjectByPath<T>(string path) where T : UnityEngine.Object;

        string GetTextByPath(string path);
        void SetTextByPath(string path, string data);
    }
}