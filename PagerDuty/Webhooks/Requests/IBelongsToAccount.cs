using System;

namespace Pager.Duty.Webhooks.Requests;

public interface IBelongsToAccount {

    /// <summary>
    /// <para>The URL subdomain for the account, organization, company, tenant, or customer. Excludes the base domain/SLD (<c>pagerduty.com</c>).</para>
    /// <para>For example, if your PagerDuty account origin that you log in to with your web browser is <c>https://mycompany.pagerduty.com/</c>, then this would return <c>mycompany</c>.</para>
    /// </summary>
    public string? AccountSubdomain { get; }

}

internal static class BelongsToAccountHelper {

    private static readonly string[] BaseDomains = [".eu.pagerduty.com", ".pagerduty.com"];

    public static string? GetAccountSubdomain(Uri? htmlUrl) {
        if (htmlUrl is null) return null;
        string host = htmlUrl.Host;
        int    end  = -1;
        for (int i = 0; end == -1 && i < BaseDomains.Length; i++) {
            end = host.LastIndexOf(BaseDomains[i], StringComparison.OrdinalIgnoreCase);
        }
        return end == -1 ? host : host.Substring(0, end);
    }

}