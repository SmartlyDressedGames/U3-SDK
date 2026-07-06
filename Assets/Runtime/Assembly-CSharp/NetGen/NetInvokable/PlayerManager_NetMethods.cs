#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
#define LOG_INVOKE_READ_ERRORS
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG_NETINVOKABLES
using SDG.NetPak;
namespace SDG.Unturned
{
	[NetInvokableGeneratedClass(typeof(PlayerManager))]
	public static class PlayerManager_NetMethods
	{
		// ReceivePlayerStates read will be called directly.
		// ReceivePlayerStates write will be called directly.
	}
}
