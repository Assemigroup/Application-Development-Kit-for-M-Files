//------------------------------------------------------------------------------------
//
// Manual changes to this file may cause unexpected behavior in your application.
//
//------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using MFilesAPI;
using MFiles.VAF.Common;

namespace VAF
{
	public static class APIExtensions
	{
		public static bool create(this ObjVerEx objVerEx, int typeId, int? createdby_mfuserid = null)
		{
			try {
				ObjectVersionAndProperties objVerAndProps =
					objVerEx.Vault.ObjectOperations.CreateNewObjectEx(typeId, objVerEx.Properties, null, false, createdby_mfuserid == null);

				int objVerID = objVerAndProps.ObjVer.ID;
				int objVersion = objVerAndProps.ObjVer.Version;

				if (createdby_mfuserid != null) {
					TypedValue created_by = new TypedValue();
					created_by.SetValue(MFDataType.MFDatatypeLookup, createdby_mfuserid.Value);
					objVerAndProps = objVerEx.Vault.ObjectPropertyOperations.SetCreationInfoAdmin(objVerAndProps.ObjVer, true, created_by, false, null);
					ObjectVersion objver = objVerEx.Vault.ObjectOperations.CheckIn(objVerAndProps.ObjVer);
					objVerID = objver.ObjVer.ID;
					objVersion = objver.ObjVer.Version;
				}
				objVerEx.ObjVer.ID = objVerID;
				objVerEx.ObjVer.Version = objVersion;
				objVerEx.ObjVer.Type = objVerAndProps.ObjVer.Type;

				return true;
			} catch (Exception exception) {
				throw exception;
			}
		}
		public static void delete(this IObjVerEx obj)
		{
			try {
				obj.undoObjectCheckoutIfCheckedOut();
				ObjectVersion objVer = obj.objVerEx.Vault.ObjectOperations.DeleteObject(obj.objVerEx.ObjVer.ObjID);
			} catch (Exception exception) {
				throw exception;
			}
		}
		public static void undelete(this IObjVerEx obj)
		{
			try {
				ObjectVersion objVer = obj.objVerEx.Vault.ObjectOperations.UndeleteObject(obj.objVerEx.ObjID);
			} catch (Exception exception) {
				throw exception;
			}
		}
		public static void destroy(this IObjVerEx obj)
		{
			try {
				obj.undoObjectCheckoutIfCheckedOut();
				obj.objVerEx.Vault.ObjectOperations.DestroyObject(obj.objVerEx.ObjID, true, -1);
			} catch (Exception exception) {
				throw exception;
			}
		}
		public static bool update(this IObjVerEx obj, bool returnUpdatedMetadata = true, bool skipVafCalculation = false, int? modifiedby_userid = null, string[] replace_files = null)
		{
			if (obj == null)
				return false;
			// Check if the objcet is deleted or destroyed
			if (obj.objVerEx.Deleted() || obj.objVerEx.IsDestroyed)
				return false;
			ObjectVersionAndProperties checkedInObject =
				obj.objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(obj.objVerEx.ObjID, true);
			if (checkedInObject.VersionData.ObjectCheckedOut)
				return false;
			//
			bool checkedOutByVAF = false;
			ObjectVersion objVersion = null;
			PropertyValue prop_modifiedby = null;
			try {
				prop_modifiedby = obj.objVerEx.GetProperty(PD_Last_modified_by.id);
				objVersion = obj.objVerEx.Vault.ObjectOperations.CheckOut(obj.objVerEx.ObjID);
				checkedOutByVAF = true;
				//
				PropertyValues propsToUpdate = obj.objVerEx.Properties.Clone();
				if (checkedInObject.Properties.Exists(PD_Comment.id) && obj.objVerEx.Properties.Exists(PD_Comment.id)
					&& obj.objVerEx.VersionComment == checkedInObject.Properties.GetProperty(PD_Comment.id).GetValueAsUnlocalizedText())
					propsToUpdate.Remove(propsToUpdate.IndexOf(PD_Comment.id));
				if (skipVafCalculation) {
					string keyword = string.Empty;
					if (propsToUpdate.TryGetProperty(26, out PropertyValue kw)) {
						keyword = kw.GetValueAsUnlocalizedText();
					}
					propsToUpdate.SetProperty(26, MFDataType.MFDatatypeText, $"{keyword}SKIP_VAF_CALCULATIONS");
				}
				ObjectVersionAndProperties objVerAndProps =
					obj.objVerEx.Vault.ObjectPropertyOperations.SetAllProperties(objVersion.ObjVer, false, propsToUpdate);

				if (prop_modifiedby != null) {
					if (modifiedby_userid != null)
						prop_modifiedby.TypedValue.SetValue(MFDataType.MFDatatypeLookup, modifiedby_userid);
					objVerAndProps = obj.objVerEx.Vault.ObjectPropertyOperations.SetLastModificationInfoAdmin(objVersion.ObjVer, true, prop_modifiedby.TypedValue, false, null);
				}

				//replace files
				if (replace_files?.Length > 0 && objVersion.ObjVer.Type == (int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument) {
					SourceObjectFiles objFiles = new SourceObjectFiles();
					foreach (string file_name in replace_files) {
						FileInfo fi = new FileInfo(file_name);
						string title = fi.Name.Replace(fi.Extension, "");
						string ext = fi.Extension.Replace(".", "");
						objFiles.AddFile(title, ext, fi.FullName);
						if (objVersion.SingleFile) {
							IEnumerator fileEnum = objVersion.Files.GetEnumerator();
							fileEnum.MoveNext();
							obj.objVerEx.Vault.ObjectFileOperations.RenameFile(objVersion.ObjVer, (fileEnum.Current as ObjectFile).FileVer, title, ext, false);
							break;
						}
					}
					ObjVerEx checkedOutVerEx = new ObjVerEx(obj.objVerEx.Vault, objVersion);
					checkedOutVerEx.ReplaceFiles(objFiles);
				}

				objVersion = obj.objVerEx.Vault.ObjectOperations.CheckIn(objVerAndProps.ObjVer);
				checkedOutByVAF = false;
				obj.objVerEx.ObjVer.Version = objVersion.ObjVer.Version;
				//
				if (returnUpdatedMetadata) {
					objVerAndProps = obj.objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(obj.objVerEx.ObjID, false);
					obj.objVerEx = new ObjVerEx(obj.objVerEx.Vault, objVerAndProps);
					obj.ClearAllCachedProperties();
				}
			} catch (Exception ex) {
				throw ex;
			} finally {
				if (objVersion != null && checkedOutByVAF)
					// Undo checkout and enforces the operation.
					obj.objVerEx.Vault.ObjectOperations.ForceUndoCheckout(objVersion.ObjVer);
			}

			return true;
		}
		public static bool update_without_returning_content(this IObjVerEx obj, bool skipVafCalculation = false, int? modifiedby_userid = null)
		{
			return obj.update(false, skipVafCalculation, modifiedby_userid);
		}
		public static bool update_without_vaf_calculations(this IObjVerEx obj, bool returnUpdatedMetadata = false, int? modifiedby_userid = null)
		{
			return obj.update(returnUpdatedMetadata, true, modifiedby_userid);
		}
		public static T search<T>(Vault vault, int objTypeId, int objOrExtObjId, bool includeDeleted)
			where T : IObjVerEx, new()
		{
			SearchConditions searchConditions = new SearchConditions();
			SearchCondition searchCondition = new SearchCondition();

			// Object type id condition
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, objTypeId);
			searchConditions.Add(1, searchCondition);

			// External objects condition
			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeExtID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, objOrExtObjId);
			searchConditions.Add(2, searchCondition);

