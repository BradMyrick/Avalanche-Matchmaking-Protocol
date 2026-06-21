// AMP Unreal module build config.
//
// Links the AMP C++ SDK (capnp-rpc) and exposes Blueprint-callable wrappers.
// Cap'n Proto and (optionally) libkj-tls are expected in a ThirdParty module
// or via the project's <Project>/Source/ThirdParty/AMPCapnp. See
// amp-sdk/cpp/unreal/AMP/README.md for the two supported bring-your-own-capnp
// paths (UE-bundled vs. system capnp).
using UnrealBuildTool;

public class AMP : ModuleRules
{
    public AMP(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(new string[] {
            "Core",
            "CoreUObject",
            "Engine",
        });

        // Path to the SDK root: <plugin>/Source/AMP/../../../..
        string SDKRoot = System.IO.Path.Combine(ModuleDirectory, "..", "..", "..", "..");

        // The SDK client + headers.
        PublicIncludePaths.Add(System.IO.Path.Combine(SDKRoot, "cpp", "include"));

        // Generated capnp headers (rebuilt by generate_bindings.sh).
        PublicIncludePaths.Add(System.IO.Path.Combine(SDKRoot, "schemas", "generated", "cpp"));

        // Compile the SDK client directly into the module so a separate static
        // lib is not required.
        PrivateIncludePaths.Add(System.IO.Path.Combine(SDKRoot, "cpp", "src"));

        // ─── Cap'n Proto / KJ bring-your-own ───────────────────────────
        // Option A (recommended): drop capnp 1.3+ headers + libs into
        //   <Project>/Source/ThirdParty/AMPCapnp/{include,lib}
        // and uncomment the following:
        //
        // string CapnpRoot = System.IO.Path.Combine(ModuleDirectory,
        //     "..", "..", "..", "..", "..", "..", "Source", "ThirdParty", "AMPCapnp");
        // PublicIncludePaths.Add(System.IO.Path.Combine(CapnpRoot, "include"));
        // PublicAdditionalLibraries.Add(System.IO.Path.Combine(CapnpRoot, "lib", "libcapnp-rpc.a"));
        // PublicAdditionalLibraries.Add(System.IO.Path.Combine(CapnpRoot, "lib", "libcapnp.a"));
        // PublicAdditionalLibraries.Add(System.IO.Path.Combine(CapnpRoot, "lib", "libkj-async.a"));
        // PublicAdditionalLibraries.Add(System.IO.Path.Combine(CapnpRoot, "lib", "libkj.a"));
        //
        // Option B: system capnp (Linux editor builds only).
        if (Target.Platform == UnrealTargetPlatform.Linux)
        {
            PublicSystemLibraries.AddRange(new string[] {
                "capnp-rpc", "capnp", "kj-async", "kj"
            });
        }

        // Enable the kj-coroutine async API when the bundled KJ was built with
        // coroutine support. Off by default for maximum portability.
        // PublicDefinitions.Add("AMP_USE_COROUTINES=1");
    }
}
