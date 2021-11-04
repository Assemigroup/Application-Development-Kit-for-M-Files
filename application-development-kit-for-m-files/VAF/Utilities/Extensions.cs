//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using MFilesAPI;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;

namespace VAF
{
	public static class Extensions
	{
		public static List<T> GetIndirectReferenceByObject<T>(this ObjVerEx objVerEx, int object_id, bool include_deleted = false)
			where T : IObjVerEx, new()
		{
			return objVerEx.GetMultiSelectLookupValueForObject<T>(object_id, include_deleted);
		}

		public static List<T> GetMultiSelectLookupValueForObject<T>(this ObjVerEx objVerEx, int object_id, bool include_deleted = false)
			where T : IObjVerEx, new()
		{
			List<T> Items = new List<T>();
			Lookups lookups = objVerEx.GetIndirectReferences(null, object_id).ToLookups();

			foreach (Lookup lookup in lookups) {
				if (lookup.Deleted && !include_deleted)
					continue;

				ObjVerEx objVEx = new ObjVerEx(objVerEx.Vault, lookup.ObjectType, lookup.Item, lookup.Version);
				T classObj = new T() { objVerEx = objVEx };

				if (null == classObj)
					continue;

				if (!Items.Contains(classObj))
					Items.Add(classObj);
			}

			return Items;
		}

		public static List<T> GetMultiSelectLookupValueForPropertyDef<T>(this ObjVerEx objVerEx, int property_def_id, bool include_deleted = false)
			where T : IObjVerEx, new()
		{
			if (!objVerEx.HasValue(property_def_id)) // Direct References Exist
				return new List<T>();

			List<T> Items = new List<T>();
			Lookups lookups = objVerEx.GetProperty(property_def_id).Value.GetValueAsLookups();

			foreach (Lookup lookup in lookups) {
				if (lookup.Deleted && !include_deleted)
					continue;

				ObjVerEx objVEx = new ObjVerEx(objVerEx.Vault, lookup.ObjectType, lookup.Item, lookup.Version);
				T classObj = new T() { objVerEx = objVEx };

				if (null == classObj)
					continue;

				if (!Items.Contains(classObj))
					Items.Add(classObj);
			}

			return Items;
		}

		public static T GetSingleSelectLookupValueForPropertyDef<T>(this ObjVerEx objVerEx, int property_def_id, bool include_deleted = false)
			where T : IObjVerEx, new()
		{
			if (!objVerEx.HasValue(property_def_id))
				return default(T);

			Lookup lookup = objVerEx.GetProperty(property_def_id).Value.GetValueAsLookup();
			if (lookup.Deleted && !include_deleted)
				return default(T);

			ObjVerEx objVEx = new ObjVerEx(objVerEx.Vault, lookup.ObjectType, lookup.Item, lookup.Version);
			return new T() { objVerEx = objVEx };
		}
		public static List<T?> GetMultiSelectValueListForPropertyDef<T>(this ObjVerEx objVerEx, int id, bool include_deleted = false)
			where T : struct
		{
			if (!typeof(T).IsEnum || !objVerEx.HasValue(id))
				return new List<T?>();

			Lookups lookups = objVerEx.GetLookups(id);
			if (lookups.Count == 0)
				return null;

			List<T?> Items = new List<T?>();
			foreach (Lookup lookup in lookups) {
				if (lookup.Deleted && !include_deleted)
					continue;
				Items.Add((T)Enum.Parse(typeof(T), lookup.Item.ToString()));
			}

			return Items;
		}

		public static T? GetSingleSelectValueListForPropertyDef<T>(this ObjVerEx objVerEx, int id)
			where T : struct
		{
			if (!typeof(T).IsEnum || !objVerEx.HasProperty(id))
				return default(T);

			PropertyValue prop = objVerEx.GetProperty(id);
			if (prop == null)
				return null;

			Lookup lookup = prop.Value.GetValueAsLookup();
			if (lookup == null || lookup.Deleted)
				return null;

			return (T)Enum.Parse(typeof(T), lookup.Item.ToString());
		}

