using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.AddressableLoader.Runtime
{
    [System.Serializable]
    public struct LoadResponse<T> where T : UnityEngine.Object
    {
        private bool _isSuccess;
        private T _result;
        private ProvidedAsset _providedAssetHandle;

        public bool IsSuccess => _isSuccess;
        public T Result => _result;
        public ProvidedAsset ProvidedAssetHandle => _providedAssetHandle;

        public LoadResponse(bool isSuccess, T result, ProvidedAsset providedAssetHandle)
        {
            this._isSuccess = isSuccess;
            this._result = result;
            this._providedAssetHandle = providedAssetHandle;
        }
    }
}