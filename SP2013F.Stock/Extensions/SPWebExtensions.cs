using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Xml;

namespace SP2013F.Stock.Extensions
{
    public static class SPWebExtensions
    {
        public static SPList[] GetDocumentLibraries(this SPWeb web, bool includeHidden = false)
        {
            var libraries = web.GetListsOfType(SPBaseType.DocumentLibrary).OfType<SPList>();
            return includeHidden ? libraries.ToArray() : libraries.Where(x => !x.Hidden).ToArray();
        }

        public static SPList TryGetList(this SPWeb web, string listUrl)
        {
            if (!string.IsNullOrEmpty(web.ServerRelativeUrl))
                listUrl = SPUrlUtility.CombineUrl(web.ServerRelativeUrl, listUrl);

            try
            {
                return web.GetList(listUrl);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static SPList TryGetList(this SPWeb web, Guid listId)
        {
            try
            {
                return web.Lists[listId];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static SPUser TryGetUser(this SPWeb web, int userId)
        {
            try
            {
                return web.AllUsers.GetByID(userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void AsElevated(this SPWeb web, Action<SPWeb> secureCode)
        {
            if (web == null) throw new ArgumentNullException("web");
            if (secureCode == null) throw new ArgumentNullException("secureCode");

            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var elevatedSite = new SPSite(web.Site.ID))
                {
                    using (var elevatedWeb = elevatedSite.OpenWeb(web.ID))
                    {
                        secureCode(elevatedWeb);
                    }
                }
            });
        }

        public static T AsElevated<T>(this SPWeb web, Func<SPWeb, T> secureCode)
        {
            if (web == null) throw new ArgumentNullException("web");
            if (secureCode == null) throw new ArgumentNullException("secureCode");

            T result = default(T);
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (SPSite elevatedSite = new SPSite(web.Site.ID))
                {
                    using (SPWeb elevatedWeb = elevatedSite.OpenWeb(web.ID))
                    {
                        result = secureCode(elevatedWeb);
                    }
                }
            });
            return result;
        }

        #region Permissions management

        public static SPRoleDefinition TryGetRoleDefinition(this SPWeb web, string roleName)
        {
            return web.RoleDefinitions.OfType<SPRoleDefinition>().FirstOrDefault(x => x.Name == roleName);
        }

        public static SPRoleDefinition AddRoleDefinition(this SPWeb web, string roleName, SPBasePermissions permissions,
            string description = null)
        {
            var roleDef = new SPRoleDefinition
            {
                Name = roleName,
                Description = description,
                BasePermissions = permissions
            };
            web.RoleDefinitions.Add(roleDef);
            return roleDef;
        }

        public static SPRoleDefinition EnsureRoleDefinition(this SPWeb web, string roleName, SPBasePermissions permissions,
            string description = null)
        {
            return web.TryGetRoleDefinition(roleName) ?? web.AddRoleDefinition(roleName, permissions, description);
        }

        #region SPGroup permissions

        public static void RemovePermissions(this SPWeb web, string spGroupName, bool updateWeb = true)
        {
            var group = web.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            web.RoleAssignments.Remove(group);
            if (updateWeb) web.Update();
        }

        public static void AddPermissions(this SPWeb web, string spGroupName, string roleName, bool updateWeb = true)
        {
            var group = web.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            var role = web.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(role);
            web.RoleAssignments.Add(roleAssignment);
            if (updateWeb) web.Update();
        }

        public static void AddPermissions(this SPWeb web, string spGroupName, SPRoleType type, bool updateWeb = true)
        {
            var group = web.TryGetGroup(spGroupName);
            if (group == null) throw new ArgumentException($"Unable to find SP group with name '{spGroupName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(web.RoleDefinitions.GetByType(type));
            web.RoleAssignments.Add(roleAssignment);
            if (updateWeb) web.Update();
        }

        public static void AddPermissions(this SPWeb web, SPGroup group, SPRoleType type, bool updateWeb = true)
        {
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(web.RoleDefinitions.GetByType(type));
            web.RoleAssignments.Add(roleAssignment);
            if (updateWeb) web.Update();
        }

        public static void AddPermissions(this SPWeb web, SPGroup group, string roleName, bool updateWeb = true)
        {
            var role = web.TryGetRoleDefinition(roleName);
            if (role == null) throw new ArgumentException($"Unable to find role with name '{roleName}'");
            var roleAssignment = new SPRoleAssignment(group);
            roleAssignment.RoleDefinitionBindings.Add(role);
            web.RoleAssignments.Add(roleAssignment);
            if (updateWeb) web.Update();
        }

        #endregion

        #endregion

        #region SPGroup management

        public static SPGroup TryGetGroup(this SPWeb web, string groupName)
        {
            return web.SiteGroups.OfType<SPGroup>().FirstOrDefault(x => x.Name == groupName);
        }

        public static SPGroup AddGroup(this SPWeb web, string groupName, SPMember owner = null, SPUser defaultUser = null, string groupDescription = null)
        {
            web.SiteGroups.Add(groupName, owner ?? web.Author, defaultUser ?? web.Author, groupDescription ?? "");
            return web.SiteGroups[groupName];
        }

        public static SPGroup EnsureGroup(this SPWeb web, string groupName, SPMember owner = null, SPUser defaultUser = null, string groupDescription = null)
        {
            return web.TryGetGroup(groupName) ?? web.AddGroup(groupName, owner, defaultUser, groupDescription);
        }

        public static void RemoveGroup(this SPWeb web, string groupName)
        {
            web.SiteGroups.Remove(groupName);
        }

        #endregion

        public static bool SendEmail(this SPWeb web, string to, string subject, string body)
        {
            try
            {
                return SPUtility.SendEmail(web, true, false, to, subject, body);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void EnsureContentEditorWebPart(this SPWeb web, string fileUrl, string zoneId, string webPartTitle,
            string webPartContent, int zoneIndex = 0)
        {
            using (var manager = web.GetLimitedWebPartManager(fileUrl, PersonalizationScope.Shared))
            {
                var webPart = manager.WebParts.OfType<ContentEditorWebPart>().FirstOrDefault(x => x.Title == webPartTitle);
                if (webPart != null) return;
                webPart = new ContentEditorWebPart();
                webPart.Title = webPartTitle;
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlElement = xmlDoc.CreateElement("Root");
                xmlElement.InnerText = webPartContent;
                webPart.Content = xmlElement;
                manager.AddWebPart(webPart, zoneId, zoneIndex);
            }
        }

        public static void EnsureScriptEditorWebPart(this SPWeb web, string fileUrl, string zoneId, string webPartTitle,
            string webPartContent, int zoneIndex = 0)
        {
            using (var manager = web.GetLimitedWebPartManager(fileUrl, PersonalizationScope.Shared))
            {
                var webPart = manager.WebParts.OfType<ScriptEditorWebPart>().FirstOrDefault(x => x.Title == webPartTitle);
                if (webPart != null) return;
                webPart = new ScriptEditorWebPart();
                webPart.Title = webPartTitle;
                webPart.Content = webPartContent;
                manager.AddWebPart(webPart, zoneId, zoneIndex);
            }
        }
    }
}
