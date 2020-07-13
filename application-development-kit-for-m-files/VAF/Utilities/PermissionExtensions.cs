//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using MFilesAPI;

namespace VAF
{
	public enum PERMISSIONS
	{
		READ = 1,
		DELETE = 2,
		EDIT = 4,
		CHANGE_PERMISSIONS = 8,
		ATTACH_OBJECT = 16
	}
	public static class PermissionsExtensions
	{
		public static bool HasPermissions(this IObjVerEx obj, PERMISSIONS permissionsToCheck, VL_User userId)
		{
			return obj.HasPermissions(permissionsToCheck, (int)userId, false);
		}
		public static bool HasPermissions(this IObjVerEx obj, PERMISSIONS permissionsToCheck, VL_User_group userGroupId)
		{
			return obj.HasPermissions(permissionsToCheck, (int)userGroupId, true);
		}
		public static bool HasPermissions(this IObjVerEx obj, PERMISSIONS permissionsToCheck, int userOrGroupId, bool isGroup)
		{
			if (obj.objVerEx.Permissions == null) return false;

			bool hasPermission = true;
			ObjectVersionPermissions objPermissions = obj.objVerEx.Permissions;
			AccessControlListComponent aclComponent = objPermissions.AccessControlList.CustomComponent;
			AccessControlEntryData aceData = aclComponent.GetACEByUserOrGroupID(userOrGroupId, isGroup);

			if ((permissionsToCheck & PERMISSIONS.READ) == PERMISSIONS.READ)
				hasPermission = hasPermission && aceData.ReadPermission == MFPermission.MFPermissionAllow;
			if ((permissionsToCheck & PERMISSIONS.DELETE) == PERMISSIONS.DELETE)
				hasPermission = hasPermission && aceData.DeletePermission == MFPermission.MFPermissionAllow;
			if ((permissionsToCheck & PERMISSIONS.EDIT) == PERMISSIONS.EDIT)
				hasPermission = hasPermission && aceData.EditPermission == MFPermission.MFPermissionAllow;
			if ((permissionsToCheck & PERMISSIONS.CHANGE_PERMISSIONS) == PERMISSIONS.CHANGE_PERMISSIONS)
				hasPermission = hasPermission && aceData.ChangePermissionsPermission == MFPermission.MFPermissionAllow;
			if ((permissionsToCheck & PERMISSIONS.ATTACH_OBJECT) == PERMISSIONS.ATTACH_OBJECT)
				hasPermission = hasPermission && aceData.AttachObjectsPermission == MFPermission.MFPermissionAllow;
			
			return hasPermission;
		}
		public static NamedACL GetNamedACL(this IObjVerEx obj)
		{
			return obj.objVerEx.Permissions.NamedACL;
		}
		public static void SetNamedACL(this IObjVerEx obj, VL_NACLs? vl_nacl)
		{
			if (vl_nacl == null) return;
			SetNamedACL(obj, new NamedACL { ID = (int)vl_nacl });
		}
		public static void SetNamedACL(this IObjVerEx obj, NamedACL namedACL)
		{
			obj.objVerEx.Vault.ObjectOperations.ChangePermissionsToNamedACL(obj.objVerEx.ObjVer, namedACL.ID, false);
		}
		public static AccessControlList GetAccessControlList(this IObjVerEx obj)
		{
			return obj.objVerEx.Permissions.AccessControlList;
		}
		public static void SetAccessControlList(this IObjVerEx obj, AccessControlList accessControlList)
		{
			obj.objVerEx.Vault.ObjectOperations.ChangePermissionsToACL(obj.objVerEx.ObjVer, accessControlList, false);
		}
	}
}