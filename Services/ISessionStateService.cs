using System.Collections.Generic;

public interface ISessionStateService<T>
{
    void Save(string name, T value);
    Dictionary<string, T> GetAll();
    T Get(string name);
    bool Remove(string name);
    void Clear();
}