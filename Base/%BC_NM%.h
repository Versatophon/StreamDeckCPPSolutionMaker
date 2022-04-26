//==============================================================================
/**
@file       %BC_NM%.h

@brief      %PL_NM% plugin

@copyright  %CP_RGT%

**/
//==============================================================================

#include "Common/ESDBasePlugin.h"
#include <mutex>

//class CpuUsageHelper;
class CallBackTimer;

class %BC_NM% : public ESDBasePlugin
{
public:
	
	%BC_NM%();
	virtual ~%BC_NM%();
	
	void KeyDownForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID) override;
	void KeyUpForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID) override;
	
	void WillAppearForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID) override;
	void WillDisappearForAction(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID) override;
	
	void DeviceDidConnect(const std::string& inDeviceID, const json &inDeviceInfo) override;
	void DeviceDidDisconnect(const std::string& inDeviceID) override;
	
	void SendToPlugin(const std::string& inAction, const std::string& inContext, const json &inPayload, const std::string& inDeviceID) override;

private:
	
	void UpdateTimer();
	
	std::mutex mVisibleContextsMutex;
	std::set<std::string> mVisibleContexts;
	
	int mInternalCounter;

	//CpuUsageHelper *mCpuUsageHelper = nullptr;
	CallBackTimer *mTimer;
};
