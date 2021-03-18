using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP2013F.Stock.Extensions
{
    public static class SPListItemExtensions
    {
        public static void SetLookupValue(this SPListItem listItem, string fieldName, SPListItem toListItem)
        {
            listItem[fieldName] = new SPFieldLookupValue(toListItem.ID, toListItem.Title);
        }

        public static SPFieldLookupValue GetLookupValue(this SPListItem listItem, string fieldName)
        {
            return listItem[fieldName] != null ? new SPFieldLookupValue(listItem[fieldName].ToString()) : null;
        }

        public static SPFieldLookupValue GetLookupValue(this SPListItem listItem, Guid fieldId)
        {
            return listItem[fieldId] != null ? new SPFieldLookupValue(listItem[fieldId].ToString()) : null;
        }

        public static void SetUser(this SPListItem listItem, string fieldName, SPUser user)
        {
            listItem[fieldName] = new SPFieldUserValue(listItem.Web, user.ID.ToString());
        }

        private static SPUser[] GetUsers(SPListItem listItem, object value, bool includeAdGroups = true)
        {
            if (value is string s) // single value mode
            {
                var user = new SPFieldUserValue(listItem.Web, s).User;
                if (user == null || (!includeAdGroups && user.UserId == null)) return new SPUser[0];
                return new[] { user };
            }

            if (value is SPFieldUserValueCollection collection) // multiple value mode
            {
                if (includeAdGroups)
                    return collection.Where(x => x.User != null).Select(x => x.User).ToArray();
                else
                    return collection.Where(x => x.User?.UserId != null).Select(x => x.User).ToArray();
            }

            return new SPUser[0];
        }

        public static SPUser[] GetUsers(this SPListItem listItem, string fieldDisplayName, bool includeAdGroups = true)
        {
            var value = listItem[fieldDisplayName];
            return GetUsers(listItem, value, includeAdGroups);
        }

        public static SPUser[] GetUsers(this SPListItem listItem, Guid fieldId, bool includeAdGroups = true)
        {
            var value = listItem[fieldId];
            return GetUsers(listItem, value, includeAdGroups);
        }

        public static Guid? GetGuid(this SPListItem listItem, string fieldName)
        {
            var value = listItem[fieldName] as string;
            return value == null ? (Guid?)null : Guid.Parse(value);
        }

        public static string[] GetChoices(this SPListItem listItem, Guid fieldId)
        {
            var value = listItem[fieldId];
            if (value == null) return new string[0];
            var choices = new SPFieldMultiChoiceValue(value.ToString());
            var result = new List<string>();
            for (var i = 0; i < choices.Count; i++) result.Add(choices[i]);
            return result.ToArray();
        }

        public static string[] GetChoices(this SPListItem listItem, string fieldName)
        {
            var value = listItem[fieldName];
            if (value == null) return new string[0];
            var choices = new SPFieldMultiChoiceValue(value.ToString());
            var result = new List<string>();
            for (var i = 0; i < choices.Count; i++) result.Add(choices[i]);
            return result.ToArray();
        }

        public static bool? GetBoolean(this SPListItem listItem, string fieldName)
        {
            return listItem[fieldName] as bool?;
        }

        public static DateTime? GetDateTime(this SPListItem listItem, string fieldName)
        {
            return listItem[fieldName] as DateTime?;
        }

        public static void RemovePermissions(this SPListItem listItem, string spGroupName, bool updateItem = true)
        {
            var group = listItem.ParentList.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            listItem.RoleAssignments.Remove(group);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void AddPermissions(this SPListItem listItem, string spGroupName, string roleName, bool updateItem = true)
        {
            var group = listItem.ParentList.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            var role = listItem.Web.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(role);
            listItem.RoleAssignments.Add(roleAssignment);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void AddPermissions(this SPListItem listItem, string spGroupName, SPRoleType type, bool updateItem = true)
        {
            var group = listItem.ParentList.ParentWeb.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");;
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(listItem.Web.RoleDefinitions.GetByType(type));
            listItem.RoleAssignments.Add(roleAssignment);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void AddPermissionsForUser(this SPListItem listItem, SPUser user, string roleName, bool updateItem = true)
        {
            var role = listItem.Web.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var roleAssignment = new SPRoleAssignment(user);
            roleAssignment.RoleDefinitionBindings.Add(role);
            listItem.RoleAssignments.Add(roleAssignment);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void AddPermissionsForUser(this SPListItem listItem, SPUser user, SPRoleType type, bool updateItem = true)
        {
            var roleAssignment = new SPRoleAssignment(user);
            roleAssignment.RoleDefinitionBindings.Add(listItem.Web.RoleDefinitions.GetByType(type));
            listItem.RoleAssignments.Add(roleAssignment);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void RemovePermissionsForUser(this SPListItem listItem, SPUser user, bool updateItem = true)
        {
            listItem.RoleAssignments.Remove(user);
            if (updateItem) listItem.SystemUpdate();
        }

        public static void RemovePermissionsForUser(this SPListItem listItem, string adLogonName, bool updateItem = true)
        {
            var adUser = listItem.Web.EnsureUser(adLogonName);
            listItem.RoleAssignments.Remove(adUser);
            if (updateItem) listItem.SystemUpdate();
        }

        public static SPUser GetAuthor(this SPListItem listItem)
        {
            return new SPFieldUserValue(listItem.Web, listItem["Author"].ToString()).User;
        }

        public static string GetDisplayFormUrl(this SPListItem listItem)
        {
            return listItem.ParentList.ParentWeb.Site.MakeFullUrl(listItem.ParentList.DefaultDisplayFormUrl) + "?ID=" + listItem.ID;
        }

        public static void SetTitle(this SPListItem listItem, string title, bool updateItem = true)
        {
            listItem[SPBuiltInFieldId.Title] = title;
            if (updateItem) listItem.Update();
        }

        public static DateTime GetCreationTime(this SPListItem listItem)
        {
            return (DateTime)listItem[SPBuiltInFieldId.Created];
        }

        public static DateTime GetModificationTime(this SPListItem listItem)
        {
            return (DateTime)listItem[SPBuiltInFieldId.Modified];
        }
    }
}
