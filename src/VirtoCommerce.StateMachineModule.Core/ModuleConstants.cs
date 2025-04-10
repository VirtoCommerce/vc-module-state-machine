using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.StateMachineModule.Core;
[ExcludeFromCodeCoverage]
public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "statemachine:access";
            public const string Create = "statemachine:create";
            public const string Read = "statemachine:read";
            public const string Update = "statemachine:update";
            public const string Delete = "statemachine:delete";
            public const string Fire = "statemachine:fire";
            public const string Localize = "statemachine:localize";

            public static string[] AllPermissions { get; } =
            {
                Access,
                Create,
                Read,
                Update,
                Delete,
                Fire,
                Localize,
            };
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor StateMachineModuleEnabled { get; } = new()
            {
                Name = "StateMachineModule.StateMachineModuleEnabled",
                GroupName = "StateMachineModule|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static SettingDescriptor StateMachineModulePassword { get; } = new()
            {
                Name = "StateMachineModule.StateMachineModulePassword",
                GroupName = "StateMachineModule|Advanced",
                ValueType = SettingValueType.SecureString,
                DefaultValue = "qwerty",
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return StateMachineModuleEnabled;
                    yield return StateMachineModulePassword;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
