using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SporeMods.Mods.Identity.V1
{
	public class MutualExclusiveComponentGroup : BaseModComponent
	{
		static bool HANDLE_IS_ENABLED = true;
		
		
		public MutualExclusiveComponentGroup(ModIdentity identity, string uniqueTag) : base(identity, uniqueTag)
		{
			ModComponentIsEnabledChanged += (sneder, e) =>
			{
				if (HANDLE_IS_ENABLED && SubComponents.Contains(e.Component) && e.Component.IsEnabled)
				{
					HANDLE_IS_ENABLED = false;


					var subComponents = SubComponents.Where(x => x != e.Component).ToList();
					foreach (BaseModComponent cp in subComponents)
					{
						cp.IsEnabled = false;
					}

					HANDLE_IS_ENABLED = true;
				}
			};
		}
	}
}
