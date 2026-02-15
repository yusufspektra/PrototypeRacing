using System.Collections.Generic;
using UnityEngine;

namespace SpektraGames.RuntimeUI.Runtime
{
    public partial class RuntimeUI
    {
        public class BlockRequest
        {
        }

        private static BlockScreenBehaviour CurrentBlockScreenBehaviour = null;
        private static Stack<BlockRequest> _blockRequests = new Stack<BlockRequest>();

        public static bool IsBlockScreenActive =>
            CurrentBlockScreenBehaviour != null && CurrentBlockScreenBehaviour.IsActive;

        public static void BlockScreen(BlockRequest request = null)
        {
            if (!CanCallRuntimeMethod)
                return;

            request ??= new BlockRequest();
            _blockRequests.Push(request);

            CheckRequests();
        }

        public static void UnblockScreen()
        {
            if (!CanCallRuntimeMethod || _blockRequests.Count <= 0)
                return;

            _blockRequests.Pop();
            CheckRequests();
        }

        public static void UnblockAll()
        {
            if (!CanCallRuntimeMethod || _blockRequests.Count <= 0)
                return;

            _blockRequests.Clear();
            CheckRequests();
        }

        private static void CheckRequests()
        {
            if (_blockRequests.Count > 0)
            {
                if (!IsBlockScreenActive)
                {
                    BlockCanvas(true);
                    CurrentBlockScreenBehaviour?.Show();


                    // Make the loading in front of the all other behaviours
                    if (CurrentBlockScreenBehaviour != null)
                        CurrentBlockScreenBehaviour.transform.SetAsLastSibling();
                }
            }
            else
            {
                if (IsBlockScreenActive)
                {
                    CurrentBlockScreenBehaviour?.Hide();
                    BlockCanvas(false);
                }
            }
        }

        private static void BlockCanvas(bool value)
        {
            // if (value)
            // {
            //     if (RuntimeUISettings.Instance.CanvasSortOrder < 1000)
            //         _canvas.sortingOrder = 1000;
            // }
            // else
            // {
            //     _canvas.sortingOrder = RuntimeUISettings.Instance.CanvasSortOrder;
            // }
        }
    }
}