		public static T GetSingleSelectLookupValueForPropertyDefCache<T>(this IObjVerEx ot, int property_id) where T : class, IObjVerEx, new()
		{
			if (ot.object_properties_cache.ContainsKey(property_id))
				return ot.object_properties_cache[property_id] as T;
			T pd_value = ot.objVerEx.GetSingleSelectLookupValueForPropertyDef<T>(property_id);
			ot.CacheProperty(property_id, pd_value);
			return pd_value;
		}

		public static void CacheProperty(this IObjVerEx obj, int property_id, object value)
		{
			if (obj.object_properties_cache.ContainsKey(property_id)) {
				obj.object_properties_cache[property_id] = value;
				return;
			}
			obj.object_properties_cache.Add(property_id, value);
		}
		public static void RemoveCachedProperty(this IObjVerEx obj, int property_id)
		{
			if (obj.object_properties_cache.ContainsKey(property_id))
				obj.object_properties_cache.Remove(property_id);
		}
		public static void ClearAllCachedProperties(this IObjVerEx obj)
		{
			obj.object_properties_cache.Clear();
		}
		public static PropertyValue ConvertObjectToLookup<T>(object id, MFDataType data_type, T objClass)
			where T : IObjVerEx
		{
			if (null == objClass) {
				PropertyValue pv = new PropertyValue() { PropertyDef = (int)id };
				pv.TypedValue.SetValueToNULL(data_type);
				return pv;
			}

			Lookup lookup = new Lookup() {
				Item = objClass.objVerEx.ID,
				ObjectType = objClass.objVerEx.Type
			};

			PropertyValue propVal = new PropertyValue();
			propVal.TypedValue.SetValueToLookup(lookup);
			propVal.PropertyDef = (int)id;
			return propVal;
		}

		public static PropertyValue ConvertObjectToLookups<T>(object id, MFDataType data_type, List<T> objClass)
			where T : IObjVerEx
		{
			if (null == objClass) {
				PropertyValue pv = new PropertyValue() { PropertyDef = (int)id };
				pv.TypedValue.SetValueToNULL(data_type);
				return pv;
			}

			Lookups lookups = new Lookups();
			foreach (T t in objClass) {
				if (null == t)
					continue;
				Lookup lookup = new Lookup() {
					Item = t.objVerEx.ID,
					ObjectType = t.objVerEx.Type
				};

				if (lookups.GetLookupIndexByItem(lookup.Item) == -1)
					lookups.Add(-1, lookup);
			}

			PropertyValue propVal = new PropertyValue();
			propVal.TypedValue.SetValueToMultiSelectLookup(lookups);
			propVal.PropertyDef = (int)id;
			return propVal;
		}

		public static PropertyValue ConvertValueListToLookup<T>(object id, MFDataType data_type, T? vList)
			where T : struct
		{
			if (!typeof(T).IsEnum || null == vList) {
				PropertyValue pv = new PropertyValue() { PropertyDef = (int)id };
				pv.TypedValue.SetValueToNULL(data_type);
				return pv;
			}

			string value_list_name = typeof(T).Name?.Replace("VL", "");
			if (!string.IsNullOrEmpty(value_list_name) && !char.IsDigit(value_list_name[1]))
				value_list_name = value_list_name.Substring(1, value_list_name.Length - 1);

			int ObjectType = (int)Enum.Parse(typeof(VL_ValueLists), value_list_name, true);

			Lookup lookup = new Lookup() {
				Item = Convert.ToInt32(vList),
				ObjectType = ObjectType
			};

			PropertyValue propVal = new PropertyValue() { PropertyDef = (int)id };
			propVal.TypedValue.SetValueToLookup(lookup);
			return propVal;
		}

		public static PropertyValue ConvertValueListToLookups<T>(object id, MFDataType data_type, List<T?> vList)
			where T : struct
		{
			if (!typeof(T).IsEnum || vList == null) {
				PropertyValue pv = new PropertyValue() { PropertyDef = (int)id };
				pv.TypedValue.SetValueToNULL(data_type);
				return pv;
			}

			Lookups lookups = new Lookups();
			foreach (T t in vList) {
				if (Equals(t, default(T)))
					continue;

				string value_list_name = typeof(T).Name.Replace("VL", "");
				if (!string.IsNullOrEmpty(value_list_name) && !char.IsDigit(value_list_name[1]))
					value_list_name = value_list_name.Substring(1, value_list_name.Length - 1);

				int ObjectType = (int)Enum.Parse(typeof(VL_ValueLists), value_list_name, true);

				Lookup lookup = new Lookup() {
					Item = Convert.ToInt32(t),
					ObjectType = ObjectType
				};

				if (lookups.GetLookupIndexByItem(lookup.Item) == -1)
					lookups.Add(-1, lookup);
			}

			PropertyValue propVal = new PropertyValue() { PropertyDef = (int)id };
			propVal.TypedValue.SetValueToMultiSelectLookup(lookups);
			return propVal;
		}

