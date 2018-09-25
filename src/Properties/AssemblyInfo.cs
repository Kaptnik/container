using System;
using System.Security;

[assembly:CLSCompliant(true)]
[assembly: SecurityTransparent]
[assembly: AllowPartiallyTrustedCallers]

#if !NETSTANDARD1_0 && !NETCOREAPP1_0
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#endif