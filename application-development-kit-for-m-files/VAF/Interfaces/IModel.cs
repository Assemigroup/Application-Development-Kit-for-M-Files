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
		public SearchParameters AddPDNullCheck<T>(bool is_null = true)
			where T : IMFPropertyDefinition
		{
			int pd_id = (int)typeof(T).GetField("id").GetRawConstantValue();
			MFDataType data_type = (MFDataType)typeof(T).GetField("data_type").GetRawConstantValue();
			return this.AddPDNullCheck(pd_id, data_type, is_null);
		}
		public SearchParameters AddPDNullCheck(int property_def, MFDataType dataType, bool is_null = true)
		{
			TypedValue value = new TypedValue();
			value.SetValueToNULL(dataType);
			SearchCondition search = new SearchCondition();
			search.Expression.DataPropertyValuePropertyDef = property_def;
			Expression exp = new Expression();
			exp.SetPropertyValueExpression(property_def, MFParentChildBehavior.MFParentChildBehaviorNone);
			search.ConditionType = is_null ? MFConditionType.MFConditionTypeEqual : MFConditionType.MFConditionTypeNotEqual;
			search.Set(exp, search.ConditionType, value);
			this.Conditions.Add(1, search);
			return this;
		}
	}
}