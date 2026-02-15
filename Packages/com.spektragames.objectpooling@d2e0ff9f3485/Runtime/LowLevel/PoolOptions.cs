using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.ObjectPooling.Runtime
{
    [Flags]
    public enum PoolOptions
    {
        None = 0,
        TriggerPoolCallbacks = 1 << 0
    }
}