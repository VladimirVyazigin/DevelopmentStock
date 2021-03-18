using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Extensions
{
    public static class SPListExtensions
    {
        public static SPListItem[] GetItemsByTitle(this SPList list, string title)
        {
            var query = new SPQuery() { Query = $@"<Where>
                                                      <Eq>
                                                         <FieldRef Name='LinkTitle' />
                                                         <Value Type='Computed'>{title}</Value>
                                                      </Eq>
                                                   </Where>", ViewAttributes = "Scope=\"RecursiveAll\"" };
            var items = list.GetItems(query);
            return items.OfType<SPListItem>().ToArray();
        }

        public static SPListItem TryGetItemById(this SPList list, int id)
        {
            try
            {
                return list.GetItemById(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void RemovePermissions(this SPList list, string spGroupName, bool updateItem = true)
        {
            var group = list.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            list.RoleAssignments.Remove(group);
            if (updateItem) list.Update();
        }

        public static void RemovePermissionsForAdObject(this SPList list, string adLogonName, bool updateItem = true)
        {
            var adUser = list.ParentWeb.EnsureUser(adLogonName);
            list.RoleAssignments.Remove(adUser);
            if (updateItem) list.Update();
        }

        public static void AddPermissionsForUser(this SPList list, string adLogonName, string roleName, bool updateItem = true)
        {
            var role = list.ParentWeb.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var adUser = list.ParentWeb.EnsureUser(adLogonName);
            var roleAssignment = new SPRoleAssignment(adUser);
            roleAssignment.RoleDefinitionBindings.Add(role);
            list.RoleAssignments.Add(roleAssignment);
            if (updateItem) list.Update();
        }

        public static void AddPermissionsForUser(this SPList list, string adLogonName, SPRoleType type, bool updateItem = true)
        {
            var adUser = list.ParentWeb.EnsureUser(adLogonName);
            var roleAssignment = new SPRoleAssignment(adUser);
            roleAssignment.RoleDefinitionBindings.Add(list.ParentWeb.RoleDefinitions.GetByType(type));
            list.RoleAssignments.Add(roleAssignment);
            if (updateItem) list.Update();
        }

        public static void AddPermissions(this SPList list, string spGroupName, string roleName, bool updateItem = true)
        {
            var group = list.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            var role = list.ParentWeb.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(role);
            list.RoleAssignments.Add(roleAssignment);
            if (updateItem) list.Update();
        }

        public static void AddPermissions(this SPList list, string spGroupName, SPRoleType type, bool updateItem = true)
        {
            var group = list.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(list.ParentWeb.RoleDefinitions.GetByType(type));
            list.RoleAssignments.Add(roleAssignment);
            if (updateItem) list.Update();
        }
    }
}
