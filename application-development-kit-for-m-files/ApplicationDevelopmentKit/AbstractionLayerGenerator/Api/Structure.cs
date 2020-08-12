using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public enum Permissions
	{
		Deny = 0,
		Allow = 1,
		NotSet = 2
	}
	public abstract class MFilesObject
	{
		public int Id { get; }
		public string Name { get; private set; }
		public string Alias { get; private set; }
		public MFDataType DataType { get; private set; }
		protected MFilesObject(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
		{
			Id = id;
			Name = name;
			Alias = alias;
			DataType = dataType;
		}
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			MFilesObject that = obj as MFilesObject;
			return Id == that.Id;
		}
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
	public class ObjectType : MFilesObject
	{
		public ObjectType(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public bool IsBasedOnValueList {
			get => throw new NotImplementedException(); set => throw new NotImplementedException();
		}
		public List<Permission> CustomPermissions { get; set; }
	}
	public class ClassType : MFilesObject
	{
		public int WorkflowId { get; set; }
		public NamedACL NamedACL { get; set; }
		public ClassType(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public bool IsBasedOnValueList {
			get => throw new NotImplementedException(); set => throw new NotImplementedException();
		}
	}
	public class PropertyDefinition : MFilesObject
	{
		public string ValueListName { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public bool IsAutomaticValue { get; set; }
		public PropertyDefinition(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
	}
	public class ValueList : MFilesObject
	{
		public int ValueListOwnerId { get; set; }
		public class ValueListItem
		{
			public string VLItemName { get; set; }
			public string VLItemDesc { get; set; }
			public string VLDisplayId { get; set; }
			public string VLItemOwnerName { get; set; }
			public int VLItemOwnerId { get; set; }
			public int VLItemId { get; set; }
			public bool HasOwner { get; set; }
			public override bool Equals(object that)
			{
				if (!(that is ValueListItem))
					return false;

				ValueListItem v = (ValueListItem)that;
				string a = $"{VLItemName}{VLItemOwnerId}";
				string b = $"{v.VLItemName}{v.VLItemOwnerId}";
				return a == b;
			}
			public override int GetHashCode()
			{
				return $"{VLItemName}{VLItemOwnerId}".GetHashCode();
			}
		}
		public ValueList(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public List<ValueListItem> VListItems { get; set; }
	}
	public class Workflow : MFilesObject
	{
		protected struct WorkflowState
		{
			int WFSId;
			string WFSName;
		}
		protected struct WorkflowStateTransition
		{
			int WFSTId;
			string WFSTName;
		}
		public Workflow(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		protected List<WorkflowState> WFStates { get; set; }
	}
	public class User : MFilesObject
	{
		public bool IsBasedOnValueList { get; set; }
		public bool IsInternalUser { get; set; }
		public List<NamedACL> NamedACLList { get; set; }
		public User(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public void PopulateNamedACLs(Vault vault, Dictionary<int, NamedACL> namedACLsDict)
		{
			if (null == vault || namedACLsDict == null)
				return;

			NamedACLAdmin namedACLAdmin = null;
			AccessControlEntryContainer aceContainer = null;
			AccessControlEntryKeys aceKeys = null;

			foreach (KeyValuePair<int, NamedACL> entry in namedACLsDict) {
				namedACLAdmin = vault.NamedACLOperations.GetNamedACLAdmin(entry.Key);
				if (namedACLAdmin.NamedACL.AccessControlList == null || namedACLAdmin.NamedACL.AccessControlList.CustomComponent == null)
					continue;
				aceContainer = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries;
				aceKeys = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries.GetKeys();

				foreach (AccessControlEntryKey aceKey in aceKeys) {
					if (aceKey.UserOrGroupID != Id)
						continue;

					if (NamedACLList == null)
						NamedACLList = new List<NamedACL>();

					NamedACLList.Add(entry.Value);
				}
			}
		}
	}
	public class UserGroup : MFilesObject
	{
		public bool IsBasedOnValueList { get; set; }
		public List<NamedACL> NamedACLList { get; set; }
		public UserGroup(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public void PopulateNamedACLs(Vault vault, Dictionary<int, NamedACL> namedACLsDict)
		{
			if (null == vault || namedACLsDict == null)
				return;

			NamedACLAdmin namedACLAdmin = null;
			AccessControlEntryContainer aceContainer = null;
			AccessControlEntryKeys aceKeys = null;

			foreach (KeyValuePair<int, NamedACL> entry in namedACLsDict) {
				namedACLAdmin = vault.NamedACLOperations.GetNamedACLAdmin(entry.Key);
				if (namedACLAdmin.NamedACL.AccessControlList == null || namedACLAdmin.NamedACL.AccessControlList.CustomComponent == null)
					continue;
				aceContainer = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries;
				aceKeys = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries.GetKeys();

				foreach (AccessControlEntryKey aceKey in aceKeys) {
					if (aceKey.UserOrGroupID != Id)
						continue;

					if (NamedACLList == null)
						NamedACLList = new List<NamedACL>();

					NamedACLList.Add(entry.Value);
				}
			}
		}
	}
	public class NamedACL : MFilesObject
	{
		public bool IsBasedOnValueList { get; set; }
		public List<MFilesObject> UserOrUserGroupList { get; set; }
		public NamedACL(int id, string name, string alias, MFDataType dataType = MFDataType.MFDatatypeUninitialized)
			: base(id, name, alias, dataType) { }
		public static NamedACL Convert(MFilesAPI.NamedACL namedACL)
		{
			if (namedACL == null)
				return null;

			return new NamedACL(namedACL.ID, namedACL.Name, namedACL.Name);
		}
		public void PopulateUsersOrUserGroups(Vault vault, Dictionary<int, User> usersDict, Dictionary<int, UserGroup> userGroupsDict)
		{
			if (null == vault || (usersDict == null && userGroupsDict == null))
				return;

			NamedACLAdmin namedACLAdmin = vault.NamedACLOperations.GetNamedACLAdmin(Id);
			NamedACL newNamedACL = NamedACL.Convert(namedACLAdmin.NamedACL);

			if (namedACLAdmin.NamedACL.AccessControlList == null || namedACLAdmin.NamedACL.AccessControlList.CustomComponent == null)
				return;

			AccessControlEntryContainer aceContainer = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries;
			AccessControlEntryKeys aceKeys = namedACLAdmin.NamedACL.AccessControlList.CustomComponent.AccessControlEntries.GetKeys();

			foreach (AccessControlEntryKey aceKey in aceKeys) {
				if (UserOrUserGroupList == null)
					UserOrUserGroupList = new List<MFilesObject>();

				if (aceKey.IsGroup)
					UserOrUserGroupList.Add(userGroupsDict[aceKey.UserOrGroupID]);
				else
					UserOrUserGroupList.Add(usersDict[aceKey.UserOrGroupID]);
			}
		}
	}
	public class Permission
	{
		public MFilesObject UserOrUserGroup { get; set; }
		[DefaultValue(Permissions.NotSet)]
		public Permissions ReadPermission { get; set; }
		[DefaultValue(Permissions.NotSet)]
		public Permissions DeletePermission { get; set; }
		[DefaultValue(Permissions.NotSet)]
		public Permissions EditPermission { get; set; }
		[DefaultValue(Permissions.NotSet)]
		public Permissions ChangePermissionsPermission { get; set; }
		[DefaultValue(Permissions.NotSet)]
		public Permissions AttachObjectsPermission { get; set; }

		public void Populate(AccessControlEntryKey aceKey, AccessControlEntryContainer accessControlEntries,
			Dictionary<int, User> usersDict, Dictionary<int, UserGroup> userGroupsDict)
		{
			if (aceKey.IsGroup)
				UserOrUserGroup = userGroupsDict[aceKey.UserOrGroupID];
			else
				UserOrUserGroup = usersDict[aceKey.UserOrGroupID];

			ReadPermission = (Permissions)accessControlEntries.At(aceKey).ReadPermission;
			DeletePermission = (Permissions)accessControlEntries.At(aceKey).DeletePermission;
			EditPermission = (Permissions)accessControlEntries.At(aceKey).EditPermission;
			ChangePermissionsPermission = (Permissions)accessControlEntries.At(aceKey).ChangePermissionsPermission;
			AttachObjectsPermission = (Permissions)accessControlEntries.At(aceKey).AttachObjectsPermission;
		}
	}
	public class ObjectTypes : IEnumerable<ObjectType>
	{
		public List<ObjectType> ObjectTypeList { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public Vault Vault { get; set; }
		public ObjectTypes(Vault vault)
		{
			if (null != vault)
				Vault = vault;

			ObjectTypeList = new List<ObjectType>();
			ObjTypesAdmin objAdmins = Vault.ObjectTypeOperations.GetObjectTypesAdmin();
			foreach (ObjTypeAdmin obj in objAdmins) {
				ObjectType objectType = GetObjectFromObjectTypeAdmin(obj);

				if (!ObjectTypeList.Contains(objectType))
					ObjectTypeList.Add(objectType);
			}
		}
		public ObjectTypes(Vault vault, Dictionary<int, User> usersDict, Dictionary<int, UserGroup> userGroupsDict)
		{
			if (null != vault)
				Vault = vault;

			ObjectTypeList = new List<ObjectType>();
			ObjTypesAdmin objAdmins = Vault.ObjectTypeOperations.GetObjectTypesAdmin();
			foreach (ObjTypeAdmin obj in objAdmins) {
				ObjectType objectType = GetObjectFromObjectTypeAdmin(obj);

				AccessControlEntryContainer acEntries = obj.ObjectType.AccessControlList.CustomComponent.AccessControlEntries;

				if (objectType.CustomPermissions == null)
					objectType.CustomPermissions = new List<Permission>();
				Permission permission = null;
				foreach (AccessControlEntryKey aceKey in acEntries.GetKeys()) {
					permission = new Permission();
					permission.Populate(aceKey, acEntries, usersDict, userGroupsDict);
					objectType.CustomPermissions.Add(permission);
				}

				if (!ObjectTypeList.Contains(objectType))
					ObjectTypeList.Add(objectType);
			}
		}
		private ObjectType GetObjectFromObjectTypeAdmin(ObjTypeAdmin obj)
		{
			if (obj == null)
				return null;

			return new ObjectType(obj.ObjectType.ID, obj.ObjectType.NameSingular, obj.SemanticAliases.Value);
		}
		public IEnumerator<ObjectType> GetEnumerator()
		{
			return ObjectTypeList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	public class ClassTypes : IEnumerable<ClassType>
	{
		public List<ClassType> ClassTypeList { get; set; }
		public Dictionary<int, ClassType> ClassTypesDict { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public Vault Vault { get; set; }
		public ClassTypes(Vault vault)
		{
			if (null != vault)
				Vault = vault;

			ClassTypeList = new List<ClassType>();
			ClassTypesDict = new Dictionary<int, ClassType>();
			ObjectClassesAdmin objClassesAdmins = Vault.ClassOperations.GetAllObjectClassesAdmin();
			foreach (ObjectClassAdmin objClassAdmin in objClassesAdmins) {
				ClassType objClass = GetObjectClass(objClassAdmin);
				if (!ClassTypeList.Contains(objClass)) {
					ClassTypeList.Add(objClass);
					ClassTypesDict.Add(objClass.Id, objClass);
				}
			}
		}
		private ClassType GetObjectClass(ObjectClassAdmin objClassAdmin)
		{
			if (objClassAdmin == null)
				return null;

			ClassType newClassType = new ClassType(objClassAdmin.ID, objClassAdmin.Name, objClassAdmin.SemanticAliases.Value) {
				WorkflowId = objClassAdmin.Workflow
			};
			if (objClassAdmin.AutomaticPermissionsForObjects != null) {
				MFilesAPI.NamedACL namedACL = objClassAdmin.AutomaticPermissionsForObjects.NamedACL;
				newClassType.NamedACL = new NamedACL(namedACL.ID, namedACL.Name, namedACL.Name);
			}

			return newClassType;
		}
		public Dictionary<int, ClassType> ObjectClasses(int ObjectID)
		{
			Dictionary<int, ClassType> ObjectClassesDictionary = new Dictionary<int, ClassType>();
			ClassType classType = null;

			ObjectClassesAdmin ObjectClassesAdmin = Vault.ClassOperations.GetObjectClassesAdmin(ObjectID);
			foreach (ObjectClassAdmin ObjClassAdmin in ObjectClassesAdmin) {
				classType = GetObjectClass(ObjClassAdmin);
				if (classType != null) {
					ObjectClassesDictionary.Add(classType.Id, classType);
				}
			}
			return ObjectClassesDictionary;
		}
		public IEnumerator<ClassType> GetEnumerator()
		{
			return ClassTypeList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	public class PropertyDefinitions : IEnumerable<PropertyDefinition>
	{
		public List<PropertyDefinition> PropertyDefsList { get; set; }
		public Dictionary<int, PropertyDefinition> PropertyDefsDict { get; set; }
		public Vault Vault { get; set; }
		public PropertyDefinitions(Vault vault)
		{
			if (vault != null)
				Vault = vault;

			PropertyDefsList = new List<PropertyDefinition>();
			PropertyDefsDict = new Dictionary<int, PropertyDefinition>();
			PropertyDefsAdmin propDefsAdmin = vault.PropertyDefOperations.GetPropertyDefsAdmin();
			foreach (PropertyDefAdmin pd in propDefsAdmin) {
				if (string.IsNullOrWhiteSpace(pd.PropertyDef.Name))
					continue;

				PropertyDefinition property = GetPropertyDefFromPropertyDefAdmin(pd);
				PropertyDefsList.Add(property);
				PropertyDefsDict.Add(pd.PropertyDef.ID, property);
			}
		}
		private PropertyDefinition GetPropertyDefFromPropertyDefAdmin(PropertyDefAdmin pd)
		{
			if (pd == null)
				return null;
			string property_name = pd.PropertyDef.Name;
			int property_id = pd.PropertyDef.ID;
			if (PropertyDefsDict.Values.Any(prp => string.Equals(prp.Name, property_name)))
				property_name = $"{property_name}_${property_id}";

			PropertyDefinition prop = new PropertyDefinition(
				property_id, property_name, pd.SemanticAliases.Value, pd.PropertyDef.DataType);
			prop.IsAutomaticValue = pd.PropertyDef.AutomaticValueType != MFAutomaticValueType.MFAutomaticValueTypeNone;

			switch (pd.PropertyDef.DataType) {
			case MFDataType.MFDatatypeLookup:
			case MFDataType.MFDatatypeMultiSelectLookup:
				if (Vault.ValueListOperations.GetValueListAdmin(pd.PropertyDef.ValueList).ObjectType.RealObjectType) {
					ObjTypeAdmin alias = Vault.ObjectTypeOperations.GetObjectTypeAdmin(pd.PropertyDef.ValueList);
					prop.ValueListName = alias.ObjectType.NameSingular.Normalize_to_cs_type();
				} else {
					prop.IsBasedOnValueList = true;
					ObjTypeAdmin alias = Vault.ValueListOperations.GetValueListAdmin(pd.PropertyDef.ValueList);
					prop.ValueListName = alias.ObjectType.NameSingular.Normalize_to_cs_type();
				}
				break;
			}

			return prop;
		}
		public Dictionary<int, PropertyDefinition> ObjectPropertyDefinitions(int ObjectID)
		{
			Dictionary<int, PropertyDefinition> ObjectPropertyDefs = new Dictionary<int, PropertyDefinition>();

			ObjectClasses ObjectClasses = Vault.ClassOperations.GetObjectClasses(ObjectID);
			PropertyDef prop_def = null;
			foreach (ObjectClass ObjClass in ObjectClasses) {
				foreach (AssociatedPropertyDef prop in ObjClass.AssociatedPropertyDefs) {
					if (ObjectPropertyDefs.ContainsKey(prop.PropertyDef))
						continue;
					if (IsInRequiredBuiltInPropertyDefinitions(prop.PropertyDef))
						continue;
					if (PropertyDefsDict.ContainsKey(prop.PropertyDef))
						ObjectPropertyDefs.Add(prop.PropertyDef, PropertyDefsDict[prop.PropertyDef]);
				}
				foreach (MFBuiltInPropertyDef prop_def_enum in MANDATORY_BUILT_IN_OBJ_PD_IDS) {
					prop_def = Vault.PropertyDefOperations.GetBuiltInPropertyDef(prop_def_enum);
					if (!ObjectPropertyDefs.Keys.Contains(prop_def.ID))
						ObjectPropertyDefs.Add(prop_def.ID, PropertyDefsDict[prop_def.ID]);
				}
			}

			ObjType current_obj_type = Vault.ObjectTypeOperations.GetObjectType(ObjectID);
			if (current_obj_type.HasOwnerType) { // are we dealing with a parent ?
				ObjType parent_obj_type = Vault.ObjectTypeOperations.GetObjectType(current_obj_type.OwnerType);
				ObjectPropertyDefs.Add(current_obj_type.OwnerPropertyDef, PropertyDefsDict[parent_obj_type.OwnerPropertyDef]);
			}

			return ObjectPropertyDefs;
		}
		public Dictionary<int, List<PropertyDefinition>> ClassPropertyDefinitions(int ObjectID, bool mustBeRequired = true)
		{
			Dictionary<int, List<PropertyDefinition>> ClassesPropertyDefsMap = new Dictionary<int, List<PropertyDefinition>>();
			List<PropertyDefinition> classPropDefsList = null;

			ObjType currentObjType = Vault.ObjectTypeOperations.GetObjectType(ObjectID);
			ObjType parentObjType = currentObjType.HasOwnerType ?
				Vault.ObjectTypeOperations.GetObjectType(currentObjType.OwnerType) : null;

			ObjectClasses objClasses = Vault.ClassOperations.GetObjectClasses(ObjectID);
			foreach (ObjectClass objClass in objClasses) {
				classPropDefsList = new List<PropertyDefinition>();
				if (parentObjType != null)
					classPropDefsList.Add(PropertyDefsDict[parentObjType.OwnerPropertyDef]);

				foreach (AssociatedPropertyDef prop in objClass.AssociatedPropertyDefs) {
					if (mustBeRequired) {
						if (prop.Required && !IsInRequiredBuiltInPropertyDefinitions(prop.PropertyDef) && !PropertyDefsDict[prop.PropertyDef].IsAutomaticValue)
							classPropDefsList.Add(PropertyDefsDict[prop.PropertyDef]);
					} else {
						if (!IsInRequiredBuiltInPropertyDefinitions(prop.PropertyDef))
							classPropDefsList.Add(PropertyDefsDict[prop.PropertyDef]);
					}
				}
				ClassesPropertyDefsMap[objClass.ID] = classPropDefsList;
			}

			return ClassesPropertyDefsMap;
		}
		public IEnumerator<PropertyDefinition> GetEnumerator()
		{
			return PropertyDefsList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		// Required for object constructors with required parameters
		private bool IsInRequiredBuiltInPropertyDefinitions(int propertyDefId)
		{
			MFBuiltInPropertyDef[] excluded_required_builtin_pds = {
				MFBuiltInPropertyDef.MFBuiltInPropertyDefCreated,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModifiedBy,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefStatusChanged,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefObjectChanged,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefSingleFileObject,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefSizeOnServerThisVersion,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefSizeOnServerAllVersions,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefMarkedForArchiving,
				MFBuiltInPropertyDef.MFBuiltInPropertyDefClassGroups
			};
			return Array.IndexOf(excluded_required_builtin_pds, (MFBuiltInPropertyDef)propertyDefId) != -1;
		}
		// Required for object fields
		private List<MFBuiltInPropertyDef> MANDATORY_BUILT_IN_OBJ_PD_IDS = new List<MFBuiltInPropertyDef>{
			MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow,
			MFBuiltInPropertyDef.MFBuiltInPropertyDefState,
			MFBuiltInPropertyDef.MFBuiltInPropertyDefKeywords,
			MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy,
			MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModifiedBy
		};
		private bool IsInMandatoryBuiltInPropertyDefinitions(int propertyDefId)
		{
			return MANDATORY_BUILT_IN_OBJ_PD_IDS.Contains((MFBuiltInPropertyDef)propertyDefId);
		}
	}
	public class ValueLists : IEnumerable<ValueList>
	{
		public List<ValueList> VListsList { get; set; }
		public Dictionary<string, string> OwnerNameDict { get; set; }
		public Vault Vault { get; set; }
		public ValueLists(Vault vault)
		{
			if (vault != null)
				Vault = vault;

			VListsList = new List<ValueList>();
			OwnerNameDict = new Dictionary<string, string>();
			List<ValueList.ValueListItem> VaultValueLists = new List<ValueList.ValueListItem>();
			ObjTypesAdmin vLists = vault.ValueListOperations.GetValueListsAdmin();
			foreach (ObjTypeAdmin vList in vLists) {
				if (vList.ObjectType.RealObjectType) continue;
				VaultValueLists.Add(new ValueList.ValueListItem {
					VLItemId = vList.ObjectType.ID,
					VLItemName = vList.ObjectType.NameSingular.Normalize_to_cs_type(),
					VLItemDesc = vList.ObjectType.NameSingular,
					HasOwner = vList.ObjectType.HasOwnerType,
					VLDisplayId = vList.ObjectType.ID.ToString(),
					VLItemOwnerId = vList.ObjectType.OwnerType
				});

				// Add ValueList Items
				VListsList.Add(GetValueListFromObjTypeAdmin(vList));
			}
			// Vault ValueLists
			VListsList.Add(new ValueList(-1, "ValueLists", "VL_ValueLists") { VListItems = VaultValueLists });

			//Populate the ValueListItems owner names
			foreach (ValueList vl in VListsList) {
				if (vl.Name == "Valuelists") continue;
				for (int i = 0; i < vl.VListItems.Count; i++) {
					ValueList.ValueListItem vli = vl.VListItems[i];
					if (!vli.HasOwner) continue;
					string value_list_id_owner_id = $"{vl.ValueListOwnerId}_{vli.VLItemOwnerId}";
					if (OwnerNameDict.ContainsKey(value_list_id_owner_id))
						vli.VLItemOwnerName = OwnerNameDict[value_list_id_owner_id];
				}
			}

			List<ValueList.ValueListItem> naclValueList = new List<ValueList.ValueListItem>();
			foreach (MFilesAPI.NamedACL nacl in vault.NamedACLOperations.GetNamedACLs()) {
				naclValueList.Add(new ValueList.ValueListItem {
					VLItemId = nacl.ID,
					VLItemName = nacl.Name.Trim(),
					VLItemDesc = nacl.Name.Trim(),
					HasOwner = false,
					VLDisplayId = nacl.ID.ToString(),
					VLItemOwnerId = -1
				});
			}
			VListsList.Add(new ValueList(-1, "NACLs", "VL_NACLs") { VListItems = naclValueList });
		}
		private ValueList GetValueListFromObjTypeAdmin(ObjTypeAdmin vList)
		{
			List<ValueList.ValueListItem> ValueListItems = new List<ValueList.ValueListItem>();

			ValueListItems vListItems = Vault.ValueListItemOperations.GetValueListItems(vList.ObjectType.ID, true);
			foreach (ValueListItem vListItem in vListItems) {
				if (vListItem.Deleted) continue;
				if (string.IsNullOrEmpty(vListItem.Name)) continue;
				if (vListItem.DisplayID == "Not available") continue;

				ValueList.ValueListItem v = new ValueList.ValueListItem {
					VLItemId = vListItem.ID,
					VLItemName = vListItem.Name.Trim(),
					VLItemDesc = vListItem.Name.Trim(),
					HasOwner = vListItem.HasOwner,
					VLDisplayId = vListItem.DisplayID.Trim(),
					VLItemOwnerId = vListItem.OwnerID,
					VLItemOwnerName = string.Empty
				};

				string value_list_id_item_id = $"{vList.ObjectType.ID}_{v.VLItemId}";
				if (!OwnerNameDict.ContainsKey(value_list_id_item_id))
					OwnerNameDict.Add(value_list_id_item_id, v.VLItemName.Normalize_to_cs_type());

				if (ValueListItems.Contains(v))
					v.VLItemName += $"_{v.VLItemId}";

				ValueListItems.Add(v);
			}

			ValueList ValueList = new ValueList(vList.ObjectType.ID, vList.ObjectType.NameSingular.Normalize_to_cs_type(), vList.SemanticAliases.Value) {
				ValueListOwnerId = vList.ObjectType.OwnerType,
				VListItems = ValueListItems
			};

			return ValueList;
		}
		public IEnumerator<ValueList> GetEnumerator()
		{
			return VListsList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	public class Workflows : IEnumerable<Workflow>
	{
		public List<Workflow> WorkflowsList;
		public IEnumerator<Workflow> GetEnumerator()
		{
			return WorkflowsList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		public static bool HasWorkflowStates(Vault vault)
		{
			bool hasWorkflowStates = false;
			WorkflowsAdmin workflowsAdmin = vault.WorkflowOperations.GetWorkflowsAdmin();

			foreach (WorkflowAdmin workflowAdmin in workflowsAdmin) {
				if (workflowAdmin.States.Count > 0) {
					hasWorkflowStates = true;
					break;
				}
			}

			return hasWorkflowStates;
		}
	}
	public class Users : IEnumerable<User>
	{
		public List<User> UserList { get; set; }
		public Dictionary<int, User> UsersDict { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public Vault Vault { get; set; }
		public Users(Vault vault)
		{
			if (null != vault)
				Vault = vault;

			UserList = new List<User>();
			UsersDict = new Dictionary<int, User>();
			UserAccounts userAccounts = Vault.UserOperations.GetUserAccounts();

			foreach (UserAccount userAccount in userAccounts) {
				User newUser = CreateUser(userAccount);
				UserList.Add(newUser);
				UsersDict.Add(userAccount.ID, newUser);
			}
		}
		public Users(Vault vault, Dictionary<int, NamedACL> namedACLsDict)
		{
			if (null != vault)
				Vault = vault;

			UserList = new List<User>();
			UsersDict = new Dictionary<int, User>();
			UserAccounts userAccounts = Vault.UserOperations.GetUserAccounts();

			foreach (UserAccount userAccount in userAccounts) {
				User newUser = CreateUser(userAccount);
				newUser.PopulateNamedACLs(Vault, namedACLsDict);
				UserList.Add(newUser);
				UsersDict.Add(userAccount.ID, newUser);
			}
		}
		private User CreateUser(UserAccount user)
		{
			if (user == null)
				return null;

			User newUser = new User(user.ID, user.LoginName, user.LoginName);
			newUser.IsInternalUser = user.InternalUser;

			return newUser;
		}
		public IEnumerator<User> GetEnumerator()
		{
			return UserList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
	public class UserGroups : IEnumerable<UserGroup>
	{
		public List<UserGroup> UserGroupList { get; set; }
		public Dictionary<int, UserGroup> UserGroupsDict { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public Vault Vault { get; set; }
		public UserGroups(Vault vault)
		{
			if (null != vault)
				Vault = vault;

			UserGroupList = new List<UserGroup>();
			UserGroupsDict = new Dictionary<int, UserGroup>();
			UserGroupsAdmin userGroupsAdmin = Vault.UserGroupOperations.GetUserGroupsAdmin();

			foreach (UserGroupAdmin userGroupAdmin in userGroupsAdmin) {
				UserGroup newUserGroup = CreateUserGroup(userGroupAdmin);
				UserGroupList.Add(newUserGroup);
				UserGroupsDict.Add(userGroupAdmin.UserGroup.ID, newUserGroup);
			}
		}
		public UserGroups(Vault vault, Dictionary<int, NamedACL> namedACLsDict)
		{
			if (null != vault)
				Vault = vault;

			UserGroupList = new List<UserGroup>();
			UserGroupsDict = new Dictionary<int, UserGroup>();
			UserGroupsAdmin userGroupsAdmin = Vault.UserGroupOperations.GetUserGroupsAdmin();

			foreach (UserGroupAdmin userGroupAdmin in userGroupsAdmin) {
				UserGroup newUserGroup = CreateUserGroup(userGroupAdmin);
				newUserGroup.PopulateNamedACLs(Vault, namedACLsDict);
				UserGroupList.Add(newUserGroup);
				UserGroupsDict.Add(userGroupAdmin.UserGroup.ID, newUserGroup);
			}
		}
		private UserGroup CreateUserGroup(UserGroupAdmin userGroupAdmin)
		{
			if (userGroupAdmin == null)
				return null;

			return new UserGroup(userGroupAdmin.UserGroup.ID, userGroupAdmin.UserGroup.Name, userGroupAdmin.SemanticAliases.Value);
		}
		public IEnumerator<UserGroup> GetEnumerator()
		{
			return UserGroupList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
	public class NamedACLs : IEnumerable<NamedACL>
	{
		public List<NamedACL> NamedACLList { get; set; }
		public Dictionary<int, NamedACL> NamedACLsDict { get; set; }
		public bool IsBasedOnValueList { get; set; }
		public Vault Vault { get; set; }
		public NamedACLs(Vault vault)
		{
			if (null != vault)
				Vault = vault;

			NamedACLList = new List<NamedACL>();
			NamedACLsDict = new Dictionary<int, NamedACL>();
			MFilesAPI.NamedACLs namedACLs = Vault.NamedACLOperations.GetNamedACLs();

			foreach (MFilesAPI.NamedACL namedACL in namedACLs) {
				NamedACL newNamedACL = NamedACL.Convert(namedACL);
				NamedACLList.Add(newNamedACL);
				NamedACLsDict.Add(namedACL.ID, newNamedACL);
			}
		}
		public NamedACLs(Vault vault, Dictionary<int, User> usersDict, Dictionary<int, UserGroup> userGroupsDict)
		{
			if (null != vault)
				Vault = vault;

			NamedACLList = new List<NamedACL>();
			NamedACLsDict = new Dictionary<int, NamedACL>();

			NamedACLsAdmin namedACLsAdmin = Vault.NamedACLOperations.GetNamedACLsAdmin();
			NamedACL newNamedACL = null;
			foreach (NamedACLAdmin namedACLAdmin in namedACLsAdmin) {
				newNamedACL = NamedACL.Convert(namedACLAdmin.NamedACL);
				newNamedACL.PopulateUsersOrUserGroups(Vault, usersDict, userGroupsDict);
				NamedACLList.Add(newNamedACL);
				NamedACLsDict.Add(namedACLAdmin.NamedACL.ID, newNamedACL);
			}
		}
		public IEnumerator<NamedACL> GetEnumerator()
		{
			return NamedACLList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