		public static PropertyValue ConvertPropertyText(object id, MFDataType data_type, string value)
		{
			PropertyValue pv = new PropertyValue() {
				PropertyDef = (int)id
			};

			if (null == value)
				pv.TypedValue.SetValueToNULL(data_type);
			else
				pv.TypedValue.SetValue(data_type, value);

			return pv;
		}

		public static PropertyValue ConvertProperyDateTimeValue(object id, MFDataType data_type, DateTime? value)
		{
			PropertyValue pv = new PropertyValue() {
				PropertyDef = (int)id
			};

			if (null == value)
				pv.TypedValue.SetValueToNULL(data_type);
			else
				pv.TypedValue.SetValue(data_type, value);

			return pv;
		}

		public static PropertyValue ConvertPropertyDoubleValue(object id, MFDataType data_type, double? value)
		{
			PropertyValue pv = new PropertyValue() {
				PropertyDef = (int)id
			};

			if (null == value)
				pv.TypedValue.SetValueToNULL(data_type);
			else
				pv.TypedValue.SetValue(data_type, value);

			return pv;
		}

		public static PropertyValue ConvertPropertyIntValue(object id, MFDataType data_type, int? value)
		{
			PropertyValue pv = new PropertyValue() {
				PropertyDef = (int)id
			};

			if (null == value)
				pv.TypedValue.SetValueToNULL(data_type);
			else
				pv.TypedValue.SetValue(data_type, value);

			return pv;
		}

		public static PropertyValue ConvertPropertyBoolValue(object id, MFDataType data_type, bool? value)
		{
			PropertyValue pv = new PropertyValue() {
				PropertyDef = (int)id
			};

			if (null == value)
				pv.TypedValue.SetValueToNULL(data_type);
			else
				pv.TypedValue.SetValue(data_type, value);

			return pv;
		}

		public static bool? GetPropertyBooleanValue(this ObjVerEx objVerEx, int id)
		{
			if (objVerEx.HasProperty(id)) {
				PropertyValue pv = objVerEx.GetProperty(id);
				if (!pv.Value.IsNULL())
					return (bool)objVerEx.GetProperty(id).Value.Value;
			}

			return null;
		}

		public static double GetPropertyDoubleValue(this ObjVerEx objVerEx, int id)
		{
			if (objVerEx.HasProperty(id)) {
				PropertyValue pv = objVerEx.GetProperty(id);
				if (!pv.Value.IsNULL())
					return (double)objVerEx.GetProperty(id).Value.Value;
			}

			return 0.0;
		}

		public static int GetPropertyIntValue(this ObjVerEx objVerEx, int id)
		{
			if (objVerEx.HasProperty(id)) {
				PropertyValue pv = objVerEx.GetProperty(id);
				if (!pv.Value.IsNULL())
					return (int)objVerEx.GetProperty(id).Value.Value;
			}

			return 0;
		}

		public static DateTime GetProperyDateTimeValue(this ObjVerEx objVerEx, int id)
		{
			if (objVerEx.HasProperty(id)) {
				PropertyValue pv = objVerEx.GetProperty(id);
				if (!pv.Value.IsNULL())
					return (DateTime)objVerEx.GetProperty(id).Value.Value;
			}

			return DateTime.MinValue;
		}

