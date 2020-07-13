//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using System.Collections.Generic;

using MFiles.VAF.Common;
using MFilesAPI;

namespace VAF
{
	public interface IObjVerEx
	{
		ObjVerEx objVerEx { get; set; }
		Dictionary<int,object> object_properties_cache { get; set; }
	}

	public interface IMFPropertyDefinition 
	{
		int id { get;}
		MFDataType data_type { get; }
	}

	public class SearchParameters
	{
		public SearchParameters()
		{
			Conditions = new SearchConditions();
		}
		public SearchConditions Conditions { get; set; }

		public SearchParameters AddProperty<T>(object value, MFConditionType cType = MFConditionType.MFConditionTypeEqual)
			where T : IMFPropertyDefinition
		{
			Conditions.AddPropertyCondition<T>(value, cType);
			return this;
		}
	}
}