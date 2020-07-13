using System;
using System.Collections.Generic;

using MFilesAPI;

namespace ApplicationDevelopmentKit
{
	public class Api
	{
		public VaultsOnServer Vaults { get; set; }
		public ObjectTypes ObjectTypes { get; set; }
		public ClassTypes ClassTypes { get; set; }
		public PropertyDefinitions PropertyDefinitions { get; set; }
		public ValueLists ValueLists { get; set; }
		public Users Users { get; set; }
		public UserGroups UserGroups { get; set; }
		public NamedACLs NamedACLs { get; set; }
		public Dictionary<string, IEnumerable<MFilesObject>> Structure { get; set; }
		public bool HasWorkflowStates { get; set; }
		public AbstractMFDevelopmentKit DevelopertTool { get; set; }
		public MFilesSettings MFilesSettings { get; set; }

		public Api(AbstractMFDevelopmentKit developmentTool)
		{
			DevelopertTool = developmentTool;
			MFilesSettings = DevelopertTool.MFilesSettings;
			Vaults = DevelopertTool.Vaults;
		}
		public void Dispose()
		{
			DevelopertTool?.Dispose();
		}
		public void InitObjectTypes()
		{
			Console.WriteLine("[INFO] Initializing ObjectTypes...");
			ObjectTypes = new ObjectTypes(DevelopertTool.Vault);
		}
		public void InitClassTypes()
		{
			Console.WriteLine("[INFO] Initializing ClassTypes...");
			ClassTypes = new ClassTypes(DevelopertTool.Vault);
		}
		public void InitPropertyDefinitions()
		{
			Console.WriteLine("[INFO] Initializing PropertyDefinitions...");
			PropertyDefinitions = new PropertyDefinitions(DevelopertTool.Vault);
		}
		public void InitValueLists()
		{
			Console.WriteLine("[INFO] Initializing ValueLists...");
			ValueLists = new ValueLists(DevelopertTool.Vault);
		}
		public void InitWorkflowWorkflowStates()
		{
			Console.WriteLine("[INFO] Initializing Workflow/WorkflowStates...");
			HasWorkflowStates = Workflows.HasWorkflowStates(DevelopertTool.Vault);
		}
		public void InitStructureEnums()
        {
            Console.WriteLine("Initializing Structure Enums...");
            Structure = new Dictionary<string, IEnumerable<MFilesObject>>();

            if (ObjectTypes == null)
                ObjectTypes = new ObjectTypes(DevelopertTool.Vault);
            Structure["Object"] = ObjectTypes;

            if (ClassTypes == null)
                ClassTypes = new ClassTypes(DevelopertTool.Vault);
            Structure["Class"] = ClassTypes;

            if (PropertyDefinitions == null)
                PropertyDefinitions = new PropertyDefinitions(DevelopertTool.Vault);
            Structure["PropertyDefinition"] = PropertyDefinitions;

            if (ValueLists == null)
                ValueLists = new ValueLists(DevelopertTool.Vault);
            // Add later
        }
	}
}
