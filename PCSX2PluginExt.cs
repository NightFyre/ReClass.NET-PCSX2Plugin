
using System.Drawing;
using ReClassNET.Plugins;

namespace PCSX2Plugin
{
	public class PCSX2PluginExt : Plugin
	{
		public override Image Icon => Properties.Resources.logo_pcsx2;

		public override CustomNodeTypes GetCustomNodeTypes()
		{
			return new CustomNodeTypes
			{
				CodeGenerator = new PS2PtrCodeGenerator(),
				Serializer = new PS2PtrNodeConverter(),
				NodeTypes = new[] { typeof(EEMemNode), typeof(PS2PtrNode) }
			};
		}
	}
}
