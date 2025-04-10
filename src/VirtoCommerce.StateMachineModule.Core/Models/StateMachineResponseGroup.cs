using System;

namespace VirtoCommerce.StateMachineModule.Core.Models;
[Flags]
public enum StateMachineResponseGroup
{
    None = 0,
    WithLocalization = 1,
    Full = None | WithLocalization
}