		public static bool HasUniqueProperty<T>(this IObjVerEx obj, int property_def, T? value)
			where T : struct
		{
			if (null == value)
				return true;

			PropertyValue prop = obj.objVerEx.GetProperty(property_def);
			if (prop.TypedValue.IsNULL())
				return true;

			Lookup lookup = prop.Value.GetValueAsLookup();
			if (lookup == null || lookup.Deleted)
				return true;

			List<ObjVerEx> result = SearchByPropertyDef(obj.objVerEx, property_def, value);
			if (result != null && result.Count > 0) {
				foreach (ObjVerEx objVerEx in result) {
					PropertyValue property_def_value = objVerEx.GetProperty(property_def);
					if (!property_def_value.TypedValue.IsNULL()
						&& objVerEx.ObjID.ID != obj.objVerEx.ID
						&& property_def_value.TypedValue.DisplayValue == lookup.DisplayValue)
						return false;
				}
			}

			return true;
		}

		public static List<ObjVerEx> SearchByPropertyDef<T>(ObjVerEx objVerEx, int property_def, T? value)
			where T : struct
		{
			SearchConditions searchConditions = new SearchConditions();

			SearchCondition searchCondition;
			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(property_def, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, value);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
			searchConditions.Add(-1, searchCondition);

			ObjectSearchResults objectSearchResults =
				objVerEx.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions, MFSearchFlags.MFSearchFlagNone, false);

