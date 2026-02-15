# Object Pooling

## Overview
The Object Pooling System is designed to efficiently manage game object instances, reducing instantiation overhead and improving performance by reusing objects. This system allows for pooling of any type of GameObject using the PoolEnum to define different object types and provides callbacks for initialization and reset actions on pooled objects.

Tools - Object Pooling - ObjectPoolingEditor

---

## Table of Contents

1. **Pooling System Overview**
2. **Usage**
    - Getting objects from the pool
    - Returning objects to the pool
    - Clearing pools
3. **Advanced Pool Management**
    - Pool merging
    - Handling callbacks
4. **Pool Item Creation**
5. **API Reference**
    - `PoolManager`
    - `ObjectPool`
    - `PoolItem`
    - `PoolEnum`
    - `PoolOptions`
    - `IPoolCallbacks`
6. **Customization and Extensions**

---

## 1. Pooling System Overview

The system consists of several components:

- **PoolManager**: Central class that manages all object pools.
- **ObjectPool**: Manages individual object pools, handles instantiation, reuse, and return of `GameObject` instances.
- **PoolEnum**: A custom enum-like class used to define different types of object pools.
- **PoolItem**: A ScriptableObject that holds configuration for each pool, such as initial pool size, prefab, and maximum pool size.
- **IPoolCallbacks**: An interface for handling object initialization and reset callbacks when objects are pulled from or returned to the pool.

---

## 2. Usage

### Getting Objects from the Pool

You can request an object from the pool by using the `PoolManager.Get(PoolEnum poolEnum)` method.

```csharp
GameObject enemy = PoolManager.Get(EnemyPool.Zombie);
```
This method retrieves an available object from the specified pool or creates a new pool if one does not exist.

### Returning Objects to the Pool
Once you're done with a pooled object, it should be returned to the pool using `PoolManager.Return(GameObject poolObject, PoolEnum poolEnum)` method.
```csharp
PoolManager.Return(enemy, EnemyPool.Zombie);
```

### Clearing Pools
You can clear a single pool, a pool category, or all pools using the following methods:
- `PoolManager.Clear(PoolEnum poolEnum)` – Clears a single pool.
- `PoolManager.ClearCategory(string poolCategory)` – Clears all pools of a specific category.
- `PoolManager.ClearAll()` – Clears all pools and destroys all pooled objects.

---

## 3. Advanced Pool Management

### Pool Merging

When loading new scenes, existing pools may need to be merged. The `PoolManager` provides automatic pool merging when new scenes are loaded.

`PoolManager.MergeAllPools()` combines pools with the same `PoolEnum` and destroys duplicates to ensure only one pool per object type exists across scenes.

### Handling Callbacks

If your `PoolItem` is configured to use callbacks, you can handle custom initialization and reset behavior for pooled objects by implementing the `IPoolCallbacks` interface.

```csharp
public class MyPooledObject : MonoBehaviour, IPoolCallbacks
{
    public void OnPoolObjectInitialize()
    {
        // Custom behavior when object is taken from pool
    }

    public void OnPoolObjectReset()
    {
        // Custom behavior when object is returned to pool
    }
}
```
Enable callbacks by setting the activatePoolCallbacks field in the PoolItem ScriptableObject.

---

## 4. Pool Item Creation

### Object Pool Editor

Object Pool Editor Menu can be accessed from `Tools - Object Pooling - ObjectPoolingEditor`. Pool items can be configured in this menu.

![ObjectPoolingEditor](Images/ObjectPoolingEditor.png)

![ObjectPoolingItem](Images/ObjectPoolingItem.png)

Each pool is defined using the `PoolItem` ScriptableObject. The following fields are available for configuration:

- **Prefab**: The `GameObject` to be pooled.
- **Initial Pool Size**: The number of objects to create at startup.
- **Pool Increase Size**: How many objects to create when the pool is empty.
- **Max Pool Size**: Maximum number of objects the pool can hold (0 = no limit).
- **Activate Callbacks**: Enables or disables pooling callbacks (`IPoolCallbacks`).

---

## 5. API Reference

### `PoolManager`

- **Get(PoolEnum poolEnum)**: Retrieves a `GameObject` from the pool.
- **Return(GameObject poolObject, PoolEnum poolEnum)**: Returns a `GameObject` to the pool.
- **Clear(PoolEnum poolEnum)**: Clears and destroys a specific pool.
- **ClearCategory(string poolCategory)**: Clears all pools within a specific category.
- **ClearAll()**: Clears and destroys all pools.
- **MergeAllPools()**: Merges duplicate pools when scene changes occur.

### `ObjectPool`

- **Initialize(PoolEnum poolEnum)**: Initializes a pool with the specified enum.
- **Get(PoolOptions options)**: Retrieves an object from the pool.
- **Return(GameObject poolItem, PoolOptions options)**: Returns an object to the pool.
- **Clear()**: Clears all objects from the pool.
- **CachePoolObject(GameObject poolObject)**: Stores a reference to active objects for callback management.

### `PoolEnum`

- A custom enum-like class used to define and differentiate between object types.

### `PoolItem`

- **Prefab**: The object to instantiate and pool.
- **Initial Pool Size**: Number of objects created when the pool is initialized.
- **Pool Increase Size**: Number of objects to instantiate when the pool runs out.
- **Max Pool Size**: The maximum number of objects the pool can hold.

### `PoolOptions` (Flags Enum)

- **None**: No specific options.
- **TriggerPoolCallbacks**: Triggers the `IPoolCallbacks` methods on the object.
- **CheckWasInPool**: Ensures the object was part of the pool.

### `IPoolCallbacks`

- **OnPoolObjectReset()**: Called when the object is returned to the pool.
- **OnPoolObjectInitialize()**: Called when the object is retrieved from the pool.

---

## 6. Customization and Extensions

You can easily extend this system by creating new pool types using `PoolEnum`, configuring more `PoolItem` ScriptableObjects, and implementing custom behavior using the `IPoolCallbacks` interface.

---