			// Include deleted objects condition
			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, includeDeleted);
			searchConditions.Add(3, searchCondition);

			// Search for external objects
			ObjectSearchResults objSearchResults =
				vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(
					searchConditions, MFSearchFlags.MFSearchFlagNone, false);

			ObjectVersionAndProperties objVerAndProps = null;
			if (objSearchResults != null && objSearchResults.Count > 0) {
				IEnumerator searchResultObjVerEnum = objSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				searchResultObjVerEnum.MoveNext();
				objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
				objVerAndProps = vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, false);
				return new T() { objVerEx = new ObjVerEx(vault, objVerAndProps) };
			} else {
				// Non-external objects condition
				searchCondition = new SearchCondition();
				searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectID);
				searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
				searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeInteger, objOrExtObjId);
				searchConditions.Add(2, searchCondition);

				// Search for non-external objects
				objSearchResults =
					vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(
						searchConditions, MFSearchFlags.MFSearchFlagNone, false);

				if (objSearchResults != null && objSearchResults.Count > 0) {
					IEnumerator searchResultObjVerEnum = objSearchResults.GetAsObjectVersions().GetEnumerator();
					ObjectVersion objectVersion = null;
					searchResultObjVerEnum.MoveNext();
					objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
					objVerAndProps = vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, false);
					return new T() { objVerEx = new ObjVerEx(vault, objVerAndProps) };
				} else {
					return default(T);
				}
			}
		}
		public static List<T> search<T>(Vault vault, Action<SearchParameters> search, int? classId = null, bool includeDeleted = false)
			where T : IObjVerEx, new()
		{
			SearchParameters searchParameter = new SearchParameters();
			search(searchParameter);
			searchParameter.Conditions.ObjectTypeID((int)typeof(T).GetField("TypeID").GetRawConstantValue());
			if (classId != null)
				searchParameter.Conditions.ObjectClassID(classId.Value);
			if (!includeDeleted)
				searchParameter.Conditions.excludeDeleted();
			return vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchParameter.Conditions, MFSearchFlags.MFSearchFlagNone, false)
				.ConvertToOTObjects<T>(vault);
		}

		public static List<T> ConvertToOTObjects<T>(this ObjectSearchResults objectSearchResults, Vault vault)
			where T : IObjVerEx, new()
		{
			List<T> result = new List<T>();
			if (objectSearchResults != null && objectSearchResults.Count > 0) {
				IEnumerator searchResultObjVerEnum = objectSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				searchResultObjVerEnum.MoveNext();
				objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
				ObjectVersionAndProperties objVerAndProps = vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, false);
				result.Add(new T() { objVerEx = new ObjVerEx(vault, objVerAndProps) });
			}
			return result;
		}
		public static SearchConditions AddPropertyCondition<T>(this SearchConditions search, object value, MFConditionType cType = MFConditionType.MFConditionTypeEqual)
			where T : IMFPropertyDefinition
		{
			int pd_id = (int)typeof(T).GetField("id").GetRawConstantValue();
			MFDataType data_type = (MFDataType)typeof(T).GetField("data_type").GetRawConstantValue();

			if (value is IObjVerEx)
				search.AddPropertyCondition(pd_id, cType, data_type, (value as IObjVerEx).objVerEx.ToLookup());
			else if (value is IList) {
				Lookups lookups = new Lookups();
				foreach (IObjVerEx objVerEx in (value as IList)) {
					if (objVerEx == null)
						break;
					lookups.Add(-1, objVerEx.objVerEx.ToLookup());
				}
				search.AddPropertyCondition(pd_id, cType, data_type, lookups);
			} else
				search.AddPropertyCondition(pd_id, cType, data_type, value);
			return search;
		}

		private static ObjectSearchResults getAllSearchResults(this Vault vault, int typeId, bool includeDeleted, int searchResultsCount)
		{
			SearchConditions searchConditions = new SearchConditions();
			SearchCondition searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, typeId);
			searchConditions.Add(-1, searchCondition);
			if (!includeDeleted) {
				searchCondition = new SearchCondition();
				searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
				searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
				searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
				searchConditions.Add(-1, searchCondition);
			}
			return vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions, MFSearchFlags.MFSearchFlagNone, false, searchResultsCount);
		}
		public static List<T> getAll<T>(Vault vault, int typeId, bool includeDeleted, int searchResultsCount)
			where T : IObjVerEx, new()
		{
			ObjectSearchResults objSearchResults = vault.getAllSearchResults(typeId, includeDeleted, searchResultsCount);
			if (objSearchResults == null || objSearchResults.Count == 0)
				return new List<T>();

			List<T> allItems = new List<T>();
			foreach (ObjectVersion objectVersion in objSearchResults.GetAsObjectVersions()) {
				T item = new T() { objVerEx = new ObjVerEx(vault, objectVersion.ObjVer) };
				allItems.Add(item);
			}
			return allItems;
		}
		public static T upload_file<T>(this IObjVerEx obj, string filePath)
			where T : IObjVerEx, new()
		{
			ObjectVersion objVersion = null;
			try {
				obj.undoObjectCheckoutIfCheckedOut();
				objVersion = obj.objVerEx.Vault.ObjectOperations.CheckOut(obj.objVerEx.ObjID);
				FileInfo uploadFileInfo = new FileInfo(filePath);

				obj.objVerEx.Vault.ObjectFileOperations.AddFile(objVersion.ObjVer,
					uploadFileInfo.Name.Replace(uploadFileInfo.Extension, ""),
					uploadFileInfo.Extension.Replace(".", ""),
					uploadFileInfo.FullName);
				obj.objVerEx.Vault.ObjectOperations.CheckIn(objVersion.ObjVer);

				return new T() { objVerEx = obj.objVerEx };
			} catch (Exception exception) {
				if (objVersion != null &&
					obj.objVerEx.Vault.ObjectOperations.IsObjectCheckedOut(objVersion.ObjVer.ObjID, true)) {
					obj.objVerEx.Vault.ObjectOperations.CheckIn(objVersion.ObjVer);
				}
				Console.WriteLine($"Exception during object update: {exception.Message}");
				throw;
			}
		}

		public static bool download_file(Vault Vault, int object_id, string download_path)
		{
			bool success = false;
			try {
				ObjID objId = new ObjID();
				objId.SetIDs((int)MFBuiltInObjectType.MFBuiltInObjectTypeDocument, object_id);
				objId.Type = 0;

				ObjectVersion objectVersion;
				ObjectVersionAndProperties objectVersionAndProperties;
				objectVersionAndProperties = Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objId, true);
				objectVersion = objectVersionAndProperties.VersionData;

				ObjectFiles objectFiles = objectVersion.Files;

				foreach (ObjectFile objectFile in objectFiles) {
					Vault.ObjectFileOperations.DownloadFile(objectFile.ID, objectFile.Version, download_path + objectFile.GetNameForFileSystem());
				}

				success = true;
			} catch (Exception ex) {
				throw ex;
			}
			return success;
		}
		public static void perform_ocr(this IObjVerEx obj)
		{
			OCROptions opts = new OCROptions();
			opts.PrimaryLanguage = MFOCRLanguage.MFOCRLanguageEnglishUS;

			obj.objVerEx.CheckOut();
			ObjectFiles files = obj.objVerEx.Vault.ObjectFileOperations.GetFiles(obj.objVerEx.ObjVer);
			foreach (ObjectFile file in files) {
				switch (file.Extension.ToLower()) {
				case "tif":
				case "tiff":
				case "jpg":
				case "jpeg":
				case "pdf":
					obj.objVerEx.Vault.ObjectFileOperations.PerformOCROperation(obj.objVerEx.ObjVer, file.FileVer,
					   opts, MFOCRZoneRecognitionMode.MFOCRZoneRecognitionModeNoZoneRecognition, null, true);
					break;
				default:
					break;
				}
			}
			obj.objVerEx.CheckIn();
		}
		public static void send_notification_email(this IObjVerEx obj, Dictionary<int, MFUserOrUserGroupType> recipients, List<string> externalRecipientsList, string subject, string body)
		{
			UserOrUserGroupIDs recipientUserOrUserGroups = new UserOrUserGroupIDs();

			if (recipients != null && recipients.Count > 0) {
				UserOrUserGroupID recipientUserOrUserGroup = null;

				foreach (KeyValuePair<int, MFUserOrUserGroupType> recipient in recipients) {
					recipientUserOrUserGroup = new UserOrUserGroupID();
					recipientUserOrUserGroup.UserOrGroupID = recipient.Key;
					recipientUserOrUserGroup.UserOrGroupType = recipient.Value;
					recipientUserOrUserGroups.Add(-1, recipientUserOrUserGroup);
				}
			}

			Strings externalRecipients = new Strings();
			if (externalRecipientsList != null && externalRecipientsList.Count > 0)
				externalRecipientsList.ForEach(externalRecipient => externalRecipients.Add(-1, externalRecipient));

			obj.objVerEx.Vault.NotificationOperations.SendCustomNotification(recipientUserOrUserGroups, false, externalRecipients, true, subject, body);
		}
		private static void undoObjectCheckoutIfCheckedOut(this IObjVerEx obj)
		{
			if (obj.objVerEx.Vault.ObjectOperations.IsObjectCheckedOut(obj.objVerEx.ObjID, true))
				obj.objVerEx.Vault.ObjectOperations.ForceUndoCheckout(obj.objVerEx.ObjVer);
		}
		public static void replace_files(this OT_Document document, string singledoc_newfilepath = null, string[] multidoc_newfilespath = null)
		{
			Vault vault = document.objVerEx.Vault;
			var obj = document.objVerEx;
			bool isSingleFile = vault.ObjectOperations.IsSingleFileObject(obj.ObjVer);

			//singlefile document
			if (isSingleFile && string.IsNullOrEmpty(singledoc_newfilepath))
				throw new Exception("No file supplied for single-file document.");

			//multifile document
			if (!isSingleFile && (multidoc_newfilespath == null || multidoc_newfilespath.Length == 0))
				throw new Exception("No file(s) supplied for multi-file document.");

			ObjectVersion new_version = vault.ObjectOperations.CheckOut(obj.ObjID);

			try {

				ObjVerEx new_versionex = new ObjVerEx(vault, new_version);
				SourceObjectFiles files = new SourceObjectFiles();

				if (isSingleFile) {
					FileInfo fi = new FileInfo(singledoc_newfilepath);
					files.AddFile(fi.Name.Replace(fi.Extension, ""), fi.Extension.Replace(".", ""), fi.FullName);

					//change the file type
					IEnumerator obfile_enumerator = vault.ObjectFileOperations.GetFiles(new_version.ObjVer).GetEnumerator();
					obfile_enumerator.MoveNext();
					ObjectFile single_fileobj = obfile_enumerator.Current as ObjectFile;
					if (single_fileobj == null)
						throw new Exception("Single file for this document not found.");
					vault.ObjectFileOperations.RenameFile(new_versionex.ObjVer, single_fileobj.FileVer, fi.Name.Replace(fi.Extension, ""), fi.Extension.Replace(".", ""), false);
				} else {
					foreach (string file in multidoc_newfilespath) {
						FileInfo fi = new FileInfo(file);
						files.AddFile(fi.Name.Replace(fi.Extension, ""), fi.Extension.Replace(".", ""), fi.FullName);
					}
				}
				new_versionex.ReplaceFiles(files);
				new_versionex.CheckIn();
				document.objVerEx = new_versionex;

			} catch {
				if (new_version.ObjectCheckedOut)
					vault.ObjectOperations.ForceUndoCheckout(new_version.ObjVer);
				throw;
			}
		}
		public static OT_Document create_singlefile_doc(this OT_Document doc, string filepath, int? createdby_mfuserid = null)
		{
			FileInfo fi = new FileInfo(filepath);
			ObjectVersionAndProperties obj = doc.objVerEx.Vault.ObjectOperations.CreateNewSFDObject(OT_Document.TypeID, doc.objVerEx.Properties,
				new SourceObjectFile {
					SourceFilePath = fi.FullName,
					Title = fi.Name.Replace(fi.Extension, ""),
					Extension = fi.Extension.Replace(".", "")
				}, createdby_mfuserid == null);
			if (createdby_mfuserid != null) {
				TypedValue created_by = new TypedValue();
				created_by.SetValue(MFDataType.MFDatatypeLookup, createdby_mfuserid.Value);
				obj = doc.objVerEx.Vault.ObjectPropertyOperations.SetCreationInfoAdmin(obj.ObjVer, true, created_by, false, null);
				doc.objVerEx.Vault.ObjectOperations.CheckIn(obj.ObjVer);
			}

			return new OT_Document() { objVerEx = new ObjVerEx(obj.Vault, obj) };
		}
		public static OT_Document create_multifile_doc(this OT_Document doc, int? createdby_mfuserid = null, params string[] files)
		{
			Vault vault = doc.objVerEx.Vault;
			ObjVerEx version = doc.objVerEx;
			SourceObjectFiles sourceFiles = new SourceObjectFiles();
			foreach (string file in files) {
				FileInfo fi = new FileInfo(file);
				sourceFiles.AddFile(fi.Name.Replace(fi.Extension, ""), fi.Extension.Replace(".", ""), fi.FullName);
			}
			ObjectVersionAndProperties obj = vault.ObjectOperations.CreateNewObjectEx(OT_Document.TypeID, version.Properties, sourceFiles, false,
				createdby_mfuserid == null);

			if (createdby_mfuserid != null) {
				TypedValue created_by = new TypedValue();
				created_by.SetValue(MFDataType.MFDatatypeLookup, createdby_mfuserid.Value);
				obj = doc.objVerEx.Vault.ObjectPropertyOperations.SetCreationInfoAdmin(obj.ObjVer, true, created_by, false, null);
				doc.objVerEx.Vault.ObjectOperations.CheckIn(obj.ObjVer);
			}

			return new OT_Document() { objVerEx = new ObjVerEx(obj.Vault, obj) };
		}

		public static List<T> search_property_definition<T, T2>(this IObjVerEx obj, int propertyDefinitionID, T2 value, bool includeDeleted = false)
			where T : IObjVerEx, new()
		{
			SearchCondition searchCondition;
			SearchConditions searchConditions = new SearchConditions();

			Type t = typeof(T);
			object o = Activator.CreateInstance<T>();
			FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			int oid = -1;
			foreach (FieldInfo f in fields)
				if (f.Name == "TypeID")
					oid = Convert.ToInt32(f.GetValue(o));

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, oid);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinitionID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, value);
			searchConditions.Add(-1, searchCondition);

			if (!includeDeleted) {
				searchCondition = new SearchCondition();
				searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
				searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
				searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
				searchConditions.Add(-1, searchCondition);
			}

			ObjectSearchResults objectSearchResults =
				obj.objVerEx.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions, MFSearchFlags.MFSearchFlagNone, false);

			List<T> searchedOT = new List<T>();
			if (objectSearchResults != null && objectSearchResults.Count > 0) {
				IEnumerator searchResultObjVerEnum = objectSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				while (searchResultObjVerEnum.MoveNext()) {
					objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
					ObjectVersionAndProperties objVerAndProps =
						obj.objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, true);
					T item = new T() { objVerEx = new ObjVerEx(obj.objVerEx.Vault, objVerAndProps) };
					searchedOT.Add(item);
				}
			}
			return searchedOT;
		}

		public static List<T> search_property_definition<T, T2>(this IObjVerEx obj, int propertyDefinitionID, IObjVerEx value, int propertyDefinition2ID, IObjVerEx value2, bool includeDeleted = false)
			where T : IObjVerEx, new()
		{
			SearchCondition searchCondition;
			SearchConditions searchConditions = new SearchConditions();

			Type t = typeof(T);
			object o = Activator.CreateInstance<T>();
			FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			int oid = -1;
			foreach (FieldInfo f in fields)
				if (f.Name == "TypeID")
					oid = Convert.ToInt32(f.GetValue(o));

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, oid);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinitionID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, value);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinition2ID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, value2.objVerEx.ToLookup());
			searchConditions.Add(-1, searchCondition);

			if (!includeDeleted) {
				searchCondition = new SearchCondition();
				searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
				searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
				searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
				searchConditions.Add(-1, searchCondition);
			}

			ObjectSearchResults objectSearchResults =
				obj.objVerEx.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions, MFSearchFlags.MFSearchFlagNone, false);

			List<T> searchedOT = new List<T>();
			if (objectSearchResults != null && objectSearchResults.Count > 0) {
				IEnumerator searchResultObjVerEnum = objectSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				while (searchResultObjVerEnum.MoveNext()) {
					objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
					ObjectVersionAndProperties objVerAndProps =
						obj.objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, true);
					T item = new T() { objVerEx = new ObjVerEx(obj.objVerEx.Vault, objVerAndProps) };
					searchedOT.Add(item);
				}
			}
			return searchedOT;
		}

		public static List<T> search_property_definition<T, T2>(this IObjVerEx obj, int propertyDefinitionID, T2 value, int propertyDefinition2ID, IObjVerEx value2, int propertyDefinition3ID, IObjVerEx value3, bool includeDeleted = false)
			where T : IObjVerEx, new()
		{
			SearchCondition searchCondition;
			SearchConditions searchConditions = new SearchConditions();

			Type t = typeof(T);
			object o = Activator.CreateInstance<T>();
			FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			int oid = -1;
			foreach (FieldInfo f in fields)
				if (f.Name == "TypeID")
					oid = Convert.ToInt32(f.GetValue(o));

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeObjectTypeID);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, oid);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinitionID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeText, value);
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinition2ID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, value2.objVerEx.ToLookup());
			searchConditions.Add(-1, searchCondition);

			searchCondition = new SearchCondition();
			searchCondition.Expression.SetPropertyValueExpression(propertyDefinition3ID, MFParentChildBehavior.MFParentChildBehaviorNone);
			searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
			searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeLookup, value3.objVerEx.ToLookup());
			searchConditions.Add(-1, searchCondition);

			if (!includeDeleted) {
				searchCondition = new SearchCondition();
				searchCondition.Expression.SetStatusValueExpression(MFStatusType.MFStatusTypeDeleted);
				searchCondition.ConditionType = MFConditionType.MFConditionTypeEqual;
				searchCondition.TypedValue.SetValue(MFDataType.MFDatatypeBoolean, false);
				searchConditions.Add(-1, searchCondition);
			}

			ObjectSearchResults objectSearchResults =
				obj.objVerEx.Vault.ObjectSearchOperations.SearchForObjectsByConditionsEx(searchConditions, MFSearchFlags.MFSearchFlagNone, false);

			List<T> searchedOT = new List<T>();
			if (objectSearchResults != null && objectSearchResults.Count > 0) {
				IEnumerator searchResultObjVerEnum = objectSearchResults.GetAsObjectVersions().GetEnumerator();
				ObjectVersion objectVersion = null;
				while (searchResultObjVerEnum.MoveNext()) {
					objectVersion = (ObjectVersion)searchResultObjVerEnum.Current;
					ObjectVersionAndProperties objVerAndProps =
						obj.objVerEx.Vault.ObjectOperations.GetLatestObjectVersionAndProperties(objectVersion.ObjVer.ObjID, true);
					T item = new T() { objVerEx = new ObjVerEx(obj.objVerEx.Vault, objVerAndProps) };
					searchedOT.Add(item);
				}
			}
			return searchedOT;
		}
	}
}