			List<ObjVerEx> searchedOT = null;
			if (objectSearchResults != null && objectSearchResults.Count > 0) {
				searchedOT = new List<ObjVerEx>();
				IEnumerator searchResultObjVerEnum = objectSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				while (searchResultObjVerEnum.MoveNext()) {
					objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
					ObjectVersionAndProperties objVerAndProps =
						objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, true);
					ObjVerEx item = new ObjVerEx(objVerEx.Vault, objVerAndProps);
					searchedOT.Add(item);
				}
			}
			return searchedOT;
		}

		public static bool HasChanged(this IObjVerEx obj, int id)
		{
			// first version?
			if (obj.objVerEx.Version == 1)
				return false;

			if (null == obj.objVerEx.PreviousVersion)
				return false;

			if (!obj.objVerEx.HasProperty(id))
				return false;

			PropertyValue curr = obj.objVerEx.GetProperty(id);
			PropertyValue prev = !obj.objVerEx.PreviousVersion.HasProperty(id) ? null : obj.objVerEx.PreviousVersion.GetProperty(id);
			if (prev == null) {
				prev = new PropertyValue();
				prev.Value.SetValueToNULL(curr.Value.DataType);
			}
			if (curr.Value.IsNULL() && prev.Value.IsNULL())
				return false;
			if (curr.IsEqual(prev, EqualityCompareOptions.Default))
				return false;

			return true;
		}

		public static bool Deleted(this ObjVerEx objVerEx)
		{
			return objVerEx.Info.Deleted;
		}

		public static Lookups ToLookups(this List<ObjVerEx> objVerEx)
		{
			Lookups lookups = new Lookups();

			foreach (ObjVerEx obj in objVerEx) {
				Lookup lu = new Lookup {
					ObjectType = obj.Type,
					Item = obj.ID,
					Version = -1
				};

				if (lookups.GetLookupIndexByItem(lu.Item) == -1)
					lookups.Add(-1, lu);
			}

			return lookups;
		}

		public static Lookup ToLookup(this List<ObjVerEx> objVerEx)
		{
			Lookup lookup = new Lookup();

			for (int i = 0; i < objVerEx.Count; i++) {
				lookup.ObjectType = objVerEx[i].Type;
				lookup.Item = objVerEx[i].ID;
				lookup.Version = -1;
			}

			return lookup;
		}

		public static Lookup ToLookup(this ObjVerEx objVerEx)
		{
			return ToLookup(objVerEx.ObjVer);
		}

		public static Lookup ToLookup(ObjVer objVer)
		{
			return new Lookup {
				ObjectType = objVer.Type,
				Item = objVer.ID,
				Version = -1
			};
		}

		public static bool GetYesOrNoAsBool(this ObjVerEx targetEx, MFIdentifier prop, out bool value)
		{
			value = false;

			// if the object doesn't have the property, return null
			if (!targetEx.HasProperty(prop)) {
				return false;
			}

			// get the text version of the property
			var propText = targetEx.GetPropertyText(prop);

			// if the text version of the property is "Yes", return true
			if (propText.Equals("Yes")) {
				value = true;
				return true;
			}

			// if the text version of the property is "No", return false
			if (propText.Equals("No")) {
				value = false;
				return true;
			}

			// couldn't parse the text value, return null
			return false;
		}

		public static bool GetDouble(this ObjVerEx targetEx, MFIdentifier prop, out double value)
		{
			// set default value
			value = 0.0;

			// check if the property exists on the target
			if (!targetEx.HasProperty(prop)) {
				return false;
			}

			// attempt to get the text value of the property
			var text = targetEx.GetPropertyText(prop);

			// attempt to parse it
			return double.TryParse(text, out value);
		}

		public static string GetGenericTitle(this IObjVerEx obj, string format, int property_def_title_id, int property_def_id)
		{
			string old_property = null;
			string new_property = null;

			if (!obj.objVerEx.HasProperty(property_def_title_id) || !obj.objVerEx.HasProperty(property_def_id))
				return null;

			old_property = obj.objVerEx.PreviousVersion.GetPropertyText(property_def_id);
			old_property = string.IsNullOrWhiteSpace(old_property) ? "" : string.Format(format, old_property);
			new_property = obj.objVerEx.GetPropertyText(property_def_id);
			new_property = string.IsNullOrWhiteSpace(new_property) ? "" : string.Format(format, new_property);

			return obj.GetGenericTitle(property_def_title_id, format, old_property, new_property);
		}

		public static string GetGenericTitle(this IObjVerEx obj, int property_def_title_id, string format, string new_text)
		{
			return obj.GetGenericTitle(property_def_title_id, format, new_text, new_text);
		}

		public static string GetGenericTitle(this IObjVerEx obj, int property_def_title_id, string format, string old_text, string new_text)
		{
			string title = null;
			string old_property = null;
			string new_property = null;

			if (!obj.objVerEx.HasProperty(property_def_title_id) || string.IsNullOrWhiteSpace(format))
				return null;

			title = obj.objVerEx.GetPropertyText(property_def_title_id);
			old_property = string.IsNullOrWhiteSpace(old_text) ? "" : string.Format(format, old_text);
			new_property = string.IsNullOrWhiteSpace(new_text) ? "" : string.Format(format, new_text);

			if (title.EndsWith(old_property))
				title = title.Replace(old_property, new_property);
			else if (!title.EndsWith(new_property))
				title += new_property;

			return title;
		}

		public static bool is_number(this string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return false;

			bool is_number = true;
			foreach (char c in input)
				is_number &= (char.IsDigit(c) || c == '.');

			return is_number;
		}

		public static void Clear(this IObjVerEx obj, object prop, MFDataType data_type)
		{
			PropertyValue pv = new PropertyValue() { PropertyDef = (int)prop };
			pv.TypedValue.SetValueToNULL(data_type);
			obj.objVerEx.SetProperty(pv);
		}

		public static bool IsNull(this IObjVerEx obj, int id)
		{
			if (obj.objVerEx.HasProperty(id)) {
				PropertyValue pv = obj.objVerEx.GetProperty(id);
				if (pv.Value.IsNULL())
					return true;
			}

			return false;
		}

		public static void SetToNull(this IObjVerEx obj, int id, MFDataType data_type)
		{
			obj.RemoveCachedProperty(id);
			PropertyValue pv = new PropertyValue() {
				PropertyDef = id
			};

			pv.TypedValue.SetValueToNULL(data_type);
			obj.objVerEx.SetProperty(pv);
		}

		public static string GetEnumDescription(Enum value)
		{
			try {
				FieldInfo fi = value.GetType().GetField(value.ToString());

				DescriptionAttribute[] attributes =
					(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attributes != null && attributes.Length > 0)
					return attributes[0].Description;
				else
					return value.ToString();
			} catch {
				return null;
			}
		}

		public static T? GetEnumByDescription<T>(string description)
			where T : struct
		{
			var type = typeof(T);
			if (!type.IsEnum)
				return null;
			foreach (var field in type.GetFields()) {
				var attribute = Attribute.GetCustomAttribute(field,
					typeof(DescriptionAttribute)) as DescriptionAttribute;
				if (attribute != null) {
					if (attribute.Description == description)
						return (T)field.GetValue(null);
				} else {
					if (field.Name == description)
						return (T)field.GetValue(null);
				}
			}
			return null;
		}

		public static string GetValueListItemFromVault(this ObjVerEx objVerEx, int id, string ValueListName)
		{
			try {
				PropertyValue prop = objVerEx.GetProperty(id);
				if (prop == null)
					return string.Empty;

				Lookup lookup = prop.Value.GetValueAsLookup();
				if (lookup == null)
					return string.Empty;

				return objVerEx.Vault.ValueListItemOperations.GetValueListItemByDisplayID(
					(int)Enum.Parse(typeof(VL_ValueLists), ValueListName), lookup.Item.ToString()).Name;
			} catch {
				return string.Empty;
			}
		}

		public static double Truncate(double value)
		{
			return Math.Truncate(100 * value) / 100;
		}

		public static void AddDistinct<T>(this List<T> collection, T item)
		{
			if (collection.Contains(item))
				return;

			collection.Add(item);
		}

		public static void Add<T>(this IObjVerEx obj, int propertyId, T value)
			where T : IObjVerEx
		{
			if (obj.objVerEx.HasProperty(propertyId)) {
				PropertyValue propValue = obj.objVerEx.GetProperty(propertyId);
				Lookups lookups = propValue.TypedValue.GetValueAsLookups();

				if (lookups == null)
					lookups = new Lookups();

				Lookup lookupToAdd = new Lookup() {
					Item = value.objVerEx.ID,
					ObjectType = value.objVerEx.Type
				};
				if (lookups.GetLookupIndexByItem(lookupToAdd.Item) == -1)
					lookups.Add(-1, lookupToAdd);

				propValue.TypedValue.SetValueToMultiSelectLookup(lookups);
				obj.objVerEx.SetProperty(propValue);
			}
		}

		public static void Remove<T>(this IObjVerEx obj, int propertyId, T value)
			where T : IObjVerEx
		{
			if (obj.objVerEx.HasProperty(propertyId)) {
				PropertyValue propValue = obj.objVerEx.GetProperty(propertyId);
				Lookups lookups = propValue.TypedValue.GetValueAsLookups();
				int itemIndex = lookups.GetLookupIndexByItem(value.objVerEx.ID);

				if (itemIndex != -1)
					lookups.Remove(itemIndex);
			}
		}

		public static bool Contains<T>(this IObjVerEx obj, int propertyId, T value)
			where T : IObjVerEx
		{
			if (obj.objVerEx.HasProperty(propertyId)) {
				PropertyValue propValue = obj.objVerEx.GetProperty(propertyId);
				Lookups lookups = propValue.TypedValue.GetValueAsLookups();
				int itemIndex = lookups.GetLookupIndexByItem(value.objVerEx.ID);

				return itemIndex != -1;
			}
			return false;
		}

		public static void Add<T>(this IObjVerEx obj, int propertyId, T? value)
			where T : struct
		{
			if (typeof(T).IsEnum && (value != null && value.GetType() == typeof(T))) {
				Lookups lookups = obj.objVerEx.GetLookups(propertyId);

				string value_list_name = typeof(T).Name.Replace("VL", "");
				if (!string.IsNullOrEmpty(value_list_name) && !char.IsDigit(value_list_name[1]))
					value_list_name = value_list_name.Substring(1, value_list_name.Length - 1);

				int ObjectType = (int)Enum.Parse(typeof(VL_ValueLists), value_list_name, true);

				Lookup lookupToAdd = new Lookup() {
					Item = Convert.ToInt32(value),
					ObjectType = ObjectType
				};
				if (lookups.GetLookupIndexByItem(lookupToAdd.Item) == -1)
					lookups.Add(-1, lookupToAdd);

				PropertyValue propValue = new PropertyValue() { PropertyDef = (int)propertyId };
				propValue.TypedValue.SetValueToMultiSelectLookup(lookups);
				obj.objVerEx.SetProperty(propValue);
			}
		}

		public static void Remove<T>(this IObjVerEx obj, int propertyId, T? value)
			where T : struct
		{
			if (typeof(T).IsEnum && (value != null && value.GetType() == typeof(T))) {
				PropertyValue propValue = obj.objVerEx.GetProperty(propertyId);
				Lookups lookups = obj.objVerEx.GetLookups(propertyId);
				int itemIndex = lookups.GetLookupIndexByItem(Convert.ToInt32(value));

				if (itemIndex != -1)
					lookups.Remove(itemIndex);
			}
		}

		public static bool Contains<T>(this IObjVerEx obj, int propertyId, T? value)
			where T : struct
		{
			if (typeof(T).IsEnum && (value != null && value.GetType() == typeof(T))) {
				PropertyValue propValue = obj.objVerEx.GetProperty(propertyId);
				Lookups lookups = obj.objVerEx.GetLookups(propertyId);
				int itemIndex = lookups.GetLookupIndexByItem(Convert.ToInt32(value));

				return itemIndex != -1;
			}
			return false;
		}

		public static bool ContainsPD(this ObjVerEx objVerEx, string propDefName)
		{
			if (string.IsNullOrWhiteSpace(propDefName))
				return false;

			PropertyDef propDef = null;
			foreach (PropertyValue pv in objVerEx.Properties) {
				if (pv.PropertyDef < 1001) // Check if built-in PD
					continue;
				propDef = objVerEx.Vault.PropertyDefOperations.GetPropertyDef(pv.PropertyDef);
				if (propDefName.Equals(propDef.Name, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}

			return false;
		}

		public static ValueListItem CreateValueListItemAdmin(this IObjVerEx obj, VL_ValueLists vlId, string vlItemNameToAdd)
		{
			if (string.IsNullOrWhiteSpace(vlItemNameToAdd))
				return null;

			ValueListItems vlItems = obj.objVerEx.Vault.ValueListItemOperations.GetValueListItems((int)vlId);
			foreach (ValueListItem vlItem in vlItems) {
				if (vlItemNameToAdd.Equals(vlItem.Name, StringComparison.InvariantCultureIgnoreCase))
					return null;
			}

			ValueListItem vlItemToAdd = new ValueListItem() { Name = vlItemNameToAdd };
			return obj.objVerEx.Vault.ValueListItemOperations.AddValueListItem((int)vlId, vlItemToAdd, true);
		}

		public static void DeleteValueListItem(this IObjVerEx obj, VL_ValueLists vlId, int vlItemId)
		{
			obj.objVerEx.Vault.ValueListItemOperations.RemoveValueListItem((int)vlId, vlItemId);
		}

		public static void DeleteValueListItem(this IObjVerEx obj, VL_ValueLists vlId, string vlItemNameToDelete)
		{
			if (string.IsNullOrWhiteSpace(vlItemNameToDelete))
				return;

			ValueListItems vlItems = obj.objVerEx.Vault.ValueListItemOperations.GetValueListItems((int)vlId);
			foreach (ValueListItem vlItem in vlItems) {
				if (vlItemNameToDelete.Equals(vlItem.Name, StringComparison.InvariantCultureIgnoreCase)) {
					obj.objVerEx.Vault.ValueListItemOperations.RemoveValueListItem((int)vlId, vlItem.ID);
					break;
				}
			}
		}

		public static bool HasValueListItem(this IObjVerEx obj, VL_ValueLists vlId, string vlItemName)
		{
			bool vlItemExists = false;

			if (string.IsNullOrWhiteSpace(vlItemName))
				return vlItemExists;

			ValueListItems vlItems = obj.objVerEx.Vault.ValueListItemOperations.GetValueListItems((int)vlId);
			foreach (ValueListItem vlItem in vlItems) {
				if (vlItemName.Equals(vlItem.Name, StringComparison.InvariantCultureIgnoreCase)) {
					vlItemExists = true;
					break;
				}
			}

			return vlItemExists;
		}

		public static int GetVautlValueListItem(this IObjVerEx obj, VL_ValueLists vlId, string item)
		{
			ValueListItems vlItems = obj.objVerEx.Vault.ValueListItemOperations.GetValueListItems((int)vlId);
			foreach (ValueListItem vlItem in vlItems) {
				if (item.Equals(vlItem.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)) {
					return vlItem.ID;
				}
			}

			return -1;
		}

		public static bool ValueListContains(ValueListItems vlitems, string item, int owner = -1)
		{
			HashSet<string> VListItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if (owner != -1) {
				foreach (ValueListItem vl in vlitems) {
					if (vl.OwnerID != owner)
						continue;
					VListItems.Add($"{owner}{vl.Name}");
				}

				if (VListItems.Contains($"{owner}{item}"))
					return true;
			} else {
				foreach (ValueListItem vl in vlitems) {
					VListItems.Add(vl.Name);
				}

				if (VListItems.Contains(item))
					return true;
			}

			return false;
		}

		public static void SetSingleSelectFromMultiSelectProperty(this IObjVerEx obj, int singleselect_property, int multiselect_property)
		{
			Vault vault = obj.objVerEx.Vault;

			if (!obj.objVerEx.HasProperty(multiselect_property))
				throw new Exception("Multi-select property not found.");
			if (!obj.objVerEx.HasProperty(singleselect_property))
				throw new Exception("Single-select property not found.");

			PropertyDefAdmin mpd = vault.PropertyDefOperations.GetPropertyDefAdmin(multiselect_property);
			if (mpd == null)
				throw new Exception("Multi-select property not found.");
			if (!mpd.PropertyDef.BasedOnValueList)
				throw new Exception("Invalid multi-select property parameter.");

			PropertyDefAdmin spd = vault.PropertyDefOperations.GetPropertyDefAdmin(singleselect_property);
			if (spd == null)
				throw new Exception("Single-select property not found.");
			if (!spd.PropertyDef.BasedOnValueList)
				throw new Exception("Invalid single-select property parameter.");

			if (spd.PropertyDef.ValueList != mpd.PropertyDef.ValueList)
				throw new Exception("Target property is not  based on the same multi-select value with the Source.");

			Lookups mvalues = obj.objVerEx.GetProperty(multiselect_property).TypedValue.GetValueAsLookups();
			IEnumerator m_enumerator = mvalues.GetEnumerator();
			m_enumerator.MoveNext();
			Lookup first_mvalue = m_enumerator.Current as Lookup;
			obj.objVerEx.SetProperty(singleselect_property, MFDataType.MFDatatypeLookup, first_mvalue?.Item);

		}

		public static bool objChanged(this IObjVerEx obj, PropertyValues compared_to)
		{
			if (obj.objVerEx.PreviousVersion == null)
				return true;

			PropertyValues cur = compared_to;
			PropertyValues prev = obj.objVerEx.Properties;

			if (cur.Count != prev.Count)
				return true;

			for (int i = 1; i < cur.Count; i++) {
				if (cur[i].GetValueAsUnlocalizedText() != prev[i].GetValueAsUnlocalizedText()
					&& i != 4    //last modified by list index
					&& i != 2    //last modified date list index
					&& i != 5) { // version date list index
					return true;
				}
			}
			return false;
		}

		public static void SetPropertyFrom(this IObjVerEx obj, IObjVerEx source_obj, int property_id, MFDataType property_datatype)
		{
			if (source_obj.objVerEx.HasProperty(property_id)) {
				obj.RemoveCachedProperty(property_id);
				if (source_obj.IsNull(property_id))
					obj.SetToNull(property_id, property_datatype);
				else
					obj.objVerEx.SetProperty(source_obj.objVerEx.GetProperty(property_id));
			}
		}

		public static MultiSelectOTProperty<T> GetMultiSelectLookupValueForPropertyDefCache<T>(this IObjVerEx ot, int property_id) where T : class, IObjVerEx, new()
		{
			if (ot.object_properties_cache.ContainsKey(property_id))
				return ot.object_properties_cache[property_id] as MultiSelectOTProperty<T>;
			MultiSelectOTProperty<T> pd_value = new MultiSelectOTProperty<T>(ot, property_id);
			ot.CacheProperty(property_id, pd_value);
			return pd_value;
		}

		public static void SetMultiSelectLookupValueForPropertyDefCache<T>(this IObjVerEx ot, int property_id, IList<T> value) where T : class, IObjVerEx, new()
		{
			ot.RemoveCachedProperty(property_id);
			MultiSelectOTProperty<T> pd_value = new MultiSelectOTProperty<T>(ot, property_id, value);
			ot.CacheProperty(property_id, pd_value);
		}
	}
}