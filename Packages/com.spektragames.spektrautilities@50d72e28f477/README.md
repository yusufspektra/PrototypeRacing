# MainThreadDispatcher
It's a simple main thread dispatcher script to exit from an other thread. Works with Editor and Runtime. For using just call MainThreadDispatcher.Enqueue(() => {}); . It's need to initialize on runtime by call MainThreadDispatcher.InitForRuntime(). If You don't call this function, then it will trigger automatically when you first call MainThreadDispatcher.Enqueue method.

# SingletonComponent
A generic Singleton solution for components, scriptable objects and spawnable resources prefabs.
Usages:
 - public class XXXX : SingletonComponent<XXXX> // For components
 - public class XXXX : SingletonResourcesPrefabComponent<XXXX> // For spawnable resources prefabs
 - public class XXXX : ScriptableObjectSingletonComponent<XXXX> // For scriptable objects

# CSCHelperForEditor
You can add or remove symbols dor csc.rsp file with this helper. Available methods:
- CSCHelperForEditor.TryAddSymbols(List<string> symbols) // Add given symbols from csc
- CSCHelperForEditor.TryAddSymbol(string symbol) // Add given single symbol from csc
- CSCHelperForEditor.TryRemoveSymbols(List<string> symbols) // Remove given symbols from csc
- CSCHelperForEditor.TryRemoveSymbol(string symbol) // Remove given symbol from csc
- CSCHelperForEditor.RemoveAllSymbols(string symbol) // Remove all symbols from csc
- CSCHelperForEditor.HasSymbol(string symbol) // Check csc file has given symbol
- CSCHelperForEditor.GetCurrentSymbols() // Get all symbols in csc
- CSCHelperForEditor.RefreshEditor() // After any change happen for csc file, you should call this function to refresh editor

# Extensions

# Helpers
## 1. Component
## 2. Editor
## 3. File
## 4. GameObject
## 5. Json
## 6. Math
## 7. Other
## 8. PrimitiveType
## 9. Timer
A simple timer class for counting time. It's useful for waiting some time in a UniTask. It's a simple class, so you can't pause or resume it. It's just for counting time. 
You can give start and end time with seconds. Just register your timer and it will start automatically. You can register some Actions like onStarted, onUpdated and onCompleted.

```csharp
        Helpers.Timer timer = new Helpers.Timer(100); // for 100 seconds
        
        timer.onStarted += () =>
        {
            Debug.Log("Timer Started");
        };
        
        timer.onUpdated += (long currentTime) =>
        {
            Debug.Log("Timer Updated: " + currentTime);
        };
        
        timer.onCompleted += () =>
        {
            Debug.Log("Timer Completed");
        };
        
        Helpers.Time.RegisterTimer(timer);
```
       
## 10. Transform
