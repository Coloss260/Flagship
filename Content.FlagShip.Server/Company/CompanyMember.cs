using Content.FlagShip.Shared.Company;
using Robust.Shared.Prototypes;

namespace Content.FlagShip.Server.Company;

public struct CompanyMemberRecord
{
    public Guid PlayerUserId;
    public string LastSeenUserName;
    public bool Owner;
    public ProtoId<CompanyPrototype> Company;
}
