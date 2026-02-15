using UnityEngine;
using MessagePack;
using MessagePack.Resolvers;
namespace CapyTools.RemoteEditor.Serialization
{
    [DefaultExecutionOrder(-99999)]
    public static partial class SerializationConfig
    {
        static bool serializerRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Initialize()
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
