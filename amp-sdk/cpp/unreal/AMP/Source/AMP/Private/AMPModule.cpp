#include "Modules/ModuleManager.h"

DEFINE_LOG_CATEGORY_STATIC(LogAMP, Log, All);

class FAMPModule : public IModuleInterface
{
public:
    virtual void StartupModule() override
    {
        UE_LOG(LogAMP, Log, TEXT("AMP SDK module starting"));
    }

    virtual void ShutdownModule() override
    {
        UE_LOG(LogAMP, Log, TEXT("AMP SDK module shutting down"));
    }
};

IMPLEMENT_MODULE(FAMPModule, AMP)
