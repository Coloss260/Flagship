using Content.FlagShip.Shared.CCVar;
using Content.FlagShip.Shared.Company;
using Robust.Shared.Configuration;

namespace Content.FlagShip.Client.Company;

public sealed partial class CompanyManager
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private IConfigurationManager _config = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private HashSet<ProtoId<CompanyPrototype>> _whitelist = new();

    public void Initialize()
    {
        _net.RegisterNetMessage<MsgCompanyWhitelist>(OnWhitelistMsg);
    }

    private void OnWhitelistMsg(MsgCompanyWhitelist msg)
    {
        _whitelist = msg.Whitelist;
    }

    public bool IsPlayerWhitelisted(ProtoId<CompanyPrototype> company)
    {
        return _whitelist.Contains(company);
    }

    public bool IsAllowed(ProtoId<CompanyPrototype> company)
    {
        if (!_config.GetCVar(MonoCVars.CompanyWhitelist))
            return true;

        if (!_proto.Resolve(company, out var companyPrototype) ||
            !companyPrototype.Whitelisted)
        {
            return true;
        }

        return IsPlayerWhitelisted(company);
    }
}
