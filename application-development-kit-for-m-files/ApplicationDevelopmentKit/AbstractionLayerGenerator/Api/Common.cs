using System;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public static class Common
	{
		public static string Normalize_to_cs_type(this string Name, bool isValueList = false)
		{
			if (String.IsNullOrWhiteSpace(Name))
				return Name;

			string normalized_name = "";
			if (isValueList)
				normalized_name += char.IsDigit(Name[0]) ? "_" : "";

			Name = Name.Replace("-", "_Hyphen_");
			Name = Name.Replace("%", "_Percentage_");
			Name = Name.Replace("?", "_QuestionMark_");
			Name = Name.Replace(".", "_dot_");
			Name = Name.Replace(" ", "_");

			foreach (char c in Name) {
				if (char.IsLetterOrDigit(c) || c == '_')
					normalized_name += c;
			}

			return normalized_name;
		}
		public static string Return_type(PropertyDefinition pd)
		{
			string return_type = String.Empty;

			switch (pd.DataType) {
			case MFDataType.MFDatatypeBoolean:
				return_type = "bool?";
				break;
			case MFDataType.MFDatatypeInteger:
			case MFDataType.MFDatatypeInteger64:
				return_type = "int";
				break;
			case MFDataType.MFDatatypeFloating:
				return_type = "double";
				break;
			case MFDataType.MFDatatypeDate:
			case MFDataType.MFDatatypeTime:
			case MFDataType.MFDatatypeTimestamp:
				return_type = "DateTime";
				break;
			case MFDataType.MFDatatypeText:
			case MFDataType.MFDatatypeMultiLineText:
				return_type = "string";
				break;
			case MFDataType.MFDatatypeLookup:
				if (pd.IsBasedOnValueList)
					return_type = $"VL_{pd.ValueListName.Normalize_to_cs_type()}?";
				else
					return_type = $"OT_{pd.ValueListName.Normalize_to_cs_type()}";
				break;
			case MFDataType.MFDatatypeMultiSelectLookup:
				if (pd.IsBasedOnValueList)
					return_type = $"List<VL_{pd.ValueListName.Normalize_to_cs_type()}?>";
				else
					return_type = $"List<OT_{pd.ValueListName.Normalize_to_cs_type()}>";
				break;
			case MFDataType.MFDatatypeFILETIME:
			case MFDataType.MFDatatypeUninitialized:
			case MFDataType.MFDatatypeACL:
				return_type = "object";
				break;
			}

			return return_type;
		}
		public static string Method_name(PropertyDefinition pd)
		{
			string method_name = String.Empty;

			switch (pd.DataType) {
			case MFDataType.MFDatatypeText:
			case MFDataType.MFDatatypeMultiLineText:
				method_name = $"objVerEx.GetPropertyText";
				break;
			case MFDataType.MFDatatypeBoolean:
				method_name = $"objVerEx.GetPropertyBooleanValue";
				break;
			case MFDataType.MFDatatypeInteger:
				method_name = $"objVerEx.GetPropertyIntValue";
				break;
			case MFDataType.MFDatatypeFloating:
				method_name = "objVerEx.GetPropertyDoubleValue";
				break;
			case MFDataType.MFDatatypeDate:
			case MFDataType.MFDatatypeTime:
			case MFDataType.MFDatatypeTimestamp:
				method_name = "objVerEx.GetProperyDateTimeValue";
				break;
			case MFDataType.MFDatatypeLookup:
				if (pd.IsBasedOnValueList)
					method_name = $"objVerEx.GetSingleSelectValueListForPropertyDef<VL_{pd.ValueListName.Normalize_to_cs_type()}>";
				else
					method_name = $"this.GetSingleSelectLookupValueForPropertyDefCache<OT_{pd.ValueListName.Normalize_to_cs_type()}>";
				break;
			case MFDataType.MFDatatypeMultiSelectLookup:
				if (pd.IsBasedOnValueList)
					method_name = $"objVerEx.GetMultiSelectValueListForPropertyDef<VL_{pd.ValueListName.Normalize_to_cs_type()}>";
				else
					method_name = $"this.GetMultiSelectLookupValueForPropertyDefCache<OT_{pd.ValueListName.Normalize_to_cs_type()}>";
				break;
			case MFDataType.MFDatatypeInteger64:
			case MFDataType.MFDatatypeFILETIME:
			case MFDataType.MFDatatypeUninitialized:
			case MFDataType.MFDatatypeACL:
				method_name = "objVerEx.GetValue(id);";
				break;
			}

			return method_name;
		}
		public static string Convert_method(PropertyDefinition pd)
		{
			string method_name = String.Empty;

			switch (pd.DataType) {
			case MFDataType.MFDatatypeText:
			case MFDataType.MFDatatypeMultiLineText:
				method_name = "ConvertPropertyText";
				break;
			case MFDataType.MFDatatypeBoolean:
				method_name = "ConvertPropertyBoolValue";
				break;
			case MFDataType.MFDatatypeInteger:
				method_name = "ConvertPropertyIntValue";
				break;
			case MFDataType.MFDatatypeFloating:
				method_name = "ConvertPropertyDoubleValue";
				break;
			case MFDataType.MFDatatypeDate:
			case MFDataType.MFDatatypeTime:
			case MFDataType.MFDatatypeTimestamp:
				method_name = "ConvertProperyDateTimeValue";
				break;
			case MFDataType.MFDatatypeLookup:
				if (pd.IsBasedOnValueList)
					method_name = $"ConvertValueListToLookup";
				else
					method_name = $"ConvertObjectToLookup";
				break;
			case MFDataType.MFDatatypeMultiSelectLookup:
				if (pd.IsBasedOnValueList)
					method_name = $"ConvertValueListToLookups";
				else
					method_name = $"ConvertObjectToLookups";
				break;
			case MFDataType.MFDatatypeInteger64:
			case MFDataType.MFDatatypeFILETIME:
			case MFDataType.MFDatatypeUninitialized:
			case MFDataType.MFDatatypeACL:
				method_name = "ConvertValue;";
				break;
			}

			return method_name;
		}
	}
}
