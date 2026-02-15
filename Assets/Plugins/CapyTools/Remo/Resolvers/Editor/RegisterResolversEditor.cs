using UnityEditor;
using MessagePack;
using MessagePack.Resolvers;
namespace CapyTools.RemoteEditor.Serialization
{
    public static partial class SerializationConfig
    {
        static bool serializerRegistered = false;
        [InitializeOnLoadMethod]
        static void EditorInitialize()
        {
            if (!serializerRegistered)
            {
                StaticCompositeResolver.Instance.Register(
                     CapyTools.RemoteEditor.Serialization.Resolvers.GeneratedResolver.Instance,
                     MessagePack.Resolvers.StandardResolver.Instance,
                     MessagePack.Unity.UnityResolver.Instance
                );

                var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

                MessagePackSerializer.DefaultOptions = option;
                serializerRegistered = true;
            }
        }
    }
}