using System.Collections.Generic;

namespace UnderstandingOOPSApp.Interfaces
{
    internal interface IRepository<K,T> where T :class
    {
        T Create(T item);
        T? GetAccount(K key);
        List<T>? GetAccounts();
        T? Update(K key, T item);
        T? Delete(K key);
    }
}