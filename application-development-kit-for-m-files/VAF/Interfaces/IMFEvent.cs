//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using System;

namespace VAF
{
	interface IMFEvent { }
	public class MFEventHandlerVaultExtensionMethod : Attribute, IMFEvent { }
	public class MFEventHandlerAfterBeginTransaction : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeCommitTransaction : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeRollbackTransaction : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeSetProperties : Attribute, IMFEvent { }
	public class MFEventHandlerAfterSetProperties : Attribute, IMFEvent { }
	public class MFEventHandlerAfterCreateNewObjectFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeCheckInChanges : Attribute, IMFEvent { }
	public class MFEventHandlerAfterCheckInChanges : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeCheckOut : Attribute, IMFEvent { }
	public class MFEventHandlerAfterCheckOut : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeDeleteObject : Attribute, IMFEvent { }
	public class MFEventHandlerAfterDeleteObject : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeDestroyObject : Attribute, IMFEvent { }
	public class MFEventHandlerAfterDestroyObject : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeSetObjectPermissions : Attribute, IMFEvent { }
	public class MFEventHandlerAfterSetObjectPermissions : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeFileUpload : Attribute, IMFEvent { }
	public class MFEventHandlerAfterFileUpload : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeFileDownload : Attribute, IMFEvent { }
	public class MFEventHandlerAfterFileDownload : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeCreateNewObjectFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeDestroyObjectVersion : Attribute, IMFEvent { }
	public class MFEventHandlerAfterDestroyObjectVersion : Attribute, IMFEvent { }
	public class MFEventHandlerReplication_AfterCreateNewObjectFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerReplication_AfterCheckInChanges : Attribute, IMFEvent { }
	public class MFEventHandlerAfterCheckInChangesFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerAfterCancelCheckoutFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeUndeleteObject : Attribute, IMFEvent { }
	public class MFEventHandlerAfterUndeleteObject : Attribute, IMFEvent { }
	public class MFEventHandlerAfterUndeleteObjectFinalize : Attribute, IMFEvent { }
	public class MFEventHandlerBeforeCheckInChangesFinalize : Attribute, IMFEvent { }
}
